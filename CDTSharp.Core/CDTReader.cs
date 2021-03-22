//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System;
using System.IO;
using System.Linq;
using CDTSharp.Core.BlockReaders;
using System.Collections.Generic;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core
{
    public class CDTReader {
        private static IBlockReader[] blockReaders = new IBlockReader[] {
            new PauseBlockReader(),
            new TurboLoadingBlockReader(),
            new PureDataBlockReader(),
            new GroupStartBlockReader(),
            new PureToneBlockReader(),
            new SequenceOfPulsesBlockReader(),
            new GroupEndBlockReader(),
            new StandardSpeedBlockReader()
        };

        public static CDTFile ReadCDTFile(string path) {
            using (var fileStream = File.OpenRead(path)) {
                var headerBuffer = new byte[8];
                var readByteCount = fileStream.Read(headerBuffer, 0, 8);

                if (readByteCount == 8) {
                    // Validate the header
                    var headerString = new string(headerBuffer.Take(7).Select(x => (char)x).ToArray());
                    if (!(headerString == "ZXTape!" && headerBuffer[7] == 0x1a)) {
                        throw new InvalidDataException();
                    }
                }
                var majorVersion = fileStream.ReadByte();
                var minorVersion = fileStream.ReadByte();

                var blocks = new List<IBlock>();
                while (fileStream.Position < fileStream.Length) {
                    var blockId = fileStream.ReadByte();
                    Console.WriteLine($"Found block with ID: 0x{blockId:x2}");

                    var blockReader = BlockReaderLookup(blockId);
                    var block = blockReader.ReadBlock(fileStream);
                    blocks.Add(block);
                    Console.WriteLine(block.Description);
                    Console.WriteLine();
                }

                return new CDTFile(
                    majorVersion, 
                    minorVersion, 
                    blocks);
            }
        }

        private static IBlockReader BlockReaderLookup(int blockId) {
            var blockReader = blockReaders.Where(x => x.BlockId == blockId).SingleOrDefault();

            if (blockReader == null) {
                Console.WriteLine($"No available block reader for block ID 0x{blockId:x2}");
            }

            return blockReader; 
        }
    }
}