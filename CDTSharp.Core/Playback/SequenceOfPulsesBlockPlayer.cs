using System;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.Playback
{
    public class SequenceOfPulsesBlockPlayer : IBlockPlayer {
        private readonly SequenceOfPulsesBlock _block;

        private int _currentPulseCount;
        private int _pulsesCompleted;

        private bool _currentState;

        public SequenceOfPulsesBlockPlayer(SequenceOfPulsesBlock block, bool initialState) {
            _block = block;
            _currentState = initialState;
        }

        public bool IsComplete => _pulsesCompleted == _block.PulseLengths.Length;

        public bool ClockTick() {
            if (_currentPulseCount < _block.PulseLengths[_pulsesCompleted]) {
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
