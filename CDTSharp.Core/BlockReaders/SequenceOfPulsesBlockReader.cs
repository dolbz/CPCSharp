using System.IO;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.BlockReaders
{
    public class SequenceOfPulsesBlockReader : IBlockReader {
        public int BlockId => 0x13;

        public IBlock ReadBlock(Stream blockStream) {
            /* 00 1  Number of pulses
               01 2  Length of first pulse in T-States
               03 2  Length of second pulse...
               .. .  etc. */

            var numberOfPulses = blockStream.ReadByte();
            var pulseLengths = new int[numberOfPulses];
            for (int i = 0; i < numberOfPulses; i++) {
                pulseLengths[i] = blockStream.ReadTwoByteInt(ReadUnit.TCycles);
            }

            return new SequenceOfPulsesBlock {
                PulseLengths = pulseLengths
            };
        }
    }
}