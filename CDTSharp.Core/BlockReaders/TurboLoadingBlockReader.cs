using System.IO;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.BlockReaders
{
    public class TurboLoadingBlockReader : IBlockReader
    {   
        public int BlockId => 0x11;

        public IBlock ReadBlock(Stream blockStream)
        {
            /*  
                00 2  Length of PILOT pulse                                            [2168]
                02 2  Length of SYNC First pulse                                        [667]
                04 2  Length of SYNC Second pulse                                       [735]
                06 2  Length of ZERO bit pulse                                          [855]
                08 2  Length of ONE bit pulse                                          [1710]
                0A 2  Length of PILOT tone (in PILOT pulses)         [8064 Header, 3220 Data]
                0C 1  Used bits in last byte (other bits should be 0)                     [8]
                    i.e. if this is 6 then the bits (x) used in last byte are: xxxxxx00
                0D 2  Pause After this block in milliseconds (ms)                      [1000]
                0F 3  Length of following data
                12 x  Data; format is as for TAP (MSb first) 
            */

            var pilotPulseLength = blockStream.ReadTwoByteInt(ReadUnit.TCycles);
            var firstSyncPulseLength = blockStream.ReadTwoByteInt(ReadUnit.TCycles);
            var secondSyncPulseLength = blockStream.ReadTwoByteInt(ReadUnit.TCycles);
            var lengthOfZeroBitPulse = blockStream.ReadTwoByteInt(ReadUnit.TCycles);
            var lengthOfOneBitPulse = blockStream.ReadTwoByteInt(ReadUnit.TCycles);
            var lengthOfPilotTone = blockStream.ReadTwoByteInt(ReadUnit.None);
            var usedBitsInLastByte = blockStream.ReadByte();
            var pauseLengthAfterBlock = blockStream.ReadTwoByteInt(ReadUnit.Milliseconds);
            var lengthOfBlockData = blockStream.ReadThreeByteInt();

            var data = new byte[lengthOfBlockData];

            for (int i = 0; i < data.Length; i++) {
                data[i] = (byte)blockStream.ReadByte();
            }

            return new TurboLoadingBlock {
                PilotPulseLength = pilotPulseLength,
                FirstSyncPulseLength = firstSyncPulseLength,
                SecondSyncPulseLength = secondSyncPulseLength,
                LengthOfZeroBitPulse = lengthOfZeroBitPulse,
                LengthOfOneBitPulse = lengthOfOneBitPulse,
                LengthOfPilotTone = lengthOfPilotTone,
                UsedBitsInLastByte = (byte)usedBitsInLastByte,
                PauseLengthAfterBlock = pauseLengthAfterBlock,
                LengthOfBlockData = lengthOfBlockData,
                Data = data
            };
        }
    }
}