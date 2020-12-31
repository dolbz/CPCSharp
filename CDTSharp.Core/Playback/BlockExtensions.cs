using System;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.Playback
{
    public static class BlockExtensions {
        public static IBlockPlayer CreateBlockPlayer(this IBlock block, bool initialState) {
            switch (block) {
                case TurboLoadingBlock tb:
                    return new TurboLoadingBlockPlayer(tb);
                case PauseBlock pb:
                    return new PauseBlockPlayer(pb, initialState);
                case PureDataBlock pdb:
                    return new PureDataBlockPlayer(pdb, initialState);
                default:
                    throw new NotImplementedException($"Reader for block type {block.GetType().Name} not implemented");
            }
        }
    }
}
