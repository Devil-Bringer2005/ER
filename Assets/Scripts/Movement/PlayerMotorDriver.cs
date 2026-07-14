namespace EndlessRunner.Player.Movement
{
    using UnityEngine;

    /// <summary>
    /// Composition root for movement: collects every IMovementContributor on this object,
    /// sums their frame displacement, and applies it through a single CharacterController.Move
    /// call. Lateral, forward, and vertical (gravity + jump) motion are each owned by their
    /// own motor component - this driver doesn't know or care which axes exist, it just sums
    /// whatever IMovementContributor components it finds. Add a dash/knockback motor later by
    /// dropping another IMovementContributor component on the player - picked up automatically.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("Player/Movement/Player Motor Driver")]
    public class PlayerMotorDriver : MonoBehaviour
    {
        private CharacterController _controller;
        private IMovementContributor[] _contributors;

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

            _controller.Move(movement);
        }
    }
}