using System;
using System.Security.Authentication.ExtendedProtection;
namespace CPCSharp.Core.PSG {

    /// <summary>
    /// Keyboard only implementation atm
    /// TODO sound stuff
    /// </summary>
    public class AY8912 {
        private readonly INativePSG _nativePsg;

        public AY8912() {
            _nativePsg = new NativePSG();
        }

        private byte _keyboardLine;
        /// <summary>
        /// In reality the line selector is a separate chip but for our purposes it's useful to have it here
        /// </summary>
        public byte KeyboardLine { 
            set 
            {
                _keyboardLine = value;
                CheckForStateChange();
            }
        
        }
        public byte Data { get; set; }

        private byte[] _registers = new byte[15];
        private int _currentRegisterIndex;

        private bool _bdir;
        public bool BDIR { 
            set {
                _bdir = value;
                CheckForStateChange();
            }
         }

        private bool _bc1;

        public bool BC1 { 
            set {
                _bc1 = value;
                CheckForStateChange();
            }
        }

        private void CheckForStateChange() {
            if (!_bdir && _bc1) { // Read from PSG 
                if (_currentRegisterIndex == 14) {
                    Data = GetKeyboardRowData();
                } else {
                    Data = _registers[_currentRegisterIndex];
                }
            } else if(_bdir && !_bc1) { // Write to psg

                _registers[_currentRegisterIndex] = Data;

                switch (_currentRegisterIndex) {
                    case 0:
                    case 1:
                        UpdateToneNative(PSGChannel.A);
                        break;
                    case 2:
                    case 3:
                        UpdateToneNative(PSGChannel.B);
                        break;
                    case 4:
                    case 5:
                        UpdateToneNative(PSGChannel.C);
                        break;
                    case 6:
                        UpdateNoisePeriod();
                        break;
                    case 7:
                        EnableRegisterUpdated();
                        break;
                    case 8:
                        UpdateAmplitude(PSGChannel.A);
                        break;
                    case 9:
                        UpdateAmplitude(PSGChannel.B);
                        break;
                    case 10:
                        UpdateAmplitude(PSGChannel.C);
                        break;
                    case 14:
                        _registers[_currentRegisterIndex] = GetKeyboardRowData();
                        break;
                    case 15:
                        break;
                    default:
                        break;
                }
            } else if (_bdir && _bc1) { // Latch address
                _currentRegisterIndex = Data & 0xf;
            }
        }

        private void UpdateNoisePeriod() {
            // TODO
        }

        private void UpdateAmplitude(PSGChannel channel) {
            int regIndex = 8 + (int)channel;

            var ampValue = _registers[regIndex] & 0xf;
            var isFixedMode = (_registers[regIndex] & 0x10) == 0;

            var amplitude = ampValue / 15.0f;

            _nativePsg.SetAmplitudeMode(channel, isFixedMode);
            _nativePsg.SetAmplitude(channel, amplitude);
        }

        private void EnableRegisterUpdated() {
            var enableRegisterValue = _registers[7];

            bool channelAEnabled = (enableRegisterValue & 0x1) == 0;
            bool channelBEnabled = ((enableRegisterValue >> 1) & 0x1) == 0;
            bool channelCEnabled = ((enableRegisterValue >> 2) & 0x1) == 0;

            bool channelANoiseEnabled = ((enableRegisterValue >> 3) & 0x1) == 0;
            bool channelBNoiseEnabled = ((enableRegisterValue >> 4) & 0x1) == 0;
            bool channelCNoiseEnabled = ((enableRegisterValue >> 5) & 0x1) == 0;

            _nativePsg.SetChannelAttributes(PSGChannel.A, channelAEnabled, channelANoiseEnabled);
            _nativePsg.SetChannelAttributes(PSGChannel.B, channelBEnabled, channelBNoiseEnabled);
            _nativePsg.SetChannelAttributes(PSGChannel.C, channelCEnabled, channelCNoiseEnabled);
        }

        private void UpdateToneNative(PSGChannel channel)
        {
            int fineFreqRegIndex = -1;
            int coarseFreqRegIndex = -1;

            switch (channel) {
                case PSGChannel.A:
                    fineFreqRegIndex = 0;
                    coarseFreqRegIndex = 1;
                    break;
                case PSGChannel.B:
                    fineFreqRegIndex = 2;
                    coarseFreqRegIndex = 3;
                    break;
                case PSGChannel.C:
                    fineFreqRegIndex = 4;
                    coarseFreqRegIndex = 5;
                    break;
            }

            var overallCounter = ((_registers[coarseFreqRegIndex] & 0xf) << 8) | (_registers[fineFreqRegIndex]);
            
            if (overallCounter == 0) {
                overallCounter = 1;
            }

            var calculatedFreq = 1_000_000.0f / (16 * overallCounter);

            _nativePsg.SetTone(channel, calculatedFreq);
        }

        private byte GetKeyboardRowData() {
            return KeyboardState.Instance.KeyStateForLine(_keyboardLine);
        }
    }
}