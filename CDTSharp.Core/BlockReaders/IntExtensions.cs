namespace CDTSharp.Core.BlockReaders
{
    public static class IntExtensions {
        public static int AdjustTCycles(this int tCycles) {
            return (int)(tCycles * (4/3.5));
        }
    }
}
