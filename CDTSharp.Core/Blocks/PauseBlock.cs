namespace CDTSharp.Core.Blocks
{
    public class PauseBlock : IBlock {
        public int PauseLength { get; set; }

        public string Description { 
            get 
            {
                return $"Pause for {PauseLength}ms";
            }
        }
    }
}