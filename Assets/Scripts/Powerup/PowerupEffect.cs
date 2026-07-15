namespace EndlessRunner.Player.Powerups
{
    using UnityEngine;

    /// <summary>
    /// One reusable unit of gameplay effect. The same asset can back a temporary PowerupData
    /// (Activate on pickup, Deactivate when its duration runs out) or a permanent
    /// PermanentUpgradeData (Activate once at run start, Deactivate never called) - the effect
    /// itself doesn't know or care which. Keep each subclass reaching into exactly one system
    /// via GetComponent, the same discovery pattern the rest of the player scripts use.
    /// </summary>
    public abstract class PowerupEffect : ScriptableObject
    {
        public abstract void Activate(GameObject target);

        /// <summary>Undo whatever Activate did. Safe to leave as a no-op for effects that are inherently one-shot (currency, instant meter fill).</summary>
        public abstract void Deactivate(GameObject target);
    }
}