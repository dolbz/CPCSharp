using System;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.Playback
{
    internal enum TurboLoadingBlockPlaybackState {
        PilotTone,
        Sync1,
        Sync2,
        DataPulse1,
        DataPulse2,
        Pause,
        End
    }

    public class TurboLoadingBlockPlayer : IBlockPlayer {
        private readonly TurboLoadingBlock _block;

        private TurboLoadingBlockPlaybackState _playbackState = TurboLoadingBlockPlaybackState.PilotTone;
        private int _currentPulseCount;
        private int _currentDataPulseLength;
        private int _pilotPulsesCompletedCount;

        private int _dataIndex;
        private int _currentBit;

        private bool _currentState;

        public TurboLoadingBlockPlayer(TurboLoadingBlock block, bool initialState) {
            _block = block;
            _currentState = initialState; // Invert as first clock inverts it because of reasons...
        }

        public bool IsComplete => _playbackState == TurboLoadingBlockPlaybackState.End;

        public bool CurrentState => _currentState;

        public bool ClockTick() {
            _currentPulseCount++;

            //var returnState = _currentState;

            switch (_playbackState) {
                case TurboLoadingBlockPlaybackState.PilotTone:
                    HandlePilotTone();
                    break;
                case TurboLoadingBlockPlaybackState.Sync1:
                    HandleSync1();
                    break;
                case TurboLoadingBlockPlaybackState.Sync2:
                    HandleSync2();
                    break;
                case TurboLoadingBlockPlaybackState.DataPulse1:
                    HandleDataPulse(nextState: TurboLoadingBlockPlaybackState.DataPulse2);
                    break;
                case TurboLoadingBlockPlaybackState.DataPulse2:
                    HandleDataPulse(nextState: TurboLoadingBlockPlaybackState.DataPulse1);
                    break;
                case TurboLoadingBlockPlaybackState.Pause:
                    HandlePause();
                    break;
                default:
                    throw new NotImplementedException();
            }
            
            return _currentState;
        }
        
        private bool HandlePilotTone() {
            var returnState = _currentState;
            if (_currentPulseCount == _block.PilotPulseLength) {
                _pilotPulsesCompletedCount++;
                if (_pilotPulsesCompletedCount == _block.LengthOfPilotTone) {
                    SwitchToState(TurboLoadingBlockPlaybackState.Sync1);
                } else {
                    _currentPulseCount = 0;
                    CreateEdge();
                }
            }

            return returnState;
        }

        private void HandleSync1() {
            if (_currentPulseCount == _block.FirstSyncPulseLength) {
                SwitchToState(TurboLoadingBlockPlaybackState.Sync2);
            }
        }

        private void HandleSync2() {
            if (_currentPulseCount == _block.SecondSyncPulseLength) {
                SwitchToState(TurboLoadingBlockPlaybackState.DataPulse1);
            }
        }

        private void HandleDataPulse(TurboLoadingBlockPlaybackState nextState) {
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
            if (_currentPulseCount == _block.PauseLengthAfterBlock) {
                SwitchToState(TurboLoadingBlockPlaybackState.End, false);
            }
        }

        private void SwitchToState(TurboLoadingBlockPlaybackState newState, bool createEdge = true) {
            _currentPulseCount = 0;
            if (createEdge) {
                CreateEdge();
            }
            switch (newState) {
                case TurboLoadingBlockPlaybackState.DataPulse1:
                    if (!SetupNextDataBit()) {
                        if (_block.PauseLengthAfterBlock != 0) {
                            newState = TurboLoadingBlockPlaybackState.Pause;
                        } else {
                            newState = TurboLoadingBlockPlaybackState.End;
                        }
                    }
                    break;
            }

            _playbackState = newState;
        }

        private bool SetupNextDataBit() {
            if (_dataIndex >= _block.Data.Length || _dataIndex == _block.Data.Length - 1 && _currentBit == _block.UsedBitsInLastByte) {
                return false;
            }

            var currentByte = _block.Data[_dataIndex];

            var currentBitValue = (currentByte >> (7 - _currentBit++)) & 1;

            _currentDataPulseLength = currentBitValue == 1 ? _block.LengthOfOneBitPulse : _block.LengthOfZeroBitPulse;

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
