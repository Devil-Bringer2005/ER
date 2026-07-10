using Unity.Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CinemachineImpulseSource))]
public sealed class CameraShake : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;

    private void Reset()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Awake()
    {
        if (impulseSource == null)
            impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    /// <summary>
    /// Uses the default impulse defined on the source.
    /// </summary>
    public void Shake()
    {
        impulseSource.GenerateImpulse();
    }

    /// <summary>
    /// Shake with configurable intensity.
    /// </summary>
    public void Shake(float intensity)
    {
        impulseSource.GenerateImpulse(Vector3.forward * intensity);
    }

    /// <summary>
    /// Shake in a specific direction.
    /// </summary>
    public void Shake(Vector3 direction, float intensity)
    {
        impulseSource.GenerateImpulse(direction.normalized * intensity);
    }
}