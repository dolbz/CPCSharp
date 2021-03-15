using System;
using Avalonia.Rendering;
using CPCSharp.Core;

namespace CPCSharp.App {
    public class CPCRenderLoopTask : IRenderLoopTask
    {
        private readonly  CPCRunner _runner;
        
        private TimeSpan _lastTime = default;
        private int _lastClockCyclesRequired;
        public bool NeedsUpdate => true;
        
        public CPCRenderLoopTask(CPCRunner runner) {
            _runner = runner;
        }

        public void Render()
        {
            // Not sure what distinction Avalonia has between Render/Update but we don't seem to need it.
        }

        public void Update(TimeSpan time)
        {
            if (_lastTime == default) {
                _lastTime = time;
                return;
            }
            var timeDiff = time - _lastTime;
            _lastTime = time;

            // There are 10,000,000 ticks in a second
            // The CPC system clock is 16MHz so 16,000,000 clock pulses/second
            // To calculate clock cycles needed we can use 1.6 * elapsed ticks 
            // This might be slightly lossy as we don't adjust for rounding over time...
            var clockCyclesRequired = (int)(1.6 * timeDiff.Ticks);
            _lastClockCyclesRequired = clockCyclesRequired;
            _runner.DispatchCycleCountRequest(clockCyclesRequired);
        }
    }
}