namespace PlatformRunner.Platforms
{
    /// <summary>
    /// The "type" of a connection point. An entry can only ever connect to an
    /// exit that shares the same category - this is what lets you build branching
    /// lanes (e.g. a high path can only continue into another block's "High" entry).
    /// Add/rename values freely for your game (Water, Rail, Air, Rooftop, ...).
    /// </summary>
    public enum ConnectorCategory
    {
        Ground,
        Left,
        Right,
        High,
        Low
    }
}
