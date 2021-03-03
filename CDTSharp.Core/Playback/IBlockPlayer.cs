namespace CDTSharp.Core.Playback
{
    public interface IBlockPlayer
    {
        bool IsComplete { get; }
        bool ClockTick();
    }
}
