//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System.IO;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.BlockReaders
{
    public class StandardSpeedBlockReader : IBlockReader
    {
        public int BlockId => 0x10;

        public IBlock ReadBlock(Stream blockStream)
        {
            /*
                00 2  Pause After this block in milliseconds (ms)                      [1000]
                02 2  Length of following data
                04 x  Data, as in .TAP File
            */
            var pauseLengthAfterBlock = blockStream.ReadTwoByteInt(ReadUnit.Milliseconds);
            var lengthOfBlockData = blockStream.ReadTwoByteInt(ReadUnit.None);

            var data = new byte[lengthOfBlockData];

            for (int i = 0; i < data.Length; i++) {
                data[i] = (byte)blockStream.ReadByte();
            } 
            
            return new StandardSpeedBlock {
                PauseLengthAfterBlock = pauseLengthAfterBlock,
                LengthOfBlockData = lengthOfBlockData,
                Data = data
            };
        }
    }
}