using System;

namespace CDTSharp.Core.Blocks
{
    public class StandardSpeedBlock : IBlock
    {
        public string Description { 
            get 
            {
                var desc = 
                       "Standard Speed Block\n" +
                       "--------------------\n" +
                       $"Pause length after block: {PauseLengthAfterBlock}\n" +
                       $"Length of data: {LengthOfBlockData}\n";
                return desc;
            }
        }

        public int PauseLengthAfterBlock { get; internal set; }
        public int LengthOfBlockData { get; internal set; }
        public byte[] Data { get; internal set; }
    }
}