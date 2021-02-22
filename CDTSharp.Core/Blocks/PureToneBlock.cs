namespace CDTSharp.Core.Blocks
{
    public class PureToneBlock : IBlock {
        public int PulseLength { get; init; }
        public int NumberOfPulses { get; init; }

        public string Description { 
            get 
            {
                return "Pure Tone Block\n" +
                       "---------------\n" +
                       $"{NumberOfPulses} pulses of {PulseLength} T cycles long";
            }
        }
    }
}