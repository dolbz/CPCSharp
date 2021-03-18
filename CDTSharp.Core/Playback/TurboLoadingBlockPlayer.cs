using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.Playback
{
    public class TurboLoadingBlockPlayer : IBlockPlayer {
        private readonly StandardBlockPlayer _player;
        public TurboLoadingBlockPlayer(TurboLoadingBlock block, bool initialState) {
            _player = new StandardBlockPlayer(
                block.PilotPulseLength,
                block.FirstSyncPulseLength,
                block.SecondSyncPulseLength,
                block.LengthOfZeroBitPulse,
                block.LengthOfOneBitPulse,
                block.LengthOfPilotTone,
                block.UsedBitsInLastByte,
                block.PauseLengthAfterBlock,
                block.Data,
                initialState);
        }

        public bool IsComplete => _player.IsComplete;

        public bool ClockTick()
        {
            return _player.ClockTick();
        }
    }
}
