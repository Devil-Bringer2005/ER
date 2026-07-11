namespace EndlessRunner.Player.Collision
{
    /// <summary>The three impact tiers PlayerCollisionHandler classifies a hit into.</summary>
    public enum CollisionImpactType
    {
        /// <summary>Obstacle's surface normal points mostly sideways relative to the player - a graze along the flank.</summary>
        SideOn,

        /// <summary>Frontal hit, but the contact point is offset from the player's center line beyond the dead-center threshold.</summary>
        OffCenterFrontal,

        /// <summary>Frontal hit landing within the dead-center threshold of the player's center line.</summary>
        DeadCenterFrontal
    }
}
