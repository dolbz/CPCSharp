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

        public DebugWindowViewModel(CPCRunner runner) {
            _runner =  runner;
            
            var timer = new Timer(75);
            timer.Elapsed += OnUpdate;
            timer.Start();
        }

        private void OnUpdate(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var snapshot = _runner.GetStateSnapshot();
            ProgramListing = snapshot.RamProgramListing.Select(x => new ProgramListingEntry{ AsmDescription = x }).ToList();
            LowerRomListing = snapshot.LowerRomProgramListing.Select(x => new ProgramListingEntry{ AsmDescription = x}).ToList();
            UpperRomListing = snapshot.UpperRomProgramListing.Select(x => new ProgramListingEntry{ AsmDescription = x}).ToList();
        }
    }
}
