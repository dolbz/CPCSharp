//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace CDTSharp.Core.Blocks
{
    public class PauseBlock : IBlock {
        public int PauseLength { get; init; }

        public string Description { 
            get 
            {
                return $"Pause for {PauseLength}ms";
            }
        }
    }
}