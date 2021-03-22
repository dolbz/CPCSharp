//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System.Linq;
namespace CDTSharp.Core.Blocks
{
    public class SequenceOfPulsesBlock : IBlock {
        public int[] PulseLengths { get; set; }

        public string Description { 
            get 
            {
                return "Sequence Of Pulses Of Different Lengths\n" +
                       "---------------------------------------\n" +
                       $"{PulseLengths.Length} pulses.\n" +
                       $"[ {string.Concat(PulseLengths.Select(x => $"{x},\n"))} ]\n";
            }
        }
    }
}