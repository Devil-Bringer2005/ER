namespace EndlessRunner.Scoring.UI
{
    using UnityEngine;
    using TMPro;

    [AddComponentMenu("Scoring/UI/Score Display")]
    public class ScoreDisplayUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private string format = "{0:N0}";

        private void OnEnable()
        {
            ScoreManager.OnScoreChanged += HandleScoreChanged;
            if (ScoreManager.Instance != null)
                HandleScoreChanged(ScoreManager.Instance.Score);
        }

        private void OnDisable()
        {
            ScoreManager.OnScoreChanged -= HandleScoreChanged;
        }

        private void HandleScoreChanged(float score)
        {
            if (scoreText != null)
                scoreText.text = string.Format(format, score);
        }
    }
}
