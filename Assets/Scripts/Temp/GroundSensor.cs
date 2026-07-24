namespace EndlessRunner.Player.Movement
{
    using UnityEngine;

    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Player/Movement/Ground Sensor")]
    public class GroundSensor : MonoBehaviour
    {
        [SerializeField] private LayerMask _groundMask = ~0;
        [Tooltip("How far below the controller's feet to probe for ground.")]
        [SerializeField] private float _probeDistance = 0.6f;
        [Tooltip("Shrink the cast radius slightly vs the controller so it doesn't catch side walls.")]
        [SerializeField] private float _radiusShrink = 0.02f;

        [SerializeField] private CharacterController _controller;

        public bool IsGrounded { get; private set; }
        public Vector3 GroundNormal { get; private set; } = Vector3.up;
        public Vector3 GroundPoint { get; private set; }
        public float DistanceToGround { get; private set; }

        private void Awake() => _controller = GetComponent<CharacterController>();

        private void Update()
        {
            float radius = Mathf.Max(0.01f, _controller.radius - _radiusShrink);
            Vector3 capsuleCenter = transform.position + _controller.center;
            Vector3 origin = capsuleCenter - Vector3.up * (_controller.height * 0.5f - radius);

            if (Physics.SphereCast(origin, radius, -transform.up, out RaycastHit hit,
                _probeDistance, _groundMask, QueryTriggerInteraction.Ignore))
            {
                IsGrounded = true;
                GroundNormal = hit.normal;
                GroundPoint = hit.point;
                DistanceToGround = hit.distance;
            }
            else
            {
                IsGrounded = false;
                GroundNormal = transform.up;
                DistanceToGround = _probeDistance;
            }
        }

        private void OnDrawGizmosSelected()
        {
            float radius = Mathf.Max(0.01f, _controller.radius - _radiusShrink);
            Vector3 capsuleCenter = transform.position + _controller.center;
            Vector3 origin = capsuleCenter - Vector3.up * (_controller.height * 0.5f - radius);


            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + (-transform.up * _probeDistance));
        }
    }
}