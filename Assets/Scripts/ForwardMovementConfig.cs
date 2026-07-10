namespace EndlessRunner.Player.Movement
{
    using UnityEngine;

    /// <summary>
    /// Tunable data for the automatic forward run, including the endless-runner difficulty
    /// ramp. Fully independent of lateral tuning.
    /// </summary>
    [CreateAssetMenu(menuName = "Player/Movement/Forward Movement Config", fileName = "ForwardMovementConfig")]
    public class ForwardMovementConfig : ScriptableObject
    {
        [Tooltip("Forward speed at the start of a run, units/sec.")]
        [SerializeField] private float _baseSpeed = 6f;

        [Tooltip("How much forward speed is gained per second survived. 0 = constant speed.")]
        [SerializeField] private float _speedRampPerSecond = 0.15f;

        [Tooltip("Absolute cap on forward speed, units/sec.")]
        [SerializeField] private float _maxSpeed = 16f;

        public float BaseSpeed => _baseSpeed;
        public float SpeedRampPerSecond => _speedRampPerSecond;
        public float MaxSpeed => _maxSpeed;
    }
}
