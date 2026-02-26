//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.Blocks
{
    public class PureDataBlock : IBlock {
        public required int LengthOfZeroBitPulse { get; init; }
        public required int LengthOfOneBitPulse { get; init; }
        public required byte UsedBitsInLastByte { get; init; }
        public required int PauseLengthAfterBlock { get; init; }
        public required int LengthOfBlockData { get; init; }

        public required long DataCdtOffset { get; init; }

        public required byte[] Data { get; init; }

        public string Description {
            get {
                return "Pure Data Block\n" +
                       "---------------\n" +
                       $"Length of ZERO bit pulse: {LengthOfZeroBitPulse}\n" +
                       $"Length of ONE bit pulse: {LengthOfOneBitPulse}\n" +
                       $"Used bits in last byte: {UsedBitsInLastByte}\n" +
                       $"Pause length after block: {PauseLengthAfterBlock}\n" +
                       $"Length of data: {LengthOfBlockData}\n" +
                       $"Data offset in CDT: 0x{DataCdtOffset:x4}";
            }
        }
    }
}