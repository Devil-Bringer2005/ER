namespace EndlessRunner.Player.Combat
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Minimal player health pool implementing IDamageable so both enemy attacks and the
    /// collision system can damage the player through the same interface.
    /// </summary>
    [AddComponentMenu("Player/Combat/Player Health")]
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _maxHealth = 100f;

        public float MaxHealth => _maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        /// <summary>Fires on any change, with (current, max) - drive a health bar off this.</summary>
        public event Action<float, float> HealthChanged;
        public event Action Died;

        private void Awake() => CurrentHealth = _maxHealth;

        public void TakeDamage(float amount, GameObject source)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, _maxHealth);

            if (IsDead)
                Died?.Invoke();
        }
    }
}