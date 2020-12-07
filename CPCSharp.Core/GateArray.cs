using System.Drawing;
using System.Collections.Generic;
using System;
using CPCSharp.Core.Interfaces;

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
        private Dictionary<int, Color> ColourMap = new Dictionary<int, Color> {
            { 0, Color.FromArgb(255/2,   255/2,  255/2) },
            { 1, Color.FromArgb(255/2,   255/2,  255/2) },
            { 2, Color.FromArgb(0,       255,    255/2) },
            { 3, Color.FromArgb(255,     255,    255/2) },
            { 4, Color.FromArgb(0,       0,      255/2) },
            { 5, Color.FromArgb(255,     0,      255/2) },
            { 6, Color.FromArgb(0,       255/2,  255/2) },
            { 7, Color.FromArgb(255,     255/2,  255/2) },
            { 8, Color.FromArgb(255,     0,      255/2) },
            { 9, Color.FromArgb(255,     255,    255/2) },
            { 10, Color.FromArgb(255,     255,    0    ) },
            { 11, Color.FromArgb(255,     255,    255  ) },
            { 12, Color.FromArgb(255,     0,      0    ) },
            { 13, Color.FromArgb(255,     0,      255  ) },
            { 14, Color.FromArgb(255,     255/2,  0    ) },
            { 15, Color.FromArgb(255,     255/2,  255  ) },
            { 16, Color.FromArgb(0,       0,      255/2) },
            { 17, Color.FromArgb(0,       255,    255/2) },
            { 18, Color.FromArgb(0,       255,    0    ) },
            { 19, Color.FromArgb(0,       255,    255  ) },
            { 20, Color.FromArgb(0,       0,      0    ) },
            { 21, Color.FromArgb(0,       0,      255  ) },
            { 22, Color.FromArgb(0,       255/2,  0    ) },
            { 23, Color.FromArgb(0,       255/2,  255  ) },
            { 24, Color.FromArgb(255/2,   0,      255/2) },
            { 25, Color.FromArgb(255/2,   255,    255/2) },
            { 26, Color.FromArgb(255/2,   255,    0    ) },
            { 27, Color.FromArgb(255/2,   255,    255  ) },
            { 28, Color.FromArgb(255/2,   0,      0    ) },
            { 29, Color.FromArgb(255/2,   0,      255  ) },
            { 30, Color.FromArgb(255/2,   255/2,  0    ) },
            { 31, Color.FromArgb(255/2,   255/2,  255  ) }
        };

        private readonly IScreenRenderer _renderer;
        private int _clockTicks = 0;
        public bool LowerROMEnabled { get; set;} = true;
        public bool UpperROMEnabled { get; set;} = true;

        private ScreenMode _screenMode;

        private int[] _penColours = new int[16];
        private int _borderColour = 0;
        private int _selectedPenIndex = 0;

        private int _hsyncsSinceVsyncStarted;

        private bool _pendingVsyncOff;
        private bool _vsync;
        public bool VSYNC { 
            get => _vsync;
            set {
                var currentValue = _vsync;
                _vsync = value;
                if (!currentValue && _vsync) {
                    _hsyncsSinceVsyncStarted = 0;
                }
                if (currentValue && !_vsync) {
                    _pendingVsyncOff = true;
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

        public bool DISPEN { get; set; }

        public bool INTERRUPT { get; set; }

        public bool CpuClock { get; private set; }

        public bool CCLK { get; private set; }
        
        public bool CCLK_Off { get; private set; }

        public bool READY { get; private set; }
        public bool CPUADDR { get; private set; }

        public bool M1 { get; set; }
        public bool IORQ { get; set; }

        private byte _data;
        public byte Data { 
            get { 
                Console.WriteLine("Data read from gate array which isn't possible");
                return 0;
            }
            set {
                _data = value;
                if (CPUADDR) {
                    ProcessInputData(); // Only if there's input data on the line
                }
            } 
        }

        private ushort _address;
        public ushort Address {
            get => _address;
            set {
                _address = value;
            }
        }

        public GateArray(IScreenRenderer renderer) {
            _renderer = renderer;
        }

        public void Clock() {
            var sixteenths = _clockTicks % 16;

            if (_pendingVsyncOff) {
                _renderer.SendVsyncEnd(); 
                _pendingVsyncOff = false;
            }

            if (CCLK || sixteenths == 9) 
            {
                SendPixels();
            }

            
            _clockTicks++;
            CpuClock = _clockTicks % 4 == 0;
            CCLK = sixteenths == 11;
            CCLK_Off = sixteenths == 3;
            READY = sixteenths >= 0 && sixteenths < 4;
            CPUADDR = sixteenths >= 0 && sixteenths < 8;

            CheckHsyncStatus();
            CheckVsyncStatus();

            if (M1 && IORQ) {
                _hsyncCompletedCount &= 0b00011111;
                INTERRUPT = false;
            }
        }

        private void SendPixels() {
            // Sends a batch of pixels to a renderer. Amount varies by screen mode
            // This is unlike the real hardware where the video output will change on each clock tick
            // but it's likely quicker to send batches and there's no real downside for non-analogue renderers.
            // The memory data is read in full bytes so it's not like the batching causes any timing mismatch with
            // pixels changing after they've been sent in a batch
            if (!DISPEN) {
                var pixels = GeneratePixelsForScreenMode();
                _renderer.SendPixels(pixels);
            }
        }

        private Color[] GeneratePixelsForScreenMode() {
            switch (_screenMode) {
                case ScreenMode.Mode0:
                    return ExtractMode0PixelsFromData();
                case ScreenMode.Mode1:
                    return ExtractMode1PixelsFromData();
                case ScreenMode.Mode2:
                    return ExtractMode2PixelsFromData();
                default:
                    throw new NotImplementedException("This screen mode is not supported");
            }
        }

        private Color[] ExtractMode0PixelsFromData() {
            var pixel0Bit0 = (_data & 0x80) >> 7;
            var pixel0Bit1 = (_data & 0x8) >> 2;
            var pixel0Bit2 = (_data & 0x20) >> 3;
            var pixel0Bit3 = (_data & 0x2) << 2;
            var pixel0Value = _penColours[pixel0Bit3 | pixel0Bit2 | pixel0Bit1 | pixel0Bit0];

            var pixel1Bit0 = (_data & 0x40) >> 6;
            var pixel1Bit1 = (_data & 0x4) >> 1;
            var pixel1Bit2 = (_data & 0x10) >> 2;
            var pixel1Bit3 = (_data & 0x1) << 3;
            var pixel1Value = _penColours[pixel1Bit3 | pixel1Bit2 | pixel1Bit1 | pixel1Bit0];

            return new Color[] { ColourMap[pixel0Value], ColourMap[pixel1Value] };
        }

        private Color[] ExtractMode1PixelsFromData() {
            var pixel0Bit0 = (_data & 0x8) >> 3;
            var pixel0Bit1 = (_data & 0x80) >> 6;
            var pixel0Value = _penColours[pixel0Bit1 | pixel0Bit0];

            var pixel1Bit0 = (_data & 0x4) >> 2;
            var pixel1Bit1 = (_data & 0x40) >> 5;
            var pixel1Value = _penColours[pixel1Bit1 | pixel1Bit0];

            var pixel2Bit0 = (_data & 0x2) >> 1;
            var pixel2Bit1 = (_data & 0x20) >> 4;
            var pixel2Value = _penColours[pixel2Bit1 | pixel2Bit0];

            var pixel3Bit0 = (_data & 0x1);
            var pixel3Bit1 = (_data & 0x10) >> 3;
            var pixel3Value = _penColours[pixel3Bit1 | pixel3Bit0];

            return new Color[] { ColourMap[pixel0Value], ColourMap[pixel1Value], ColourMap[pixel2Value], ColourMap[pixel3Value] };
        }

        private Color[] ExtractMode2PixelsFromData() {
            var pixel7Value = _penColours[(_data & 0x80) >> 7];
            var pixel6Value = _penColours[(_data & 0x40) >> 6];
            var pixel5Value = _penColours[(_data & 0x20) >> 5];
            var pixel4Value = _penColours[(_data & 0x10) >> 4];
            var pixel3Value = _penColours[(_data & 0x8) >> 3];
            var pixel2Value = _penColours[(_data & 0x4) >> 2];
            var pixel1Value = _penColours[(_data & 0x2) >> 1];
            var pixel0Value = _penColours[_data & 0x1];

            return new Color[] { 
                ColourMap[pixel0Value], 
                ColourMap[pixel1Value],
                ColourMap[pixel2Value],
                ColourMap[pixel3Value], 
                ColourMap[pixel4Value],
                ColourMap[pixel5Value],
                ColourMap[pixel6Value],
                ColourMap[pixel7Value],
            };
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
            _penColours[_selectedPenIndex] = _data & 0x1f;
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

            if ((_data & 0x16) == 0x16) {
                _selectedPenIndex = -1; // -1 is a magic value indicating border colour pen is selected
            } else {
                var penNumber = _data & 0x0f;
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

            var screenModeBits = _data & 0x3;
            _screenMode = (ScreenMode)screenModeBits;

            //Console.WriteLine($"Screen Mode and ROM config Lower ROM En: {LowerROMEnabled}, Upper ROM EN: {UpperROMEnabled}");
        }
    }
}