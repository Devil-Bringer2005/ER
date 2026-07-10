namespace EndlessRunner.Player.Movement
{
    using UnityEngine;

    /// <summary>
    /// Purely cosmetic lean into a strafe. Reads PlayerLateralMotor's normalized velocity
    /// and rolls a visual child transform - never the CharacterController root - so collision
    /// and gameplay state are completely unaffected by how far the model tilts.
    /// </summary>
    [AddComponentMenu("Player/Movement/Player Tilt Controller")]
    public class PlayerTiltController : MonoBehaviour
    {
        [SerializeField] private PlayerLateralMotor _lateralMotor;
        [SerializeField] private LateralMovementConfig _config;
        [SerializeField] private Transform _visualRoot;

        private float _currentAngle;
        private float _angleVelocity;

        private void Reset()
        {
            _lateralMotor = GetComponent<PlayerLateralMotor>();
        }

        private void LateUpdate()
        {
            if (_lateralMotor == null || _config == null || _visualRoot == null) return;

            float momentum = _lateralMotor.NormalizedVelocity;
            float easedFactor = _config.TiltCurve.Evaluate(Mathf.Abs(momentum)) * Mathf.Sign(momentum);
            float targetAngle = -easedFactor * _config.MaxTiltAngle;

            _currentAngle = Mathf.SmoothDamp(_currentAngle, targetAngle, ref _angleVelocity, _config.TiltSmoothTime);
            _visualRoot.localRotation = Quaternion.Euler(0f, 0f, _currentAngle);
        }
    }
}