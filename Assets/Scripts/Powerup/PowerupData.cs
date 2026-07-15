namespace EndlessRunner.Player.Powerups
{
    using UnityEngine;

    /// <summary>
    /// Definition for a single powerup pickup. Duration &lt;= 0 means instant: PowerupController
    /// calls Activate once and never schedules a Deactivate (use this for coins, an instant
    /// meter fill, anything with nothing to revert). Duration &gt; 0 means Deactivate fires
    /// automatically once that many seconds have passed.
    /// </summary>
    [CreateAssetMenu(menuName = "Powerups/Powerup Data", fileName = "New Powerup")]
    public class PowerupData : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private float _duration;
        [SerializeField] private PowerupEffect[] _effects;

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public float Duration => _duration;
        public PowerupEffect[] Effects => _effects;
    }
}