using System.Collections.Generic;
using Z80;

namespace CPCSharp.Core {
    public class MachineStateSnapshot {

        public MachineStateSnapshot(
            bool waitingOnBreakpoint,
            Z80CpuSnapshot cpuSnapshot, 
            List<string> ramProgramListing,
            List<string> lowerRomProgramListing,
            List<string> upperRomProgramListing,
            MemoryReadLocation currentMemoryReadLocation,
            List<byte> stackContents,
            double cpuFrequencyMHz) 
        {
            this.WaitingOnBreakpoint = waitingOnBreakpoint;
            this.Cpu = cpuSnapshot;
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

        public List<byte> Stack { get; }

        public Z80CpuSnapshot Cpu { get; }

        public double FrequencyMHz { get;}
    }
}