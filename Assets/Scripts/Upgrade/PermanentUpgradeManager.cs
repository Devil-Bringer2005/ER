namespace EndlessRunner.Player.Powerups
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// The permanent counterpart to PowerupController: applies each unlocked upgrade's effects
    /// once, at run start, and never calls Deactivate. Which upgrades are unlocked is a
    /// meta-progression/save-data concern outside this script's scope - populate the list
    /// however your save system decides, this just applies it.
    /// </summary>
    [AddComponentMenu("Player/Powerups/Permanent Upgrade Manager")]
    public class PermanentUpgradeManager : MonoBehaviour
    {
        [SerializeField] private List<PermanentUpgradeData> _unlockedUpgrades = new List<PermanentUpgradeData>();

        private void Start()
        {
            foreach (PermanentUpgradeData upgrade in _unlockedUpgrades)
            {
                if (upgrade.Effects == null) continue;

                foreach (PowerupEffect effect in upgrade.Effects)
                    effect.Activate(gameObject);
            }
        }
    }
}