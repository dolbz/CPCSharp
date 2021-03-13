using System.Diagnostics;
using System;
using Z80;
using Z80.Instructions;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using CPCSharp.Core.Interfaces;
using CDTSharp.Core.Playback;
using CDTSharp.Core;
using CPCSharp.Core.Debugging;
using System.Runtime.CompilerServices;
using CPCSharp.Core.PSG;

namespace CPCSharp.Core
{
    public struct CycleCountObservation
    {
        public double ElapsedMilliseconds { get; set; }
        public long Count { get; set; }
    }

    public class CPCRunner
    {
        private static bool _cpuRunning = false;
        private Z80Cpu _cpu = new Z80Cpu();
        private byte[] _ram = new byte[64 * 1024];

        public byte[] Ram => _ram;

        private List<string> _lowerRomDisassembly = new List<string>();
        private List<string> _upperRomDisassembly = new List<string>();

        private byte[] _lowerRom = new byte[16 * 1024];
        private byte[] _upperRom = new byte[16 * 1024];

        private readonly GateArray _gateArray;

        private readonly CRTC _crtc;
        private readonly PPI _ppi;

        private PlayableTape _tape;
        private List<IODevice> _ioDevices = new List<IODevice>();

        private AutoResetEvent _cpuBreakingSignal = new AutoResetEvent(false);
        private AutoResetEvent _uiCompleteSignal = new AutoResetEvent(false);

        private List<CPCBreakpoint> _breakPoints = new List<CPCBreakpoint>();
        private List<ushort> _ramBreakpoints = new List<ushort>();
        private bool _breakpointHit;
        private uint _systemClockCycles = 0;
        private Stopwatch stopwatch = Stopwatch.StartNew();
        private CycleCountObservation previousObservation = new CycleCountObservation();
        private CycleCountObservation currentObservation = new CycleCountObservation();

        public CPCRunner(IScreenRenderer renderer, INativePSG psg)
        {
            _gateArray = new GateArray(renderer);
            _crtc = new CRTC(renderer, _gateArray);
            _ppi = new PPI(_crtc, psg);
        }

        public void LoadTape(string path)
        {
            _tape = new PlayableTape(CDTReader.ReadCDTFile(path));
        }

        public void RewindTape()
        {
            _tape?.Rewind();
        }

        public void AddRamBreakpoint(ushort addr)
        {
            lock (_cpu.CpuStateLock)
            {
                if (!_ramBreakpoints.Contains(addr))
                {
                    _ramBreakpoints.Add(addr);
                }
            }
        }

        public void RemoveRamBreakpoint(ushort addr)
        {
            lock (_cpu.CpuStateLock)
            {
                if (_ramBreakpoints.Contains(addr))
                {
                    _ramBreakpoints.Remove(addr);
                }
            }
        }

        public void AccessCpuState(Action<Z80Cpu, byte[]> cpuAction)
        {
            lock (_cpu.CpuStateLock)
            {
                cpuAction(_cpu, _ram);
            }
        }

        public void Initialize(bool spawnThread = true)
        {
            _cpu.Initialize();
            LoadROMs();
            SetupIODevices();
            
            if (spawnThread)
            {
                ThreadStart work = RunCpuContinous;
                Thread thread = new Thread(work);
                thread.Start();
            }
        }

        public void Shutdown()
        {
            _cpuRunning = false;
        }

        public void Step()
        {
            lock (_cpu.CpuStateLock)
            {
                var id = Guid.NewGuid();
                _breakpointHit = false;
                var step = new StepBreakpoint();
                _breakPoints.Add(step);
            }
            _uiCompleteSignal.Set();
        }

        public void Continue()
        {
            lock (_cpu.CpuStateLock)
            {
                _breakpointHit = false;
            }
            _uiCompleteSignal.Set();
        }

        public void StepOver()
        {
            lock (_cpu.CpuStateLock)
            {
                var id = Guid.NewGuid();
                var snapshot = GetStateSnapshot();
                _breakpointHit = false;
                var stepOver = new StepOverBreakpoint(snapshot);
                _breakPoints.Add(stepOver);
            }
            _uiCompleteSignal.Set();
        }

        public void Reset()
        {
            lock (_cpu.CpuStateLock)
            {
                _breakpointHit = false;
                _cpu.Reset();
                _ram = new byte[64 * 1024];
            }
            _uiCompleteSignal.Set();
        }

        private void LoadROMs()
        {
            var offset = 0;
            var assembly = typeof(CPCRunner).GetTypeInfo().Assembly;

            using (var romStream = assembly.GetManifestResourceStream("CPCSharp.Core.OS_464.ROM"))
            {
                while (offset < romStream.Length)
                {
                    byte membyte = (byte)romStream.ReadByte();
                    _lowerRom[offset++] = membyte;
                }
            }
            _lowerRomDisassembly.Clear();
            _lowerRomDisassembly = Disassemble(0, _lowerRom);

            offset = 0;
            using (var romStream = assembly.GetManifestResourceStream("CPCSharp.Core.BASIC_1.0.ROM"))
            {
                while (offset < romStream.Length)
                {
                    byte membyte = (byte)romStream.ReadByte();
                    _upperRom[offset++] = membyte;
                }
            }
            _upperRomDisassembly.Clear();
            _upperRomDisassembly = Disassemble(0xc000, _upperRom);
        }

        private List<string> Disassemble(ushort initialOffset, byte[] binary)
        {
            var skipNextLookup = false;
            var listing = new List<string>();

            for (int i = 0; i < binary.Length; i++)
            {
                var description = "";

                if (skipNextLookup)
                {
                    skipNextLookup = false;
                }
                else
                {
                    var instruction = InstructionFor(i, binary);
                    if (instruction.byteCount == 2)
                    {
                        skipNextLookup = true;
                    }
                    description = instruction.instruction?.Mnemonic ?? "";
                }
                var currentOffset = (ushort)(initialOffset + i);
                listing.Add($" 0x{currentOffset:x4}:    0x{binary[i]:x2}    {description}");
            }

            return listing;
        }

        private void SetupIODevices()
        {
            _ioDevices.Add(_crtc);
            _ioDevices.Add(_gateArray);
            _ioDevices.Add(_ppi);
        }

        private IODevice GetIoDeviceForAddress(ushort address)
        {
            foreach (var device in _ioDevices)
            {
                if (device.ActiveAtAddress(address))
                {
                    return device;
                }
            }
            return null;
        }

        public MachineStateSnapshot GetStateSnapshot()
        {
            lock (_cpu.CpuStateLock)
            {
                var cpuSnapshot = _cpu.GetStateSnapshot();
                var gateArraySnapshot = GateArraySnapshot.SnapShotFromGateArray(_gateArray);

                var pc = cpuSnapshot.PC;
                var startPc = (ushort)(pc - 20);

                var listingBytes = new byte[50];

                for (int i = 0; i < 50; i++)
                {
                    var currentPc = (ushort)(startPc + i);
                    listingBytes[i] = _ram[currentPc];
                }
                var listing = Disassemble(startPc, listingBytes);

                var sp = cpuSnapshot.SP;

                var stackContents = new List<ushort>();
                for (int i = 0; i < 40; i += 2)
                {
                    var currentSp = (ushort)(sp + i);

                    if (currentSp >= 0xc000)
                    { // CPC normal base of stack
                        break;
                    }
                    stackContents.Add((ushort)(_ram[currentSp + 1] << 8 | _ram[currentSp]));
                }

                var timeDelta = currentObservation.ElapsedMilliseconds - previousObservation.ElapsedMilliseconds;
                var cycleCountDelta = currentObservation.Count - previousObservation.Count;

                var calculatedMhzFrequency = (cycleCountDelta / timeDelta) / 1_000;

                return new MachineStateSnapshot(
                        _breakpointHit,
                        cpuSnapshot,
                        gateArraySnapshot,
                        listing,
                        _lowerRomDisassembly,
                        _upperRomDisassembly,
                        GetCurrentReadLocationForAddress(cpuSnapshot.PC),
                        stackContents,
                        calculatedMhzFrequency);
            }
        }

        private MemoryReadLocation GetCurrentReadLocationForAddress(ushort address)
        {
            if (_gateArray.LowerROMEnabled && address < 0x4000)
            {
                return new MemoryReadLocation(PhysicalMemoryComponent.LowerROM, address);
            }
            else if (_gateArray.UpperROMEnabled && _cpu.Address >= 0xc000)
            {
                return new MemoryReadLocation(PhysicalMemoryComponent.UpperROM, (ushort)(address & 0x3fff));
            }
            else
            {
                return new MemoryReadLocation(PhysicalMemoryComponent.RAM, address);
            }
        }

        public (IInstruction instruction, int byteCount) InstructionFor(int addr, byte[] source)
        {
            int opcode = source[addr];
            var byteCount = 1;
            if (opcode == 0xcb || opcode == 0xdd || opcode == 0xed || opcode == 0xfd)
            {
                // Pickup next byte as this is a prefixed instruction
                if (addr + 1 < source.Length)
                {
                    opcode = opcode << 8 | source[addr + 1];
                    byteCount = 2;
                }
            }

            return (_cpu.instructions[opcode], byteCount);
        }

        public void RunCpuContinous()
        {
            _cpuRunning = true;
            while (_cpuRunning)
            {
                if (_breakpointHit)
                {
                    //_cpuBreakingSignal.Set();
                    _uiCompleteSignal.WaitOne(); // Wait until the UI has signalled CPU execution can continue
                }
                lock (_cpu.CpuStateLock)
                {
                    do
                    {
                        RunCpuCycle();
                    } while (!_cpu.NewInstruction && !_breakpointHit);

                    CPCBreakpoint bpToRemove = null;
                    foreach (var bp in _breakPoints)
                    {
                        if (bp.Hit(GetStateSnapshot()))
                        {
                            _breakpointHit = true;
                            if (bp.SingleHit)
                            {
                                bpToRemove = bp;
                            }
                            break;
                        }
                    }

                    if (bpToRemove != null)
                    {
                        _breakPoints.Remove(bpToRemove);
                    }
                }
            }
            //instructionSignal.Set();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RunCpuCycle()
        {
            if (_systemClockCycles > currentObservation.Count + 1000)
            {
                // We want to compare over a long period but adjust in much shorter periods
                // So this code runs every 1000 cycles but compares against a time period 
                // of 50000 cycles
                if (_systemClockCycles % 50000 == 0)
                {
                    previousObservation = currentObservation;
                }
                double calculatedMhzFrequency = 0;
                do
                {
                    currentObservation = new CycleCountObservation
                    {
                        ElapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds,
                        Count = _systemClockCycles
                    };

                    var timeDelta = currentObservation.ElapsedMilliseconds - previousObservation.ElapsedMilliseconds;
                    var cycleCountDelta = currentObservation.Count - previousObservation.Count;

                    calculatedMhzFrequency = (cycleCountDelta / timeDelta) / 1000;
                    //Console.WriteLine($"Calculated MHz {calculatedMhzFrequency}");
                } while (calculatedMhzFrequency > 16);
            }

            _systemClockCycles++;
            _gateArray.Clock();

            _gateArray.HSYNC = _crtc.HSYNC;
            _gateArray.VSYNC = _crtc.VSYNC;
            _gateArray.DISPEN = _crtc.DISP;
            _gateArray.M1 = _cpu.M1;
            _gateArray.IORQ = _cpu.IORQ;

            _cpu.INT = _gateArray.INTERRUPT;
            _cpu.WAIT = !_gateArray.READY;

            if (!_gateArray.CPUADDR)
            {
                var ra0to2 = (_crtc.RowAddress & 0x7) << 11;
                var ma12to13 = (_crtc.MemoryAddress & 0x3000) << 2;
                var ma0to9 = (_crtc.MemoryAddress & 0x3ff) << 1;

                if (_gateArray.CCLK)
                {
                    _gateArray.Address = (ushort)(ma0to9 | ma12to13 | ra0to2 | 0x1); // TODO The gate array doens't even have an address
                    _gateArray.Data = _ram[_gateArray.Address];
                    _crtc.Clock();
                }
                else
                {
                    _gateArray.Address = (ushort)(ma0to9 | ma12to13 | ra0to2); // TODO The gate array doesn't even have an address
                    _gateArray.Data = _ram[_gateArray.Address];
                }
            }

            if (_gateArray.CpuClock)
            {
                // if (_cpu.NewInstruction) {
                //     _pcQueue.enqueue();
                // }
                if (_ramBreakpoints.Count > 0 && _cpu.NewInstruction && _ramBreakpoints.Contains(_cpu.PC))
                {
                    _breakpointHit = true;
                }

                _cpu.Clock();
                if (_ppi.TapeMotorOn)
                {
                    _ppi.CassetteIn = _tape?.ClockTick() ?? false;
                }
                if (_cpu.IORQ && !_cpu.WAIT)
                {
                    var ioDevice = GetIoDeviceForAddress(_cpu.Address);
                    if (ioDevice != null)
                    {
                        ioDevice.Address = _cpu.Address;
                        if (_cpu.RD)
                        {
                            _cpu.Data = ioDevice.Data;
                        }
                        else if (_cpu.WR)
                        {
                            // write to an IO device
                            ioDevice.Data = _cpu.Data;
                        }
                    }
                    else
                    {
                        //Console.WriteLine($"IORQ {(_cpu.RD ? "read" : "write")} for unknown IO address: {_cpu.Address:x4}");
                    }
                }
                if (_cpu.MREQ && _cpu.RD)
                {
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
                if (_cpu.MREQ && _cpu.WR)
                {
                    _ram[_cpu.Address] = _cpu.Data;
                }
            }
        }
    }
}