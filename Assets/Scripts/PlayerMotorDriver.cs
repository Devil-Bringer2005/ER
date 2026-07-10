namespace EndlessRunner.Player.Movement
{
    using UnityEngine;

    /// <summary>
    /// Composition root for movement: collects every IMovementContributor on this object,
    /// sums their frame displacement, and applies it through a single CharacterController.Move
    /// call. Add a dash/knockback motor later by dropping another IMovementContributor
    /// component on the player - this driver picks it up automatically, no wiring needed.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Player/Movement/Player Motor Driver")]
    public class PlayerMotorDriver : MonoBehaviour
    {
        [SerializeField] private float _gravity = -20f;

        private CharacterController _controller;
        private IMovementContributor[] _contributors;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _contributors = GetComponents<IMovementContributor>();
        }

        private void Update()
        {
            Vector3 movement = Vector3.zero;
            for (int i = 0; i < _contributors.Length; i++)
                movement += _contributors[i].GetFrameMovement(Time.deltaTime);

            _verticalVelocity = _controller.isGrounded ? -1f : _verticalVelocity + _gravity * Time.deltaTime;
            movement += Vector3.up * (_verticalVelocity * Time.deltaTime);

            _controller.Move(movement);
        }
    }
}
