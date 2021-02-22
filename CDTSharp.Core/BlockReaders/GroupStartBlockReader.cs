using System.IO;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.BlockReaders
{
    public class GroupStartBlockReader : IBlockReader {
        public int BlockId => 0x21;

        public IBlock ReadBlock(Stream blockStream) {
            var groupNameLength = blockStream.ReadByte();

            return new GroupStartBlock {
                GroupName = blockStream.ReadString(groupNameLength)
            };
        }
    }
}