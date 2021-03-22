//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace CDTSharp.Core.BlockReaders
{
    public static class IntExtensions {
        public static int AdjustTCycles(this int tCycles) {
            return (int)(tCycles * (4/3.5));
        }
    }
}
