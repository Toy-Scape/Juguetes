namespace CheckpointSystem
{
    /// <summary>
    /// Interface for any object that needs to be reset when the player dies/respawns.
    /// Implement this on enemies, moving platforms, or puzzles that should reset.
    /// </summary>
    public interface IResettable
    {
        void ResetState();
    }
}
