using System;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.Playback
{
    public static class BlockExtensions {
        public static IBlockPlayer CreateBlockPlayer(this IBlock block, bool initialState) {
            switch (block) {
                case TurboLoadingBlock tb:
                    return new TurboLoadingBlockPlayer(tb, initialState);
                case PauseBlock pb:
                    return new PauseBlockPlayer(pb, initialState);
                case PureDataBlock pdb:
                    return new PureDataBlockPlayer(pdb, initialState);
                case PureToneBlock ptb:
                    return new PureToneBlockPlayer(ptb, initialState);
                case SequenceOfPulsesBlock seq:
                    return new SequenceOfPulsesBlockPlayer(seq, initialState);
                case GroupStartBlock:
                case GroupEndBlock:
                    // Cases that return null are non-playable blocks.
                    // The consuming code will request a player for the next block
                    // If it receives a null back
                    return null;
                default:
                    throw new NotImplementedException($"Player for block type {block.GetType().Name} not implemented");
            }
        }
    }
}
