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

        /// <summary>Read-only snapshot of one active instance, for UI to poll without seeing ActiveInstance itself.</summary>
        public readonly struct ActivePowerupInfo
        {
            public readonly PowerupData Data;
            public readonly float Remaining;

            public ActivePowerupInfo(PowerupData data, float remaining)
            {
                Data = data;
                Remaining = remaining;
            }
        }

        private readonly List<ActiveInstance> _active = new List<ActiveInstance>();

        /// <summary>Fired the moment a powerup's effects are applied (both instant and timed).</summary>
        public event Action<PowerupData> PowerupActivated;

        /// <summary>Fired only for timed powerups, the moment their effects are reverted.</summary>
        public event Action<PowerupData> PowerupExpired;

        /// <summary>
        /// True if at least one instance of this powerup is still active. Remaining is the
        /// longest time left across every overlapping instance of it - i.e. time until this
        /// powerup is no longer active at all, which is what a single "is it still on" bar
        /// should show.
        /// </summary>
        public bool TryGetRemaining(PowerupData data, out float remaining)
        {
            remaining = 0f;
            bool found = false;

            foreach (ActiveInstance instance in _active)
            {
                if (instance.Data != data) continue;
                found = true;
                if (instance.Remaining > remaining) remaining = instance.Remaining;
            }

            return found;
        }

        /// <summary>One entry per active instance (not merged by type) - matches how Activate/Update track them internally.</summary>
        public IEnumerable<ActivePowerupInfo> GetActivePowerups()
        {
            foreach (ActiveInstance instance in _active)
                yield return new ActivePowerupInfo(instance.Data, instance.Remaining);
        }

        public void Activate(PowerupData data)
        {
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