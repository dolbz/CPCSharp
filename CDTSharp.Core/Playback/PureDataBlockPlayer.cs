//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System;
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.Playback
{
   internal enum PureDataBlockPlaybackState {
        DataPulse1,
        DataPulse2,
        Pause,
        End
    }

    public class PureDataBlockPlayer : IBlockPlayer {
        private readonly PureDataBlock _block;

        private PureDataBlockPlaybackState _playbackState = PureDataBlockPlaybackState.DataPulse1;
        private int _currentPulseCount;
        private int _currentDataPulseLength;
        private int _pilotPulsesCompletedCount;

        private int _dataIndex;
        private int _currentBit;

        private bool _currentState;

        public PureDataBlockPlayer(PureDataBlock block, bool initialState) {
            _block = block;
            _currentState = initialState;
            SetupNextDataBit();
        }

        public bool IsComplete => _playbackState == PureDataBlockPlaybackState.End;

        public bool CurrentState => _currentState;

        public bool ClockTick() {
            _currentPulseCount++;

            switch (_playbackState) {
                case PureDataBlockPlaybackState.DataPulse1:
                    HandleDataPulse(nextState: PureDataBlockPlaybackState.DataPulse2);
                    break;
                case PureDataBlockPlaybackState.DataPulse2:
                    HandleDataPulse(nextState: PureDataBlockPlaybackState.DataPulse1);
                    break;
                case PureDataBlockPlaybackState.Pause:
                    HandlePause();
                    break;
                default:
                    throw new NotImplementedException();
            }
            
            return _currentState;
        }

        private void HandleDataPulse(PureDataBlockPlaybackState nextState) {
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
                SwitchToState(PureDataBlockPlaybackState.End, false);
            }
        }

        private void SwitchToState(PureDataBlockPlaybackState newState, bool createEdge = true) {
            _currentPulseCount = 0;
            if (createEdge) {
                CreateEdge();
            }
            switch (newState) {
                case PureDataBlockPlaybackState.DataPulse1:
                    if (!SetupNextDataBit()) {
                        if (_block.PauseLengthAfterBlock != 0) {
                            newState = PureDataBlockPlaybackState.Pause;
                        } else {
                            newState = PureDataBlockPlaybackState.End;
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
