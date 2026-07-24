namespace EndlessRunner.Player.Movement
{
    using UnityEngine;

    [RequireComponent(typeof(GroundSensor))]
    [AddComponentMenu("Player/Movement/Ground Stick Motor")]
    public class GroundStickMotor : MonoBehaviour, IMovementContributor
    {
        [SerializeField] private float _gravity = -30f;
        [SerializeField] private float _jumpHeight = 1.6f;
        [Tooltip("Small extra downward push while grounded so the controller's own collision check always registers a landing, even on a perfectly flat frame.")]
        [SerializeField] private float _groundedStickForce = 6f;
        [Tooltip("Safety clamp on how far the snap can pull the character down in one frame.")]
        [SerializeField] private float _maxSnapSpeed = 40f;

        private GroundSensor _sensor;
        private float _verticalVelocity;
        private bool _jumpRequested;

        private void Awake() => _sensor = GetComponent<GroundSensor>();

        public void RequestJump() => _jumpRequested = true;

        public Vector3 GetFrameMovement(float deltaTime)
        {
            if (_jumpRequested && _sensor.IsGrounded)
            {
                _jumpRequested = false;
                _verticalVelocity = Mathf.Sqrt(2f * -_gravity * _jumpHeight);
                return transform.up * (_verticalVelocity * deltaTime);
            }

            _jumpRequested = false;

            if (_sensor.IsGrounded && _verticalVelocity <= 0f)
            {
                _verticalVelocity = -_groundedStickForce;
                float snapDistance = Mathf.Min(_sensor.DistanceToGround, _maxSnapSpeed * deltaTime);
                return -transform.up * (snapDistance + _groundedStickForce * deltaTime);
            }

            _verticalVelocity += _gravity * deltaTime;
            return transform.up * (_verticalVelocity * deltaTime);
        }
    }
}