//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System.IO;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.BlockReaders
{
    public class PureToneBlockReader : IBlockReader {
        public int BlockId => 0x12;

        public IBlock ReadBlock(Stream blockStream) {
            /* 00 2  Length of pulse in T-States
               02 2  Number of pulses */

            var pulseLength = blockStream.ReadTwoByteInt(ReadUnit.TCycles);
            var noOfpulses = blockStream.ReadTwoByteInt(ReadUnit.None);
            return new PureToneBlock {
                NumberOfPulses = noOfpulses,
                PulseLength = pulseLength
            };
        }
    }
}