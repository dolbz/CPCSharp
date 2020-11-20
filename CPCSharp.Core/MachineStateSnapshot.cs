using System.Collections.Generic;
using Z80;

namespace CPCSharp.Core {
    public class MachineStateSnapshot {

        public MachineStateSnapshot(Z80CpuSnapshot cpuSnapshot, List<string> ramProgramListing, List<string> lowerRomProgramListing, List<string> upperRomProgramListing) {
            this.CpuSnapshot = cpuSnapshot;
            this.RamProgramListing = ramProgramListing;
            this.LowerRomProgramListing = lowerRomProgramListing;
            this.UpperRomProgramListing = upperRomProgramListing;
        }
        public List<string> RamProgramListing { get; }
        public List<string> LowerRomProgramListing { get; }
        public List<string> UpperRomProgramListing { get; }

        public Z80CpuSnapshot CpuSnapshot { get; }
    }
}