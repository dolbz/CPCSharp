namespace CPCSharp.Core {
    /// <summary>
    /// Provides functionality as it's configured by default in the CPC.
    /// </summary>
    public class CRTC {
        public bool HSYNC { get;set; }
        public bool VSYNC { get;set; }

        private int _clockCyclesThisLine = 0;
        private int _numberOfLines = 0;
        public void Clock() 
        {
            _clockCyclesThisLine++;

            if (_clockCyclesThisLine == 60) { //40chars + 6 border + 14 HSYNC
                _clockCyclesThisLine = 0;
                _numberOfLines++;
            }

            if (_numberOfLines > 328) {
                _numberOfLines = 0;
            }

            // TODO Border
            HSYNC = _clockCyclesThisLine > 46;
            VSYNC = _numberOfLines > 200;
        }
    }
}