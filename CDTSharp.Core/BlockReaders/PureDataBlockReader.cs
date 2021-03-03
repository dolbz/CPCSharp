using System.IO;
using CDTSharp.Core.BlockReaders;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.BlockReaders
{
    public class PureDataBlockReader : IBlockReader
    {   
        public int BlockId => 0x14;

        public IBlock ReadBlock(Stream blockStream)
        {
            /*  
                00 2  Length of ZERO bit pulse
                02 2  Length of ONE bit pulse
                04 1  Used bits in LAST Byte
                05 2  Pause after this block in milliseconds (ms)
                07 3  Length of following data
                0A x  Data (MSb first)
            */

            var lengthOfZeroBitPulse = blockStream.ReadTwoByteInt(ReadUnit.TCycles);
            var lengthOfOneBitPulse = blockStream.ReadTwoByteInt(ReadUnit.TCycles);
            var usedBitsInLastByte = blockStream.ReadByte();
            var pauseLengthAfterBlock = blockStream.ReadTwoByteInt(ReadUnit.Milliseconds);
            var lengthOfBlockData = blockStream.ReadThreeByteInt();

            var data = new byte[lengthOfBlockData];

            var dataCdtOffset = blockStream.Position;
            for (int i = 0; i < data.Length; i++) {
                data[i] = (byte)blockStream.ReadByte();
            }

            return new PureDataBlock {
                LengthOfZeroBitPulse = lengthOfZeroBitPulse,
                LengthOfOneBitPulse = lengthOfOneBitPulse,
                UsedBitsInLastByte = (byte)usedBitsInLastByte,
                PauseLengthAfterBlock = pauseLengthAfterBlock,
                LengthOfBlockData = lengthOfBlockData,
                Data = data,
                DataCdtOffset = dataCdtOffset,
            };
        }
    }
}