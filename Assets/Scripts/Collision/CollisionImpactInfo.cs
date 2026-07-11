namespace EndlessRunner.Player.Collision
{
    using UnityEngine;

    /// <summary>
    /// What PlayerCollisionHandler hands to listeners after processing a hit. Carries the
    /// resolved animator trigger name (owned by CollisionResponseConfig) so animation doesn't
    /// need its own copy of the classification-to-trigger mapping.
    /// </summary>
    public readonly struct CollisionImpactInfo
    {
        public readonly CollisionImpactType Type;
        public readonly Vector3 ContactPoint;
        public readonly string AnimatorTrigger;

        public CollisionImpactInfo(CollisionImpactType type, Vector3 contactPoint, string animatorTrigger)
        {
            Type = type;
            ContactPoint = contactPoint;
            AnimatorTrigger = animatorTrigger;
        }
    }
}
