using System;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.Playback
{
    public class PureDataBlockPlayer : IBlockPlayer {
        // TODO make use of the last bytes bits used property
        private readonly PureDataBlock _block;

        private int _currentPulseCount;
        private int _dataIndex;
        private int _currentBit;

        private bool _currentState;

        private int _currentPulseLength;

        private bool _dataBitSecondPulseStarted = true;

        private bool _pauseStarted;
        private bool _lastDataBitStarted;

        private bool _alternatePausePeriodComplete;

        public PureDataBlockPlayer(PureDataBlock block, bool initialState) {
            _block = block;
            _currentState = initialState;
        }

        public bool IsComplete => _lastDataBitStarted && _dataBitSecondPulseStarted && _pauseStarted && _currentPulseCount == _currentPulseLength;

        public bool ClockTick() {
            if (_currentPulseCount < _currentPulseLength) {
                _currentPulseCount++;
                return _currentState;
            }

            _currentPulseCount = 0;
            _currentState = !_currentState;

            if (!_dataBitSecondPulseStarted) {
                _dataBitSecondPulseStarted = true;
            } else if (!_lastDataBitStarted) {
                _dataBitSecondPulseStarted = false;

                _currentPulseLength = ((_block.Data[_dataIndex] >> 7-_currentBit++) & 0x1) == 0x1 ? _block.LengthOfOneBitPulse : _block.LengthOfZeroBitPulse;
                
                if (_currentBit == 8) {
                    _currentBit = 0;
                    _dataIndex++;
                    if (_dataIndex == _block.Data.Length) {
                        _lastDataBitStarted = true;
                    }
                }
            } else if (!_pauseStarted) {
                if (!_alternatePausePeriodComplete) {
                    _currentPulseLength = 4000; // 4000 cycles = 1ms on CPC
                    _alternatePausePeriodComplete = true;
                } else {
                    _currentPulseLength = _block.PauseLengthAfterBlock-4000;
                    _pauseStarted = true;
                    _currentState = false;
                }
            } else {
                throw new Exception("Unexpected fall through");
            }
            return _currentState;
        }
    }
}
