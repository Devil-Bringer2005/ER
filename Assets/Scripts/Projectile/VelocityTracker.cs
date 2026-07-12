using UnityEngine;

/// <summary>
/// Tracks a Transform's world-space velocity by sampling position deltas.
/// Works no matter how the object actually moves (CharacterController,
/// manual transform, NavMeshAgent, kinematic or non-kinematic Rigidbody).
/// Attach to the player so enemies can read a live velocity for lead-aiming.
/// </summary>
public class VelocityTracker : MonoBehaviour
{
    [field: SerializeField] public Vector3 Velocity { get; private set; }

    [SerializeField] float sampleRate = 0.1f;

    private float timer;
    private Vector3 lastPosition;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < sampleRate)
            return;

        Velocity = (transform.position - lastPosition) / timer;
        lastPosition = transform.position;
        timer = 0f;
    }
}