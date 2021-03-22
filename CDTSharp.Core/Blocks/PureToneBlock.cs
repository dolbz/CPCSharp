//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace CDTSharp.Core.Blocks
{
    public class PureToneBlock : IBlock {
        public int PulseLength { get; init; }
        public int NumberOfPulses { get; init; }

        public string Description { 
            get 
            {
                return "Pure Tone Block\n" +
                       "---------------\n" +
                       $"{NumberOfPulses} pulses of {PulseLength} T cycles long";
            }
        }
    }
}