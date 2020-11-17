using System.IO;
using System;
using Z80;
using System.Threading;
using System.Reflection;

namespace CPCSharp.Core
{
    public class CPCRunner {
        private static bool _cpuRunning = false;
        private Z80Cpu _cpu = new Z80Cpu();
        private byte[] _ram = new byte[64*1024];
        private readonly static object cpuState = new object();
        
        public CPCRunner() {
            // TODO shouldn't do this in a construction
            _cpu.Initialize();
            ThreadStart work = RunCpu;
            Thread thread = new Thread(work);
            thread.Start();
        }
        public void AccessCpuState(Action<Z80Cpu, byte[]> cpuAction) {
            lock(cpuState) {
                cpuAction(_cpu, _ram);
            }
        }

        public void Initialize() {
            LoadROM();
        }

        private  void LoadROM() {
            var offset = 0;
            var assembly = typeof(CPCRunner).GetTypeInfo().Assembly;
             
            using (var romStream = assembly.GetManifestResourceStream("CPCSharp.Core.OS_464.ROM")) {
                while (offset < romStream.Length) {
                    byte membyte = (byte)romStream.ReadByte();
                    _ram[offset++] = membyte;
                }
                //Console.WriteLine($"Wrote {offset - startOffset} bytes to RAM");
            }            
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
                        if (_cpu.MREQ && _cpu.RD) {
                            var data = _ram[_cpu.Address];
                            _cpu.Data = data;
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