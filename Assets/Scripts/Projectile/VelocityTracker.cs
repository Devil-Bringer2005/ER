using UnityEngine;

/// <summary>
/// Tracks a Transform's world-space velocity by sampling position deltas.
/// Works no matter how the object actually moves (CharacterController,
/// manual transform, NavMeshAgent, kinematic or non-kinematic Rigidbody).
/// Attach to the player so enemies can read a live velocity for lead-aiming.
/// </summary>
public class VelocityTracker : MonoBehaviour
{
    public Vector3 Velocity { get; private set; }

    private Vector3 _lastPosition;

    private void Start()
    {
        _lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        Velocity = (transform.position - _lastPosition) / Time.fixedDeltaTime;
        _lastPosition = transform.position;
    }
}