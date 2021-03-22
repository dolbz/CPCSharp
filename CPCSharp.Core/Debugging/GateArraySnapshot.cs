//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace CPCSharp.Core.Debugging {
    public class GateArraySnapshot {
        public bool CpuClock { get; private set; }

        public static GateArraySnapshot SnapShotFromGateArray(GateArray gateArray) {
            return new GateArraySnapshot {
                CpuClock = gateArray.CpuClock
            };
        }
    }
}