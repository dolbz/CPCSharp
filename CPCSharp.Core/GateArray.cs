using System;

namespace CPCSharp.Core {

    public interface IODevice {
        ushort Address { set; }
        byte Data { get; set; }
        bool ActiveAtAddress(ushort address);
    }

    /// <summary>
    /// Interrupt behaviour as documented by http://cpctech.cpcwiki.de/docs/ints.html
    /// </summary>
    public class GateArray : IODevice
    {
        private int _clockTicks = 0;
        public bool LowerROMEnabled { get; set;} = true;
        public bool UpperROMEnabled { get; set;} = true;

        private int interruptsThisFrame;
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
                    // select pen
                    break;
                case 1:
                    // select colour for pen
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

        private void ScreenModeROMConfig() {
            // 4	x	Interrupt generation control
            // 3	x	1=Upper ROM area disable, 0=Upper ROM area enable
            // 2	x	1=Lower ROM area disable, 0=Lower ROM area enable
            // 1	x	Screen Mode slection
            // 0   x

            var lowerROMConfigMask = 0x4;
            var upperROMConfigMask = 0x8;

            LowerROMEnabled = (_data & lowerROMConfigMask) == 0;
            UpperROMEnabled = (_data & upperROMConfigMask) == 0;
            //Console.WriteLine($"Screen Mode and ROM config Lower ROM En: {LowerROMEnabled}, Upper ROM EN: {UpperROMEnabled}");
        }
    }
}