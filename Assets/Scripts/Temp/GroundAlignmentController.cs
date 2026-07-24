namespace EndlessRunner.Player.Movement
{
    using UnityEngine;

    [RequireComponent(typeof(GroundSensor))]
    [AddComponentMenu("Player/Movement/Ground Alignment Controller")]
    public class GroundAlignmentController : MonoBehaviour
    {
        [SerializeField] private Transform _visualRoot;
        [Tooltip("Degrees/sec the visual root can rotate to catch up with the ground normal.")]
        [SerializeField] private float _alignSpeedDegPerSec = 480f;
        [Tooltip("Ignore normals steeper than this - treat it as a wall, not floor, and hold last alignment.")]
        [SerializeField] private float _maxAlignAngle = 75f;

        private GroundSensor _sensor;

        private void Awake() => _sensor = GetComponent<GroundSensor>();

        private void LateUpdate()
        {
            if (_visualRoot == null) return;

            Vector3 normal = _sensor.IsGrounded && Vector3.Angle(Vector3.up, _sensor.GroundNormal) <= _maxAlignAngle
                ? _sensor.GroundNormal
                : Vector3.up;

            // Rebuild rotation from the current forward direction so alignment never fights turning.
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, normal).normalized;
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

            Quaternion target = Quaternion.LookRotation(forward, normal);
            _visualRoot.rotation = Quaternion.RotateTowards(_visualRoot.rotation, target, _alignSpeedDegPerSec * Time.deltaTime);
        }
    }
}