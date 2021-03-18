using System;

namespace CDTSharp.Core.Playback
{
    public class StandardBlockPlayer : IBlockPlayer {

        private readonly int _pilotPulseLength;
        private readonly int _firstSyncPulseLength;
        private readonly int _secondSyncPulseLength;
        private readonly int _lengthOfZeroBitPulse;
        private readonly int _lengthOfOneBitPulse;
        private readonly int _lengthOfPilotTone;
        private readonly int _usedBitsInLastByte;
        private readonly int _pauseLengthAfterBlock;
        private readonly byte[] _data;
        private StandardBlockPlaybackState _playbackState = StandardBlockPlaybackState.PilotTone;
        private int _currentPulseCount;
        private int _currentDataPulseLength;
        private int _pilotPulsesCompletedCount;

        private int _dataIndex;
        private int _currentBit;
        private bool _currentState;

        public StandardBlockPlayer(
            int pilotPulseLength,
            int firstSyncPulseLength,
            int secondSyncPulseLength,
            int lengthOfZeroBitPulse,
            int lengthOfOneBitPulse,
            int lengthOfPilotTone,
            int usedBitsInLastByte,
            int pauseLengthAfterBlock,
            byte[] data, 
            bool initialState) {
            _pilotPulseLength = pilotPulseLength;
            _firstSyncPulseLength = firstSyncPulseLength;
            _secondSyncPulseLength = secondSyncPulseLength;
            _lengthOfZeroBitPulse = lengthOfZeroBitPulse;
            _lengthOfOneBitPulse = lengthOfOneBitPulse;
            _lengthOfPilotTone = lengthOfPilotTone;
            _usedBitsInLastByte = usedBitsInLastByte;
            _pauseLengthAfterBlock = pauseLengthAfterBlock;
            _data = data;
            _currentState = initialState;
        }

        public bool IsComplete => _playbackState == StandardBlockPlaybackState.End;

        public bool CurrentState => _currentState;

        public bool ClockTick() {
            _currentPulseCount++;

            switch (_playbackState) {
                case StandardBlockPlaybackState.PilotTone:
                    HandlePilotTone();
                    break;
                case StandardBlockPlaybackState.Sync1:
                    HandleSync1();
                    break;
                case StandardBlockPlaybackState.Sync2:
                    HandleSync2();
                    break;
                case StandardBlockPlaybackState.DataPulse1:
                    HandleDataPulse(nextState: StandardBlockPlaybackState.DataPulse2);
                    break;
                case StandardBlockPlaybackState.DataPulse2:
                    HandleDataPulse(nextState: StandardBlockPlaybackState.DataPulse1);
                    break;
                case StandardBlockPlaybackState.Pause:
                    HandlePause();
                    break;
                default:
                    throw new NotImplementedException();
            }
            
            return _currentState;
        }
        
        private bool HandlePilotTone() {
            var returnState = _currentState;
            if (_currentPulseCount == _pilotPulseLength) {
                _pilotPulsesCompletedCount++;
                if (_pilotPulsesCompletedCount == _lengthOfPilotTone) {
                    SwitchToState(StandardBlockPlaybackState.Sync1);
                } else {
                    _currentPulseCount = 0;
                    CreateEdge();
                }
            }

            return returnState;
        }

        private void HandleSync1() {
            if (_currentPulseCount == _firstSyncPulseLength) {
                SwitchToState(StandardBlockPlaybackState.Sync2);
            }
        }

        private void HandleSync2() {
            if (_currentPulseCount == _secondSyncPulseLength) {
                SwitchToState(StandardBlockPlaybackState.DataPulse1);
            }
        }

        private void HandleDataPulse(StandardBlockPlaybackState nextState) {
            if (_currentPulseCount == _currentDataPulseLength) {
                SwitchToState(nextState);
            }
        }

        private void HandlePause() {
            if (_currentPulseCount == 4000) {
                CreateEdge();
            }
            if (_currentPulseCount == 12000 && _currentState) {
                _currentState = false;
            }
            if (_currentPulseCount == _pauseLengthAfterBlock) {
                SwitchToState(StandardBlockPlaybackState.End, false);
            }
        }

        private void SwitchToState(StandardBlockPlaybackState newState, bool createEdge = true) {
            _currentPulseCount = 0;
            if (createEdge) {
                CreateEdge();
            }
            switch (newState) {
                case StandardBlockPlaybackState.DataPulse1:
                    if (!SetupNextDataBit()) {
                        if (_pauseLengthAfterBlock != 0) {
                            newState = StandardBlockPlaybackState.Pause;
                        } else {
                            newState = StandardBlockPlaybackState.End;
                        }
                    }
                    break;
            }

            _playbackState = newState;
        }

        private bool SetupNextDataBit() {
            if (_dataIndex >= _data.Length || _dataIndex == _data.Length - 1 && _currentBit == _usedBitsInLastByte) {
                return false;
            }

            var currentByte = _data[_dataIndex];

            var currentBitValue = (currentByte >> (7 - _currentBit++)) & 1;

            _currentDataPulseLength = currentBitValue == 1 ? _lengthOfOneBitPulse : _lengthOfZeroBitPulse;

            if (_currentBit == 8) {
                _currentBit = 0;
                _dataIndex++;
            }
            return true;
        }

        private void CreateEdge() {
            _currentState = !_currentState;
        }
    }
}
