using System.Diagnostics;
using System.IO;
using System;
using Z80;
using Z80.Instructions;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

namespace CPCSharp.Core
{
    public class CPCRunner {
        private static bool _cpuRunning = false;
        private Z80Cpu _cpu = new Z80Cpu();
        private byte[] _ram = new byte[64*1024];

        private List<string> _lowerRomDisassembly = new List<string>();
        private List<string> _upperRomDisassembly = new List<string>();

        private byte[] _lowerRom = new byte[16*1024];
        private byte[] _upperRom = new byte[16*1024];

        private GateArray _gateArray = new GateArray();

        private List<IODevice> _ioDevices = new List<IODevice>();
        
        public void AccessCpuState(Action<Z80Cpu, byte[]> cpuAction) {
            lock(_cpu.CpuStateLock) {
                cpuAction(_cpu, _ram);
            }
        }

        public void Initialize() {
            _cpu.Initialize();
            LoadROMs();
            SetupIODevices();

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
            _lowerRomDisassembly.Clear();
            _lowerRomDisassembly = Disassemble(0, _lowerRom);

            offset = 0;
            using (var romStream = assembly.GetManifestResourceStream("CPCSharp.Core.BASIC_1.0.ROM")) {
                while (offset < romStream.Length) {
                    byte membyte = (byte)romStream.ReadByte();
                    _upperRom[offset++] = membyte;
                }
            }
            _upperRomDisassembly.Clear();
            _upperRomDisassembly = Disassemble(0xc000, _upperRom);           
        }

        private List<string> Disassemble(ushort initialOffset, byte[] binary) {
            var skipNextLookup = false;
            var listing = new List<string>();

            for (int i = 0; i < binary.Length; i++) {
                var description = "";

                if (skipNextLookup) {
                    skipNextLookup = false;
                } else {
                    var instruction = InstructionFor(i, binary);
                    if (instruction.byteCount == 2) {
                        skipNextLookup = true;
                    }
                    description = instruction.instruction?.Mnemonic ?? "";
                }
                var currentOffset = (ushort)(initialOffset+i);
                listing.Add($" 0x{currentOffset:x4}:    0x{binary[i]:x2}    {description}");  
            }

            return listing;
        }

        private void SetupIODevices() {
            _ioDevices.Add(_gateArray);
            _ioDevices.Add(new PPI());
        }

        private IODevice GetIoDeviceForAddress(ushort address) {
            foreach (var device in _ioDevices) {
                if (device.ActiveAtAddress(address)) {
                    return device;
                }
            }
            return null;
        }

        public MachineStateSnapshot GetStateSnapshot() {
            lock(_cpu.CpuStateLock) {
                var cpuSnapshot = _cpu.GetStateSnapshot();

                var pc = _cpu.PC;
                var startPc = (ushort)(pc-20);

                var listingBytes = new byte[50];

                for (int i = 0; i < 50; i++) {
                    var currentPc = (ushort)(startPc+i);
                    listingBytes[i] = _ram[currentPc];
                }
                var listing = Disassemble(startPc, listingBytes);

                return new MachineStateSnapshot(cpuSnapshot, listing, _lowerRomDisassembly, _upperRomDisassembly);
            }
        }

        private MemoryReadLocation GetCurrentReadLocationForAddress(ushort address) {
            if (_gateArray.LowerROMEnabled && address < 0x4000) {
                return new MemoryReadLocation(PhysicalMemoryComponent.LowerROM, address);
            } else if (_gateArray.UpperROMEnabled && _cpu.Address >= 0xc000) {
                return new MemoryReadLocation(PhysicalMemoryComponent.UpperROM, (ushort)(address & 0x3fff));
            } else { 
                return new MemoryReadLocation(PhysicalMemoryComponent.RAM, address);
            }
        }

        public (IInstruction instruction, int byteCount) InstructionFor(int addr, byte[] source) {
            int opcode = source[addr];
            var byteCount = 1;
            if (opcode == 0xcb || opcode == 0xdd || opcode == 0xed || opcode == 0xfd) {
                // Pickup next byte as this is a prefixed instruction
                opcode = opcode << 8 | source[addr + 1];
                byteCount = 2;
            }

            return (_cpu.instructions[opcode], byteCount);
        }

        public void RunCpu() {
            _cpuRunning = true;
            while (_cpuRunning) {
                // if (manuallyStepped) {
                //     instructionSignal.Set();
                //     uiSignal.WaitOne(); // Wait until the UI has signalled CPU execution can continue
                // }
                lock(_cpu.CpuStateLock) {
                    do {
                        _cpu.Clock();
                        if (_cpu.IORQ) {
                            var ioDevice = GetIoDeviceForAddress(_cpu.Address);
                            if (ioDevice != null) {
                                ioDevice.Address = _cpu.Address;
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
                            var readLocation = GetCurrentReadLocationForAddress(_cpu.Address);
                            switch (readLocation.Component)
                            {
                                case PhysicalMemoryComponent.LowerROM:
                                    _cpu.Data = _lowerRom[readLocation.ComponentLocalAddress];
                                    break;
                                case PhysicalMemoryComponent.UpperROM:
                                    _cpu.Data = _upperRom[readLocation.ComponentLocalAddress];
                                    break;
                                case PhysicalMemoryComponent.RAM:
                                    _cpu.Data = _ram[readLocation.ComponentLocalAddress];
                                    break;
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