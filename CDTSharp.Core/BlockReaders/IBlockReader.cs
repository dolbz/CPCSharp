//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System.IO;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.BlockReaders
{
    public interface IBlockReader {
        int BlockId { get; }

        IBlock ReadBlock(Stream blockStream);
    }
}