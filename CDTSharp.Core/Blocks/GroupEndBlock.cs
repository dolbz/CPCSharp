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