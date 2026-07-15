namespace EndlessRunner.Player.Powerups
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Runs every active timed powerup on this GameObject. Each pickup gets its own independent
    /// timer rather than refreshing/replacing an existing one of the same type, so two
    /// overlapping instances of the same powerup both expire on their own schedule - this only
    /// stays correct because the effects themselves are additive/ref-counted (see
    /// PlayerHealth.BeginInvincibility, PlayerJumpController.BonusAirJumps), never
    /// "set to a value".
    /// </summary>
    [AddComponentMenu("Player/Powerups/Powerup Controller")]
    public class PowerupController : MonoBehaviour
    {
        private class ActiveInstance
        {
            public PowerupData Data;
            public float Remaining;
        }

        private readonly List<ActiveInstance> _active = new List<ActiveInstance>();

        /// <summary>Fired the moment a powerup's effects are applied (both instant and timed).</summary>
        public event Action<PowerupData> PowerupActivated;

        /// <summary>Fired only for timed powerups, the moment their effects are reverted.</summary>
        public event Action<PowerupData> PowerupExpired;

        public void Activate(PowerupData data)
        {
            Debug.Log("Activate");
            if (data == null || data.Effects == null) return;
            foreach (PowerupEffect effect in data.Effects)
                effect.Activate(gameObject);

            PowerupActivated?.Invoke(data);

            if (data.Duration > 0f)
                _active.Add(new ActiveInstance { Data = data, Remaining = data.Duration });
        }

        private void Update()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                _active[i].Remaining -= Time.deltaTime;
                if (_active[i].Remaining > 0f) continue;

                foreach (PowerupEffect effect in _active[i].Data.Effects)
                    effect.Deactivate(gameObject);

                PowerupExpired?.Invoke(_active[i].Data);
                _active.RemoveAt(i);
            }
        }
    }
}