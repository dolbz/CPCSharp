//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System.IO;
using CDTSharp.Core.BlockReaders;

namespace CDTSharp.Core
{
    public static class StreamExtensions {
        // Assumes little endian byte order
        public static int ReadTwoByteInt(this Stream stream, ReadUnit unit) {
            var lsb = stream.ReadByte();
            var msb = stream.ReadByte();

            var result = (msb << 8 | lsb);
            
            switch (unit) {
                case ReadUnit.Milliseconds:
                    return AdjustFromMilliseconds(result);
                case ReadUnit.TCycles:
                    return AdjustToNativeTCycles(result);
                case ReadUnit.None:
                default:
                    return result;
            }
        }

        public static int AdjustFromMilliseconds(int result) {
            // On CPC 1 millisecond is 4000 T cycles
            return result * 4000;
        }

        public static int AdjustToNativeTCycles(int result) {
            return (int)(result * (4/3.5));
        }

        public static int ReadThreeByteInt(this Stream stream) {
            var byte0 = stream.ReadByte();
            var byte1 = stream.ReadByte();
            var byte2 = stream.ReadByte();

            var result = byte2 << 16 | byte1 << 8 | byte0;

            return result;
        }

        public static string ReadString(this Stream stream, int length) {
            var characters = new char[length];
            for (int i = 0; i < length; i++) {
                characters[i] = (char)stream.ReadByte();
            }
            return new string(characters);
        }
    }
}