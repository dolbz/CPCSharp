//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System;
using System.Linq;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.Blocks
{
    public class TurboLoadingBlock : IBlock {
        public int PilotPulseLength { get; init; }
        public int FirstSyncPulseLength { get; init; }
        public int SecondSyncPulseLength { get; init; }
        public int LengthOfZeroBitPulse { get; init; }
        public int LengthOfOneBitPulse { get; init; }
        public int LengthOfPilotTone { get; init; }
        public byte UsedBitsInLastByte { get; init; }
        public int PauseLengthAfterBlock { get; init; }
        public int LengthOfBlockData { get; init; }

        public byte[] Data { get; init; }

        private string BlockType {
            get {
                var zeroByte = Data[0];

                switch (zeroByte) {
                    case 0x2c:
                        return "Header";
                    case 0x16:
                        return "Data";
                    default:
                        return "Unknown/Invalid";
                }
            }
        }

        private bool IsHeader => Data[0] == 0x2c;

        public string Description {
            get {
                var desc = 
                       "Turbo Loading Block\n" +
                       "-------------------\n" +
                       $"PILOT pulse length: {PilotPulseLength}\n" +
                       $"First SYNC pulse length: {FirstSyncPulseLength}\n" +
                       $"Second SYNC pulse length: {SecondSyncPulseLength}\n" +
                       $"Length of ZERO bit pulse: {LengthOfZeroBitPulse}\n" +
                       $"Length of ONE bit pulse: {LengthOfOneBitPulse}\n" +
                       $"Length of PILOT tone: {LengthOfPilotTone}\n" +
                       $"Used bits in last byte: {UsedBitsInLastByte}\n" +
                       $"Pause length after block: {PauseLengthAfterBlock}\n" +
                       $"Length of data: {LengthOfBlockData}\n" +
                       $"Block type: {BlockType}";

                if (IsHeader) {
                    if (Data.Length >= 29) {
                        desc += "\n" +
                        "Header Data\n" +
                        "-----------\n" +
                        $"Filename: {new string(Data.Skip(1).Take(16).Select(x => Convert.ToChar(x)).ToArray())}\n" +
                        $"Block Number: {Data[17]}\n" +
                        $"Last Block?: {(Data[18] != 0 ? "True" : "False")}\n" +
                        $"File type: {Data[19]:x2}\n" +
                        $"Data length: {Data[21] << 8 | Data[20]}\n" +
                        $"Data Location: {Data[23] << 8 | Data[22]:x4}\n" +
                        $"First Block?: {(Data[24] != 0 ? "True" : "False")}\n" +
                        $"Logical Length: {Data[26] << 8 | Data[25]}\n" +
                        $"Entry address: {Data[28] << 8 | Data[27]:x4}";
                    } else {
                        desc += "\nHeader Data Truncated. Could be in a following pure data block";
                    }
                }

                return desc;
            }
        }
    }
}