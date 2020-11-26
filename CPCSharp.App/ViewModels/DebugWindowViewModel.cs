using System.Linq;
using System.Collections.Generic;
using CPCSharp.Core;
using ReactiveUI;
using System.Timers;

namespace CPCSharp.ViewModels
{
    public class ProgramListingEntry {
        public string AsmDescription { get; set; }
    }

    public class DebugWindowViewModel : ViewModelBase {
        private CPCRunner _runner;

        private List<ProgramListingEntry> _programListing;
        public List<ProgramListingEntry> ProgramListing {
            get => _programListing;
            set => this.RaiseAndSetIfChanged(ref _programListing, value);
        }

        private List<ProgramListingEntry> _lowerRomListing;
        public List<ProgramListingEntry> LowerRomListing {
            get => _lowerRomListing;
            set => this.RaiseAndSetIfChanged(ref _lowerRomListing, value);
        }

        private List<ProgramListingEntry> _upperRomListing;
        public List<ProgramListingEntry> UpperRomListing {
            get => _upperRomListing;
            set => this.RaiseAndSetIfChanged(ref _upperRomListing, value);
        }

        private int _selectedLowerRomIndex;
        private int SelectedLowerRomIndex {
            get => _selectedLowerRomIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedLowerRomIndex, value);
        }

        private int _selectedUpperRomIndex;
        private int SelectedUpperRomIndex {
            get => _selectedUpperRomIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedUpperRomIndex, value);
        }

        private int _selectedRamIndex;
        private int SelectedRamIndex {
            get => _selectedRamIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedRamIndex, value);
        }       

        private MachineStateSnapshot _snapshot;
        public MachineStateSnapshot Snapshot { 
            get => _snapshot;
            set => this.RaiseAndSetIfChanged(ref _snapshot, value);
        }

        public DebugWindowViewModel(CPCRunner runner) {
            _runner =  runner;
            
            var timer = new Timer(75);
            timer.Elapsed += OnUpdate;
            timer.Start();
        }

        public void Step() {
            _runner.Step();
        }

        public void Reset() {
            _runner.Reset();
        }

        private void OnUpdate(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var snapshot = _runner.GetStateSnapshot();
            Snapshot = snapshot;

            ProgramListing = snapshot.RamProgramListing.Select(x => new ProgramListingEntry{ AsmDescription = x }).ToList();
            if (LowerRomListing == null) {
                LowerRomListing = snapshot.LowerRomProgramListing.Select(x => new ProgramListingEntry{ AsmDescription = x}).ToList();
                UpperRomListing = snapshot.UpperRomProgramListing.Select(x => new ProgramListingEntry{ AsmDescription = x}).ToList();
            }

            SelectedLowerRomIndex = -1;
            SelectedUpperRomIndex = -1;
            SelectedRamIndex = -1;

            switch (snapshot.MemoryReadLocation.Component) {
                case PhysicalMemoryComponent.LowerROM:
                    SelectedLowerRomIndex = Snapshot.Cpu.PC;
                    break;
                case PhysicalMemoryComponent.UpperROM:
                    SelectedUpperRomIndex = Snapshot.Cpu.PC - 0xc000;
                    break;
                case PhysicalMemoryComponent.RAM:
                    SelectedRamIndex = 20; // Currently executing instruction is always at position 20 as the RAM list contains a snapshot of that area of RAM rather than the full contents
                    break;

            }
        }
    }
}
