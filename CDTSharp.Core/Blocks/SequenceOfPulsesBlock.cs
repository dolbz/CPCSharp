using System.Linq;
namespace CDTSharp.Core.Blocks
{
    public class SequenceOfPulsesBlock : IBlock {
        public int[] PulseLengths { get; set; }

        public string Description { 
            get 
            {
                return "Sequence Of Pulses Of Different Lengths\n" +
                       "---------------------------------------\n" +
                       $"{PulseLengths.Length} pulses.\n" +
                       $"[ {string.Concat(PulseLengths.Select(x => $"{x},\n"))} ]\n";
            }
        }
    }
}