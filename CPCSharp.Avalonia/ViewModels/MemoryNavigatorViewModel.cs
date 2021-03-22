//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using ReactiveUI;

namespace CPCSharp.ViewModels
{
    public class MemoryNavigatorViewModel : ViewModelBase {
        private byte[] _ram;

        private List<string> _ramListing;
        public List<string> RamListing {
            get => _ramListing;
            private set => this.RaiseAndSetIfChanged(ref _ramListing, value);
        }

        private Task _populateListingTask;

        public MemoryNavigatorViewModel(byte[] ram) {
            _ram = ram;
            _populateListingTask = Task.Run(PopulateListing);
        }

        public void PopulateListing() {
            var listing = new List<string>();

            var currentLineBytes = new StringBuilder();
            var currentLineStringRepresentation = new StringBuilder();

            var currentLineOffset = 0;

            for (int i = 0; i < _ram.Length; i++) {
                if (i != 0 && i % 32 == 0) {
                    listing.Add($"{currentLineOffset:x4}: " + currentLineBytes.ToString() + " " + currentLineStringRepresentation.ToString());
                    currentLineOffset = i;
                    currentLineBytes.Clear();
                    currentLineStringRepresentation.Clear();
                }
                var ramValue = _ram[i];
                currentLineBytes.Append($"{ramValue:x2} ");

                
                char strRepresentation = ramValue >= 0x20 && ramValue < 0x7f ? (char)ramValue : '.';

                currentLineStringRepresentation.Append(strRepresentation);
            }

            RamListing = listing;
        }

        public async Task DumpRam() {
            Console.WriteLine("Dumping RAM");
            await File.WriteAllBytesAsync("dump.bin", _ram);
        }
    }
}