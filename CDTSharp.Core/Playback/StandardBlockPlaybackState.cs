//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace CDTSharp.Core.Playback
{
    internal enum StandardBlockPlaybackState {
        PilotTone,
        Sync1,
        Sync2,
        DataPulse1,
        DataPulse2,
        Pause,
        End
    }
}
