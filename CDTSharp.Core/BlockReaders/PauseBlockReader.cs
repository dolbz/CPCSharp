//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System;
using System.IO;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.BlockReaders
{
    public enum ReadUnit {
        Milliseconds,
        TCycles,
        None
    }
    public class PauseBlockReader : IBlockReader {
        public int BlockId => 0x20;

        public IBlock ReadBlock(Stream blockStream) {
            var pauseLength = blockStream.ReadTwoByteInt(ReadUnit.Milliseconds);

            return new PauseBlock {
                PauseLength = pauseLength
            };
        }
    }
}