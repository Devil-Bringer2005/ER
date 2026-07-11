namespace PlatformRunner.Spawning
{
    /// <summary>
    /// Shared category tag used by both SpawnPoint (where something CAN be
    /// spawned) and SpawnablePlacement (what CAN be spawned there). The
    /// spawner only ever pairs a spawnable with a spawn point of a matching
    /// category. Edit this list to match your game's actual content (add
    /// Enemy, Hazard, Checkpoint, etc. as needed).
    /// </summary>
    public enum SpawnCategory
    {
        Collectible,
        Obstacle,
        Decoration,
        PowerUp
    }
}
