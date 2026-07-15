namespace EndlessRunner.Player.Combat
{
    using MoreMountains.Feedbacks;
    using MoreMountains.Tools;
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
        [SerializeField] private MMProgressBar[] progressBars;

        public float MaxHealth => _maxHealth;
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        // Counter rather than a bool so two overlapping invincibility powerups don't cancel
        // each other out - the second one's Deactivate only lifts its own contribution.
        private int _invincibilityCount;
        public bool IsInvincible => _invincibilityCount > 0;

        /// <summary>Fires on any change, with (current, max) - drive a health bar off this.</summary>
        public event Action<float, float> HealthChanged;
        public event Action Died;

        public MMF_Player playerHit;

        private float SegmentSize => _maxHealth / progressBars.Length;

        private void Awake()
        {
            CurrentHealth = _maxHealth;
            UpdateSegmentBars();
        }

        public void TakeDamage(float amount, GameObject source)
        {
            if (IsDead || amount <= 0f || IsInvincible) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            HealthChanged?.Invoke(CurrentHealth, _maxHealth);
            UpdateSegmentBars();

            playerHit.PlayFeedbacks();

            if (IsDead)
                Died?.Invoke();
        }

        /// <summary>Call paired with EndInvincibility - safe to call from multiple overlapping sources.</summary>
        public void BeginInvincibility() => _invincibilityCount++;

        /// <summary>Only clears invincibility once every BeginInvincibility call has a matching end.</summary>
        public void EndInvincibility() => _invincibilityCount = Mathf.Max(0, _invincibilityCount - 1);

        /// <summary>
        /// Raises or lowers the health pool. Positive amounts heal by the same amount so the
        /// bonus is felt immediately; negative amounts (undoing a bonus) just clamp current
        /// health down to the new ceiling instead of also subtracting from it.
        /// </summary>
        public void AddMaxHealthBonus(float amount)
        {
            if (amount == 0f) return;

            _maxHealth = Mathf.Max(0f, _maxHealth + amount);
            CurrentHealth = amount > 0f
                ? Mathf.Min(_maxHealth, CurrentHealth + amount)
                : Mathf.Min(CurrentHealth, _maxHealth);

            HealthChanged?.Invoke(CurrentHealth, _maxHealth);
            UpdateSegmentBars();
        }

        /// <summary>
        /// Splits CurrentHealth across progressBars evenly. The last index represents the
        /// topmost slice of the health pool, so it drains first; index 0 drains last.
        /// </summary>
        private void UpdateSegmentBars()
        {
            if (progressBars == null || progressBars.Length == 0) return;

            float segmentSize = SegmentSize;

            for (int i = 0; i < progressBars.Length; i++)
            {
                float segmentMin = i * segmentSize;
                float segmentCurrent = Mathf.Clamp(CurrentHealth, segmentMin, segmentMin + segmentSize) - segmentMin;

                progressBars[i].UpdateBar(segmentCurrent, 0f, segmentSize);
            }
        }
    }
}