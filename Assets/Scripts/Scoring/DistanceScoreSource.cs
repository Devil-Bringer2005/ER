namespace EndlessRunner.Scoring
{
    using UnityEngine;
    using EndlessRunner.Player.Movement;

    /// <summary>
    /// Base score source: awards score every frame proportional to distance
    /// travelled (speed * deltaTime), scaled by the current multiplier
    /// through ScoreManager.AddScore. This is what the multiplier factors
    /// actually end up affecting - without a base source like this,
    /// increasing the multiplier has nothing to multiply.
    /// </summary>
    [AddComponentMenu("Scoring/Sources/Distance Score Source")]
    public class DistanceScoreSource : MonoBehaviour
    {
        [SerializeField] private PlayerForwardMotor forwardMotor;
        [SerializeField] private float pointsPerUnit = 1f;

        private void Update()
        {
            if (forwardMotor == null) return;
            ScoreManager.Instance?.AddScore(forwardMotor.CurrentSpeed * pointsPerUnit * Time.deltaTime);
        }
    }
}
