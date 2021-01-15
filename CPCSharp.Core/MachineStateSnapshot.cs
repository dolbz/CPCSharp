using System.Collections.Generic;
using CPCSharp.Core.Debugging;
using Z80;

namespace CPCSharp.Core {
    public struct MachineStateSnapshot {

        public MachineStateSnapshot(
            bool waitingOnBreakpoint,
            Z80CpuSnapshot cpuSnapshot,
            GateArraySnapshot gateArraySnapshot,
            List<string> ramProgramListing,
            List<string> lowerRomProgramListing,
            List<string> upperRomProgramListing,
            MemoryReadLocation currentMemoryReadLocation,
            List<ushort> stackContents,
            double cpuFrequencyMHz) 
        {
            this.WaitingOnBreakpoint = waitingOnBreakpoint;
            this.Cpu = cpuSnapshot;
            this.GateArray = gateArraySnapshot;
            this.RamProgramListing = ramProgramListing;
            this.LowerRomProgramListing = lowerRomProgramListing;
            this.UpperRomProgramListing = upperRomProgramListing;
            this.MemoryReadLocation = currentMemoryReadLocation;
            this.Stack = stackContents;
            this.FrequencyMHz = cpuFrequencyMHz;
        }

        public bool WaitingOnBreakpoint { get; }

        public List<string> RamProgramListing { get; }
        public List<string> LowerRomProgramListing { get; }
        public List<string> UpperRomProgramListing { get; }

        public MemoryReadLocation MemoryReadLocation { get; }

        public List<ushort> Stack { get; }

        public Z80CpuSnapshot Cpu { get; }

        public GateArraySnapshot GateArray { get;}

        public double FrequencyMHz { get;}
    }
}