using System;
using System.Security;
namespace CPCSharp.Core {
    /// <summary>
    /// Provides functionality as it's configured by default in the CPC.
    /// </summary>
    public class CRTC {
        public bool HSYNC { get;set; }
        public bool VSYNC { get;set; }
        public bool DISP { get; set; }

        private int _clockCyclesThisLine = 0;
        private int _numberOfLines = 0;
        private int _currentMemoryAddress = 0;
        private int _horizontalSyncPosition = 46; // TODO allow this to be set via IORQ
        private int _hsyncWidth = 14; // TODO extract this from Vsync/hsync register value set via IORQ

        private int _vsyncWidth = 8;

        private int _horizontalTotal = 63;
        private int _horizontalDisplayed = 40;
        private int _verticalDisplayed = 25;

        private int _verticalSyncPosition = 30;
        private int _verticalTotal = 38;

        private int _linesCompleted = 0;

        private int _startAddressHigh = 0x30;
        private int _startAddressLow = 0;

        private int _maxRasterAddress = 7;

        public int RowAddress { get; set; }
        public int MemoryAddress { get; set; }

        public void Clock() 
        {
            if (_clockCyclesThisLine >= _horizontalSyncPosition && _clockCyclesThisLine < _horizontalSyncPosition + _hsyncWidth) {
                HSYNC = true;
            } else {
                HSYNC = false;
            }

            if (!DISP) {
                if (_clockCyclesThisLine == _horizontalDisplayed && RowAddress != _maxRasterAddress) {
                    MemoryAddress -= _horizontalDisplayed;
                }
            }

            if (_clockCyclesThisLine == _horizontalTotal) {
                if (RowAddress == _maxRasterAddress) {
                    RowAddress = 0;
                } else {
                    RowAddress++;
                }
                _clockCyclesThisLine = 0;
                _linesCompleted++;
            }

            if (_linesCompleted >= (_verticalSyncPosition * _maxRasterAddress) && _linesCompleted < (_verticalSyncPosition * _maxRasterAddress) + _vsyncWidth) {
                VSYNC = true;
                MemoryAddress = (_startAddressHigh << 8) | _startAddressLow;
            } else {
                VSYNC = false;
            }

            if (_linesCompleted >= _verticalTotal*_maxRasterAddress) {
                
                RowAddress = 0;
                _linesCompleted = 0;
            }

            if (!DISP) {
                MemoryAddress++;
            }

            if (_clockCyclesThisLine >= _horizontalDisplayed || _linesCompleted >= _verticalDisplayed * _maxRasterAddress) {
                DISP = true;
            } else {
                DISP = false;
            }

            _clockCyclesThisLine++;
        }
    }
}