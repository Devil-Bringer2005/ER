namespace EndlessRunner.Scoring
{
    using UnityEngine;

    /// <summary>
    /// Awards a multiplier bonus every triggerInterval seconds while the
    /// player's forward speed remains above the threshold.
    ///
    /// If the player's speed remains below the reset threshold for
    /// resetDelay seconds, the entire multiplier is reset.
    /// </summary>
    [AddComponentMenu("Scoring/Sources/High Speed Multiplier Trigger")]
    public class HighSpeedMultiplierTrigger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private VelocityTracker velocityTracker;
        [SerializeField] private MultiplierSourceSO source;

        [Header("Multiplier Gain")]
        [Tooltip("Minimum forward speed required to gain multiplier.")]
        [SerializeField] private float speedThreshold = 15f;

        [Tooltip("Time between multiplier gains while above the speed threshold.")]
        [SerializeField] private float triggerInterval = 2f;

        [Header("Combo Break")]
        [Tooltip("If speed remains below this value for Reset Delay seconds, the combo is broken.")]
        [SerializeField] private float resetSpeedThreshold = 5f;

        [Tooltip("How long the player must stay below the reset threshold before breaking the combo.")]
        [SerializeField] private float resetDelay = 0.5f;

        private float multiplierTimer;
        private float belowResetTimer;
        private bool comboBroken;

        private void Update()
        {
            if (velocityTracker == null)
                return;

            float currentSpeed = velocityTracker.Velocity.z;

            EvaluateComboBreak(currentSpeed);

            if (source == null)
                return;

            if (currentSpeed >= speedThreshold)
            {
                multiplierTimer += Time.deltaTime;

                if (multiplierTimer >= triggerInterval)
                {
                    multiplierTimer = 0f;
                    ScoreManager.Instance?.TriggerMultiplierSource(source);
                }
            }
            else
            {
                multiplierTimer = 0f;
            }
        }

        private void EvaluateComboBreak(float currentSpeed)
        {
            if (currentSpeed < resetSpeedThreshold)
            {
                belowResetTimer += Time.deltaTime;

                if (!comboBroken && belowResetTimer >= resetDelay)
                {
                    comboBroken = true;
                    ScoreManager.Instance?.ResetMultiplier();
                }
            }
            else
            {
                // Player has recovered speed.
                belowResetTimer = 0f;
                comboBroken = false;
            }
        }
    }
}