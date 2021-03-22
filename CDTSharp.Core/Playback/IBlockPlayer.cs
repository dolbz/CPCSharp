//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace CDTSharp.Core.Playback
{
    public interface IBlockPlayer
    {
        bool IsComplete { get; }
        bool ClockTick();
    }
}
