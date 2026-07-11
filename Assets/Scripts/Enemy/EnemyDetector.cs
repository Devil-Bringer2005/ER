using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Transform eyePoint;

    /// <summary>
    /// Returns true and outputs the player transform if the player is within
    /// range and there is a clear line of sight (no obstacles in between).
    /// </summary>
    public bool TryDetectPlayer(out Transform player)
    {
        player = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        if (hits.Length == 0) return false;

        Transform candidate = hits[0].transform;
        Vector3 origin = eyePoint ? eyePoint.position : transform.position;
        Vector3 toPlayer = candidate.position - origin;
        float distance = toPlayer.magnitude;

        if (Physics.Raycast(origin, toPlayer.normalized, out _, distance, obstacleLayer))
            return false; // view blocked by an obstacle

        player = candidate;
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}