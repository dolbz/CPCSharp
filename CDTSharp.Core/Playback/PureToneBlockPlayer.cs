using System;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.Playback
{
    public class PureToneBlockPlayer : IBlockPlayer {
        private readonly PureToneBlock _block;

        private int _currentPulseCount;
        private int _pulsesCompleted;

        private bool _currentState;

        public PureToneBlockPlayer(PureToneBlock block, bool initialState) {
            _block = block;
            _currentState = initialState;
        }

        public bool IsComplete => _pulsesCompleted == _block.NumberOfPulses;

        public bool ClockTick() {
            if (_currentPulseCount < _block.PulseLength) {
                _currentPulseCount++;
                return _currentState;
            }

            _pulsesCompleted++;
            _currentPulseCount = 0;
            _currentState = !_currentState;

            return _currentState;
        }
    }
}
