namespace EndlessRunner.Player.Movement
{
    using UnityEngine;

    /// <summary>
    /// Tuning data for PlayerHeadingMotor. Same acceleration-based feel as the old
    /// lateral movement config, but expressed in degrees/sec instead of units/sec
    /// since this motor rotates the player rather than translating it.
    /// </summary>
    [CreateAssetMenu(fileName = "HeadingMovementConfig", menuName = "Player/Movement/Heading Movement Config")]
    public class HeadingMovementConfig : ScriptableObject
    {
        [Tooltip("Top angular speed the player can turn at, in degrees/sec.")]
        [SerializeField] private float _maxTurnSpeed = 180f;

        [Tooltip("How fast angular velocity ramps up toward MaxTurnSpeed while turn input is held, in degrees/sec^2.")]
        [SerializeField] private float _acceleration = 360f;

        [Tooltip("How fast angular velocity decays back toward zero once turn input is released, in degrees/sec^2. " +
                 "Usually higher than Acceleration so the heading settles quickly instead of drifting.")]
        [SerializeField] private float _deceleration = 540f;

        [Tooltip("Maximum degrees the heading is allowed to drift from its starting orientation, in either direction. " +
                 "0 = unlimited (free 360-degree turning, e.g. an open field). " +
                 "Set this if the level is a lane/corridor and the player shouldn't be able to turn all the way around.")]
        [SerializeField] private float _maxHeadingAngle = 0f;

        public float MaxTurnSpeed => _maxTurnSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float MaxHeadingAngle => _maxHeadingAngle;
    }
}