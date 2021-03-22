//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System.ComponentModel;
using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using CPCSharp.Core;
using ReactiveUI;
using System.Timers;
using CPCSharp.App.Views;
using Avalonia.Threading;
using CPCSharp.App.Models;

namespace CPCSharp.ViewModels
{
    public class ProgramListingEntry {
        public string AsmDescription { get; set; }
    }

    public class DebugWindowViewModel : ViewModelBase, INotifyPropertyChanged {
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

        private int _selectedBreakpointIndex = -1;
        private int SelectedBreakpointIndex {
            get => _selectedBreakpointIndex;
            set => this.RaiseAndSetIfChanged(ref _selectedBreakpointIndex, value);
        }

        private string _newBreakpointAddress;
        public string NewBreakpointAddress {
            get => _newBreakpointAddress;
            set => this.RaiseAndSetIfChanged(ref _newBreakpointAddress, value);
        }

        private List<Breakpoint> _breakpoints = new List<Breakpoint>();
        public List<Breakpoint> Breakpoints {
            get => _breakpoints;
            set => this.RaiseAndSetIfChanged(ref _breakpoints, value);
        }  

        private MachineStateSnapshot _snapshot;
        public MachineStateSnapshot Snapshot { 
            get => _snapshot;
            set => this.RaiseAndSetIfChanged(ref _snapshot, value);
        }

        public DebugWindowViewModel(CPCRunner runner) {
            _runner =  runner;
            var timer = new Timer(125);
            timer.Elapsed += OnUpdate;
            timer.Start();
        }

        public void Step() {
            _runner.Step();
        }

        public void Continue() {
            _runner.Continue();
        }

        public void StepOver() {
            _runner.StepOver();
        }

        public void Reset() {
            _runner.Reset();
        }

        public void AddBreakpoint() {
            var cleanValue = (_newBreakpointAddress ?? string.Empty).Trim().Replace("0x", string.Empty);

            if (cleanValue.Length <= 4) {
                ushort bpAddress;
                if (ushort.TryParse(cleanValue, NumberStyles.HexNumber, null, out bpAddress)) {
                    NewBreakpointAddress = string.Empty;
                    _runner.AddRamBreakpoint(bpAddress);
                    
                    var bp = new Breakpoint {
                        Address = bpAddress
                    };

                    var updatedList = new List<Breakpoint>(Breakpoints);
                    updatedList.Add(bp);
                    Breakpoints = updatedList;
                    
                    // TODO why doesn't this work?
                    //Breakpoints.Add(bpAddress);
                    //Dispatcher.UIThread.Post(() => this.RaisePropertyChanged(nameof(Breakpoints)));
                }
            }
        }

        public void RemoveBreakpoint() {
            if (SelectedBreakpointIndex >= 0 && SelectedBreakpointIndex < Breakpoints.Count) {
                var bp = Breakpoints[SelectedBreakpointIndex];
                _runner.RemoveRamBreakpoint(bp.Address);
                var newBreakpoints = new List<Breakpoint>(Breakpoints);
                newBreakpoints.Remove(bp);
                Breakpoints = newBreakpoints;
            }
        }
        
        public void MemoryView() {
            new MemoryNavigator() {
                DataContext = new MemoryNavigatorViewModel(_runner.Ram)
            }.Show();
        }

        private void OnUpdate(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var snapshot = _runner.GetStateSnapshot();
            Snapshot = snapshot;

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
                    ProgramListing = snapshot.RamProgramListing.Select(x => new ProgramListingEntry{ AsmDescription = x }).ToList();
                    SelectedRamIndex = 20; // Currently executing instruction is always at position 20 as the RAM list contains a snapshot of that area of RAM rather than the full contents
                    break;

            }
        }
    }
}
