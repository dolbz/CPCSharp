using System.Linq;
namespace CDTSharp.Core.Blocks
{
    public class GroupStartBlock : IBlock {
        public string GroupName { get; init; }

        public string Description { 
            get 
            {
                return $"Group Start: {GroupName}\n" +
                       $"-------------{new string(GroupName.Select(x => '-').ToArray())}";
            }
        }
    }
}