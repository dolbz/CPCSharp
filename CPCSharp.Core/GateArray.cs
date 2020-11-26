using System;

namespace CPCSharp.Core
{
    public enum ScreenMode {
        Mode0 = 0,
        Mode1 = 1,
        Mode2 = 2,
        Mode3 = 3 // Unofficial and not supported here
    }

    /// <summary>
    /// Interrupt behaviour as documented by http://cpctech.cpcwiki.de/docs/ints.html
    /// </summary>
    public class GateArray : IODevice
    {
        private int _clockTicks = 0;
        public bool LowerROMEnabled { get; set;} = true;
        public bool UpperROMEnabled { get; set;} = true;

        private ScreenMode _screenMode;

        private int[] _penColours = new int[16];
        private int _borderColour = 0;
        private int _selectedPenIndex = 0;

        private int _hsyncsSinceVsyncStarted;
        private bool _vsync;
        public bool VSYNC { 
            get => _vsync;
            set {
                var currentValue = _vsync;
                _vsync = value;
                if (!currentValue && _vsync) {
                    _hsyncsSinceVsyncStarted = 0;
                }
            }
        }

        private bool _hsync;
        private int _hsyncCompletedCount;
        public bool HSYNC { 
            set {
                var currentValue = _hsync;
                _hsync = value;
                if (currentValue && !_hsync) {
                    _hsyncCompletedCount++;
                    if (_vsync) {
                        _hsyncsSinceVsyncStarted++;
                    }
                }
            }
        }

        public bool INTERRUPT { get; set; }

        public bool CpuClock { get; private set; }

        public bool CCLK{ get; private set; }

        public bool M1 { get; set; }
        public bool IORQ { get; set; }

        private byte _data;
        public byte Data { 
            get { 
                Console.WriteLine("Data read from gate array which isn't possible");
                return 0;}
        
            set {
                _data = value;
                ProcessInputData();
            } 
        }

        public ushort Address {
            get;
            set;
        }

        public void Clock() {
            _clockTicks++;
            CpuClock = _clockTicks % 4 == 0;
            CCLK = _clockTicks % 16 == 0;

            CheckHsyncStatus();
            CheckVsyncStatus();

            if (M1 && IORQ) {
                _hsyncCompletedCount &= 0b00011111;
                INTERRUPT = false;
            }
        }

        private void CheckHsyncStatus() {
            if (_hsyncCompletedCount == 52) {
                _hsyncCompletedCount = 0;
                INTERRUPT = true;
            }
        }

        private void CheckVsyncStatus() {
            if (_hsyncsSinceVsyncStarted == 2) {
                if (_hsyncCompletedCount < 32) {
                    INTERRUPT = true;
                }
                _hsyncsSinceVsyncStarted++; // Stops the interrupt being triggered a second time this scree
                _hsyncCompletedCount = 0;
            }
        }

        public bool ActiveAtAddress(ushort address)
        {
            // Returns true if most significant 2 bits == 01
            return (address & 0xc000) == 0x4000;
        }

        private void ProcessInputData() {
            var functionSelectionBits = _data >> 6;

            switch (functionSelectionBits) {
                case 0:
                    SelectPen();
                    break;
                case 1:
                    SelectPenColour();
                    break;
                case 2:
                    // select screen mode, ROM configuration and interrupt control
                    ScreenModeROMConfig();
                    break;
                case 3:
                    // RAM management
                    break;
            }
        }

        private void SelectPenColour() 
        {
            _penColours[_selectedPenIndex] = Data & 0x1f;
        }

        private void SelectPen() {
            // Bit	Value	Function
            // 7	0	    Gate Array function "Pen Selection"
            // 6	0
            // 5	-	    not used
            // 4	1	    Select border
            // 3	x	    bits 0-3 = pen number if select border is false
            // 2	x
            // 1	x
            // 0	x

            if ((Data & 0x16) == 0x16) {
                _selectedPenIndex = -1; // -1 is a magic value indicating border colour pen is selected
            } else {
                var penNumber = Data & 0x0f;
                _selectedPenIndex = penNumber;
            }
        }

        private void ScreenModeROMConfig() {
            // 4	x	Interrupt generation control
            // 3	x	1=Upper ROM area disable, 0=Upper ROM area enable
            // 2	x	1=Lower ROM area disable, 0=Lower ROM area enable
            // 1	x	Screen Mode selection
            // 0    x

            var lowerROMConfigMask = 0x4;
            var upperROMConfigMask = 0x8;

            LowerROMEnabled = (_data & lowerROMConfigMask) == 0;
            UpperROMEnabled = (_data & upperROMConfigMask) == 0;

            var screenModeBits = Data & 0x3;
            _screenMode = (ScreenMode)screenModeBits;

            //Console.WriteLine($"Screen Mode and ROM config Lower ROM En: {LowerROMEnabled}, Upper ROM EN: {UpperROMEnabled}");
        }
    }
}