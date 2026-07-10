namespace EndlessRunner.Player.Movement
{
    using UnityEngine;

    /// <summary>
    /// Tunable data for left/right strafing and its cosmetic tilt. Kept separate from
    /// forward-movement data so a designer can reuse or swap lateral feel (e.g. a "drifty"
    /// vs "snappy" variant) without touching the auto-run tuning at all.
    /// </summary>
    [CreateAssetMenu(menuName = "Player/Movement/Lateral Movement Config", fileName = "LateralMovementConfig")]
    public class LateralMovementConfig : ScriptableObject
    {
        [Header("Speed")]
        [Tooltip("Units/sec^2 gained while input is held toward a direction.")]
        [SerializeField] private float _acceleration = 18f;

        [Tooltip("Units/sec^2 lost when input is released or reversed.")]
        [SerializeField] private float _deceleration = 24f;

        [Tooltip("Absolute cap on lateral speed, units/sec.")]
        [SerializeField] private float _maxSpeed = 8f;

        [Header("Lane Bounds")]
        [Tooltip("Distance from the center lane the player may travel. 0 = unbounded.")]
        [SerializeField] private float _laneHalfWidth = 4f;

        [Header("Tilt")]
        [Tooltip("Maps clamped lateral acceleration [0,1] to a tilt easing factor [0,1].")]
        [SerializeField] private AnimationCurve _tiltCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Maximum roll angle applied at full clamped acceleration, in degrees.")]
        [SerializeField] private float _maxTiltAngle = 25f;

        [Tooltip("Smoothing time for the tilt to catch up to its target angle.")]
        [SerializeField] private float _tiltSmoothTime = 0.12f;

        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float MaxSpeed => _maxSpeed;
        public float LaneHalfWidth => _laneHalfWidth;
        public AnimationCurve TiltCurve => _tiltCurve;
        public float MaxTiltAngle => _maxTiltAngle;
        public float TiltSmoothTime => _tiltSmoothTime;
    }
}
