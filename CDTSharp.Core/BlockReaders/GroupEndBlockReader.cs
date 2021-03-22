//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System.IO;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.BlockReaders
{
    public class GroupEndBlockReader : IBlockReader {
        public int BlockId => 0x22;

        public IBlock ReadBlock(Stream blockStream) {
            return new GroupEndBlock();
        }
    }
}