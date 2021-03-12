using System.Drawing;
using System;
using System.Security;
using CPCSharp.Core.Interfaces;

namespace CPCSharp.Core {
    /// <summary>
    /// Provides functionality as it's configured by default in the CPC.
    /// </summary>
    public class CRTC : IODevice {

        private readonly IScreenRenderer _screenRenderer;
        private readonly GateArray _gateArray;
        public bool HSYNC { get;set; }
        public bool VSYNC { get;set; }
        public bool DISP { get; set; }

        private int _clockCyclesThisLine;
        private int _numberOfLines;
        private int _currentMemoryAddress;
        private int _horizontalSyncPosition;
        private int _hsyncWidth = 14; // TODO extract this and VSYNC width from register value

        private int _vsyncWidth = 8;

        private int _horizontalTotal;
        private int _horizontalDisplayed;
        private int _verticalDisplayed;

        private int _verticalSyncPosition;
        private int _verticalTotal;

        private int _linesCompleted = 0 ;

        private int _startAddressHigh;
        private int _startAddressLow;

        private int _maxRasterAddress;
        private byte _verticalTotalAdjust;

        public byte _selectedRegister;

        public int RowAddress { get; set; }
        public int MemoryAddress { get; set; }
        public ushort Address { private get; set; }


        public int TotalScanLines => (_verticalTotal*(_maxRasterAddress+1)) + _verticalTotalAdjust;
        public bool InHsyncRegion => _clockCyclesThisLine >= _horizontalSyncPosition-1 && _clockCyclesThisLine < _horizontalSyncPosition-1 + _hsyncWidth;
        public bool InVsyncRegion => _linesCompleted >= (_verticalSyncPosition * (_maxRasterAddress+1)) && _linesCompleted < (_verticalSyncPosition * (_maxRasterAddress+1)) + _vsyncWidth;

        public bool InDispEnRegion => _clockCyclesThisLine >= _horizontalDisplayed || _linesCompleted >= _verticalDisplayed * (_maxRasterAddress+1);
        
        public CRTC(IScreenRenderer screenRenderer, GateArray gateArray) {
            _screenRenderer = screenRenderer;
            _gateArray = gateArray;
        }

        public byte Data { 
            get => throw new NotImplementedException(); 
            set
            {
                switch(Address & 0xff00) {
                    case 0xbc00:
                        _selectedRegister = value;
                        break;
                    case 0xbd00:
                        switch(_selectedRegister) {
                            case 0:
                                _horizontalTotal = value;
                                break;
                            case 1:
                                _horizontalDisplayed = value;
                                break;
                            case 2:
                                _horizontalSyncPosition = value;
                                break;
                            case 3:
                                // TODO _hsyncWidth = 
                                break;
                            case 4:
                                _verticalTotal = value;
                                break;
                            case 5:
                                _verticalTotalAdjust = value;
                                break;
                            case 6:
                                _verticalDisplayed = value;
                                break;
                            case 7:
                                _verticalSyncPosition = value;
                                break;
                            case 9:
                                _maxRasterAddress = value;
                                break;
                            case 12:
                                _startAddressHigh = value;
                                break;
                            case 13:
                                _startAddressLow = value;
                                break;
                            default:
                                // Function not supported at the moment. Do nothing
                                break;
                        }
                        _screenRenderer.ResolutionChanged(CalculateDimensions());
                        break;
                    default:
                        // Not currently supported
                        break;
                }
            } 
        }

        private Size CalculateDimensions() {
            // x2 as the lowest bit of the video ram address is connected to CCLK. This means the 
            // CRTC isn't clocked for every byte rendered but for every other byte.
            var width = (_horizontalTotal+1 - _hsyncWidth) * _gateArray.PixelsPerByte * 2;
            var height = (_verticalTotal * (_maxRasterAddress+1)) - _vsyncWidth;
            if (height <= 0) { // TODO what are the values on CRTC reset?
                height = 1;
            }
            if (width <= 0) {
                width = 1;
            }
            return new Size(width, height);
        }

        public void Clock()
        {
            HSYNC = InHsyncRegion;

            if (!DISP) {
                if (_clockCyclesThisLine == _horizontalDisplayed && RowAddress != _maxRasterAddress) {
                    MemoryAddress -= _horizontalDisplayed;
                }
            }

            if (_clockCyclesThisLine > _horizontalTotal) {
                if (RowAddress == _maxRasterAddress) {
                    RowAddress = 0;
                } else {
                    RowAddress++;
                }
                _clockCyclesThisLine = 0;
                _linesCompleted++;
            }

            if (InVsyncRegion) {
                VSYNC = true;
                MemoryAddress = (_startAddressHigh << 8) | _startAddressLow;
            } else {
                VSYNC = false;
            }

            if (_linesCompleted >= TotalScanLines) {                
                RowAddress = 0;
                _linesCompleted = 0;
            }

            if (!DISP) {
                MemoryAddress++;
            }

            if (InDispEnRegion) {
                DISP = true;
            } else {
                DISP = false;
            }

            _clockCyclesThisLine++;
        }

        public bool ActiveAtAddress(ushort address)
        {
            var significantAddressBits = (address & 0xff00);
            return  significantAddressBits >= 0xbc00 && significantAddressBits <= 0xbf00;
        }
    }
}