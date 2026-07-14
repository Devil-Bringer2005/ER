namespace EndlessRunner.Player.Movement
{
    using MoreMountains.Tools;
    using UnityEngine;

    /// <summary>
    /// Drives an MMProgressBar off SpeedBoostMeter.ChargeChanged. Purely a UI concern -
    /// SpeedBoostMeter has no idea this exists, so swapping or removing the bar never touches
    /// gameplay code, and this same pattern works for any other meter that exposes a
    /// (current, max) event.
    /// </summary>
    [AddComponentMenu("Player/Movement/Speed Boost Meter UI")]
    public class SpeedBoostMeterUI : MonoBehaviour
    {
        [SerializeField] private SpeedBoostMeter _meter;
        [SerializeField] private MMProgressBar _progressBar;
        [SerializeField] private float updateFrequency = 0.1f;
        private float lastUpdateTime = -10f;

        private void OnEnable()
        {
            if (_meter == null) return;
            _meter.ChargeChanged += UpdateBar;
            UpdateBar(_meter.CurrentCharge, _meter.MaxCharge);
        }

        private void OnDisable()
        {
            if (_meter == null) return;
            _meter.ChargeChanged -= UpdateBar;
        }

        private void UpdateBar(float current, float max)
        {
            if (lastUpdateTime + updateFrequency > Time.time)
            {
                if (current == 0)
                    _progressBar.UpdateBar(current, 0f, max);

                return;
            }

            _progressBar.UpdateBar(current, 0f, max);
            lastUpdateTime = Time.time;
        }
    }
}