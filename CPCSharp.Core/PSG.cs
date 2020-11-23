namespace CPCSharp.Core {

    /// <summary>
    /// Keyboard only implementation atm
    /// TODO sound stuff
    /// </summary>
    public class PSG {

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
                if (_currentRegisterIndex == 14) {
                    //_registers[_currentRegisterIndex] = IOA;

                    _registers[_currentRegisterIndex] = GetKeyboardRowData();
                } else if (_currentRegisterIndex == 15) {

                } else {
                    _registers[_currentRegisterIndex] = Data;
                }
            } else if (_bdir && _bc1) { // Latch address
                _currentRegisterIndex = Data & 0xf;
            }
        }

        private byte GetKeyboardRowData() {
            return KeyboardState.Instance.KeyStateForLine(_keyboardLine);
        }
    }
}