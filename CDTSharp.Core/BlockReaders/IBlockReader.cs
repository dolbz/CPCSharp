using System.IO;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.BlockReaders
{
    public interface IBlockReader {
        int BlockId { get; }

        IBlock ReadBlock(Stream blockStream);
    }
}