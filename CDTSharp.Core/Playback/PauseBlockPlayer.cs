//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.Playback
{
    public class PauseBlockPlayer : IBlockPlayer {
        private int _pauseLength;
        private bool _initialState; 

        // TODO: do we need this behaviour? A 'Pause' block consists of a 'low' pulse level of some duration. To ensure that the last 
        // edge produced is properly finished there should be atleast 1ms pause of the opposite level and only after that the pulse 
        // should go to 'low'. At the end of a 'Pause' block the 'current pulse level' is low. (Note that the first pulse will therefore
        // not immediately produce an edge.) A 'pause' block of zero duration is completely ignored, so the 'current pulse level' will NOT
        // change in this case. This also applies to 'Data' blocks that have some pause duration included in them.

        public PauseBlockPlayer(PauseBlock block, bool initialState) {
            _initialState = initialState;
            _pauseLength = block.PauseLength;
        }

        public bool IsComplete => _pauseLength == 0;

        public bool ClockTick()
        {
            _pauseLength--;
            return false;
        }
    }
}
