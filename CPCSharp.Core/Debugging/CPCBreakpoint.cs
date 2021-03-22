//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System;

namespace CPCSharp.Core.Debugging {
    public abstract class CPCBreakpoint {
        public Guid Identifier { get; } = Guid.NewGuid();
        public bool SingleHit { get; set; } = false;
        public abstract bool Hit(MachineStateSnapshot snapshot);
    }

    public class StepOverBreakpoint : CPCBreakpoint
    {   
        private readonly MachineStateSnapshot _previousState;

        public StepOverBreakpoint(MachineStateSnapshot previousState) {
            SingleHit = true;
            _previousState = previousState;
        }
        public override bool Hit(MachineStateSnapshot snapshot)
        {
            return snapshot.Cpu.NewInstruction && 
            snapshot.GateArray.CpuClock && 
            snapshot.Cpu.SP >= _previousState.Cpu.SP;
        }
    }

    public class StepBreakpoint : CPCBreakpoint
    {
        public StepBreakpoint() {
            SingleHit = true;
        }
        public override bool Hit(MachineStateSnapshot snapshot)
        {
            return snapshot.Cpu.NewInstruction && snapshot.GateArray.CpuClock;
        }
    }
}