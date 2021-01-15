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