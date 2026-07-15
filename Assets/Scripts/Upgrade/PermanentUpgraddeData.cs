namespace EndlessRunner.Player.Powerups
{
    using UnityEngine;

    /// <summary>
    /// A permanently-unlocked upgrade. Effects is the same array type PowerupData uses - a
    /// "Permanent Double Jump" upgrade can point at the exact same DoubleJumpEffect asset a
    /// temporary powerup uses, since the effect itself doesn't know whether it'll be reverted.
    /// Cost is left here for whatever shop/meta-progression UI ends up spending currency on
    /// these; this asset doesn't spend anything itself.
    /// </summary>
    [CreateAssetMenu(menuName = "Powerups/Permanent Upgrade", fileName = "New Permanent Upgrade")]
    public class PermanentUpgradeData : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private int _cost;
        [SerializeField] private PowerupEffect[] _effects;

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public int Cost => _cost;
        public PowerupEffect[] Effects => _effects;
    }
}