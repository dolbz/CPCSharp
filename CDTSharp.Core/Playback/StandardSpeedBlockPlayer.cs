//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using CDTSharp.Core.Blocks;

namespace CDTSharp.Core.Playback
{
    public class StandardSpeedBlockPlayer : IBlockPlayer {
        private readonly StandardBlockPlayer _player;
        public StandardSpeedBlockPlayer(StandardSpeedBlock block, bool initialState) {
            
            var blockIsHeader = block.Data[0] == 0x2c;

            _player = new StandardBlockPlayer(
                pilotPulseLength: 2168,
                firstSyncPulseLength: 667,
                secondSyncPulseLength: 735,
                lengthOfZeroBitPulse: 855,
                lengthOfOneBitPulse: 1710,
                lengthOfPilotTone: blockIsHeader ? 8064 : 3220,
                usedBitsInLastByte: 8,
                pauseLengthAfterBlock: block.PauseLengthAfterBlock,
                block.Data,
                initialState);
        }

        public bool IsComplete => _player.IsComplete;

        public bool ClockTick()
        {
            return _player.ClockTick();
        }
    }
}
