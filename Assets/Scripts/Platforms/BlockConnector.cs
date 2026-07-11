using UnityEngine;

namespace PlatformRunner.Platforms
{
    public enum ConnectorType { Entry, Exit }

    /// <summary>
    /// Marks a single connection point on a block prefab. Place these as empty
    /// child GameObjects sitting exactly at the seam, with local +Z (forward)
    /// pointing outward in the direction of travel. A block can have several of
    /// these - e.g. one Entry plus three Exits (Ground/Left/Right) to branch paths.
    /// </summary>
    public class BlockConnector : MonoBehaviour
    {
        [SerializeField] private ConnectorType connectorType;
        [SerializeField] private ConnectorCategory category;

        public ConnectorType Type => connectorType;
        public ConnectorCategory Category => category;

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = connectorType == ConnectorType.Entry ? Color.cyan : Color.magenta;
            Gizmos.DrawSphere(transform.position, 0.25f);
            Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
        }
#endif
    }
}
