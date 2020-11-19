using System.Diagnostics;
using System.IO;
using System;
using Z80;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

namespace CPCSharp.Core
{
    public class CPCRunner {
        private static bool _cpuRunning = false;
        private Z80Cpu _cpu = new Z80Cpu();
        private byte[] _ram = new byte[64*1024];

        private byte[] _lowerRom = new byte[16*1024];
        private byte[] _upperRom = new byte[16*1024];

        private GateArray _gateArray = new GateArray();

        private List<IODevice> _ioDevices = new List<IODevice>();
        private readonly static object cpuState = new object();
        
        public void AccessCpuState(Action<Z80Cpu, byte[]> cpuAction) {
            lock(cpuState) {
                cpuAction(_cpu, _ram);
            }
        }

        public void Initialize() {
            LoadROMs();
            SetupIODevices();

            _cpu.Initialize();
            ThreadStart work = RunCpu;
            Thread thread = new Thread(work);
            thread.Start();
        }

        private void LoadROMs() {
            var offset = 0;
            var assembly = typeof(CPCRunner).GetTypeInfo().Assembly;
             
            using (var romStream = assembly.GetManifestResourceStream("CPCSharp.Core.OS_464.ROM")) {
                while (offset < romStream.Length) {
                    byte membyte = (byte)romStream.ReadByte();
                    _lowerRom[offset++] = membyte;
                }
            }
            using (var romStream = assembly.GetManifestResourceStream("CPCSharp.Core.BASIC_1.0.ROM")) {
                while (offset < romStream.Length) {
                    byte membyte = (byte)romStream.ReadByte();
                    _upperRom[offset++] = membyte;
                }
            }            
        }

        private void SetupIODevices() {
            _ioDevices.Add(_gateArray);
        }

        private IODevice GetIoDeviceForAddress(ushort address) {
            foreach (var device in _ioDevices) {
                if (device.ActiveAtAddress(address)) {
                    return device;
                }
            }
            return null;
        }

        public void RunCpu() {
            _cpuRunning = true;
            while (_cpuRunning) {
                // if (manuallyStepped) {
                //     instructionSignal.Set();
                //     uiSignal.WaitOne(); // Wait until the UI has signalled CPU execution can continue
                // }
                lock(cpuState) {
                    do {
                        _cpu.Clock();
                        if (_cpu.IORQ) {
                            var ioDevice = GetIoDeviceForAddress(_cpu.Address);
                            if (ioDevice != null) {
                                if (_cpu.RD) {
                                    _cpu.Data = ioDevice.Data;
                                }
                                else if (_cpu.WR) {
                                    // write to an IO device
                                    ioDevice.Data = _cpu.Data;
                                }
                            } else {
                                Console.WriteLine($"IORQ for unknown IO address: {_cpu.Address:x4}");
                            }
                        }
                        if (_cpu.MREQ && _cpu.RD) {
                            if (_gateArray.LowerROMEnabled && _cpu.Address < 0x4000) {
                                _cpu.Data = _lowerRom[_cpu.Address];
                            } else if (_gateArray.UpperROMEnabled && _cpu.Address >= 0xc000) {
                                _cpu.Data = _upperRom[_cpu.Address & 0x0fff];
                            } else {
                                _cpu.Data = _ram[_cpu.Address];;
                            }
                        }
                        if (_cpu.MREQ && _cpu.WR) {
                            _ram[_cpu.Address] = _cpu.Data;
                        }
                    } while(!_cpu.NewInstruction);
                }
            }
            //instructionSignal.Set();
        }
    }
}