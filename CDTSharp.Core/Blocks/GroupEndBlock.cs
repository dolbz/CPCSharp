//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace CDTSharp.Core.Blocks
{
    public class GroupEndBlock : IBlock {
        public string GroupName { get; init; }

        public string Description { 
            get 
            {
                return $"Group End" +
                       $"---------";
            }
        }
    }
}