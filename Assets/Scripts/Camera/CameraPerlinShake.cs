using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public sealed class CameraPerlinShake : MonoBehaviour
{
    [SerializeField] private CinemachineBasicMultiChannelPerlin perlin;

    private Coroutine shakeRoutine;

    private void Reset()
    {
        perlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Awake()
    {
        if (perlin == null)
            perlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void Shake(float amplitude, float frequency, float duration)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(amplitude, frequency, duration));
    }

    public void SetShake(float amplitude, float frequency)
    {
        perlin.AmplitudeGain = amplitude;
        perlin.FrequencyGain = frequency;
    }

    public void StopShake(float fadeTime = 0.15f)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(FadeOut(fadeTime));
    }

    private IEnumerator ShakeRoutine(float amplitude, float frequency, float duration)
    {
        perlin.AmplitudeGain = amplitude;
        perlin.FrequencyGain = frequency;

        yield return new WaitForSeconds(duration);

        yield return FadeOut(0.15f);
    }

    private IEnumerator FadeOut(float duration)
    {
        float startAmplitude = perlin.AmplitudeGain;
        float startFrequency = perlin.FrequencyGain;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = t / duration;

            perlin.AmplitudeGain = Mathf.Lerp(startAmplitude, 0f, alpha);
            perlin.FrequencyGain = Mathf.Lerp(startFrequency, 0f, alpha);

            yield return null;
        }

        perlin.AmplitudeGain = 0f;
        perlin.FrequencyGain = 0f;

        shakeRoutine = null;
    }

    [ContextMenu("Test Shake")]
    public void TryShake()
    {
        Shake(1, 1, 1);
    }
}