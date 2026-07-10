using EndlessRunner.Player.Controls;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraDutchController : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler input;

    [Header("Dutch")]
    [SerializeField] private float maxDutch = 12f;
    [SerializeField] private float smoothTime = 0.15f;

    private CinemachineCamera cinemachineCamera;
    private float velocity;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
    }

    private void LateUpdate()
    {
        float targetDutch = -input.LateralInput * maxDutch;

        cinemachineCamera.Lens.Dutch = Mathf.SmoothDamp(
            cinemachineCamera.Lens.Dutch,
            targetDutch,
            ref velocity,
            smoothTime);
    }
}