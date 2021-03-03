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