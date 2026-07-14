namespace EndlessRunner.Player.Movement
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Charge pool filled by BoostMeterCollectibleData pickups and drained by SpeedBoostMotor
    /// while boosting. Neither side references the other directly - both go through this.
    /// </summary>
    [AddComponentMenu("Player/Movement/Speed Boost Meter")]
    public class SpeedBoostMeter : MonoBehaviour
    {
        [SerializeField] private float _maxCharge = 100f;

        public float MaxCharge => _maxCharge;
        public float CurrentCharge { get; private set; }
        public bool HasCharge => CurrentCharge > 0f;

        /// <summary>Fires on any change, with (current, max) - drive a meter UI off this.</summary>
        public event Action<float, float> ChargeChanged;

        public void AddCharge(float amount)
        {
            if (amount <= 0f) return;
            CurrentCharge = Mathf.Min(_maxCharge, CurrentCharge + amount);
            ChargeChanged?.Invoke(CurrentCharge, _maxCharge);
        }

        public void Drain(float amount)
        {
            if (amount <= 0f || CurrentCharge <= 0f) return;
            CurrentCharge = Mathf.Max(0f, CurrentCharge - amount);
            ChargeChanged?.Invoke(CurrentCharge, _maxCharge);
        }
    }
}