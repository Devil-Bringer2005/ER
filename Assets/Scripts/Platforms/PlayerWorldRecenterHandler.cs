using UnityEngine;

namespace PlatformRunner.Platforms
{
    /// <summary>
    /// Use this instead of WorldRecenterSubscriber on any object driven by a
    /// CharacterController (e.g. the player).
    ///
    /// Why: CharacterController.Move() can synchronously fire OnTriggerEnter for
    /// triggers it sweeps into - including a BlockExitTrigger - *from inside the
    /// Move() call itself*. If you set transform.position right there (as a plain
    /// subscriber would), CharacterController.Move() is still on the stack above
    /// you and applies its own computed final position afterward, silently
    /// clobbering the recenter offset. So instead of applying the offset
    /// immediately, this queues it and applies it in LateUpdate - by which point
    /// Move() has fully returned - and briefly disables the controller while
    /// teleporting, which is the standard safe way to move a CharacterController
    /// outside of Move().
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerWorldRecenterHandler : MonoBehaviour
    {
        private CharacterController _controller;
        private Vector3 _pendingOffset;
        private bool _hasPendingOffset;

        private void Awake() => _controller = GetComponent<CharacterController>();

        private void OnEnable() => PlatformGenerator.OnWorldRecentered += QueueRecenter;
        private void OnDisable() => PlatformGenerator.OnWorldRecentered -= QueueRecenter;

        private void QueueRecenter(Vector3 offset)
        {
            _pendingOffset += offset;
            _hasPendingOffset = true;
        }

        private void LateUpdate()
        {
            if (!_hasPendingOffset) return;

            _controller.enabled = false;
            transform.position += _pendingOffset;
            _controller.enabled = true;

            _pendingOffset = Vector3.zero;
            _hasPendingOffset = false;
        }
    }
}
