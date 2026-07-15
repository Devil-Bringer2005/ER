namespace EndlessRunner.Player.Powerups.Effects
{
    using EndlessRunner.Collectibles;
    using UnityEngine;

    /// <summary>Instant effect - assign to a PowerupData with Duration &lt;= 0. Also a natural fit for a "starting currency" permanent upgrade.</summary>
    [CreateAssetMenu(menuName = "Powerups/Effects/Grant Currency", fileName = "New Grant Currency Effect")]
    public class GrantCurrencyEffect : PowerupEffect
    {
        [SerializeField] private int _amount = 50;

        public override void Activate(GameObject target) => CollectibleEvents.RaiseCurrencyGranted(_amount);

        public override void Deactivate(GameObject target) { }
    }
} 