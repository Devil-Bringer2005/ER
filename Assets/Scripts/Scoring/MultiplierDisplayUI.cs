namespace EndlessRunner.Scoring.UI
{
    using UnityEngine;
    using TMPro;

    [AddComponentMenu("Scoring/UI/Multiplier Display")]
    public class MultiplierDisplayUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text multiplierText;
        [SerializeField] private string format = "x{0:0.0}";

        private void OnEnable()
        {
            ScoreManager.OnMultiplierChanged += HandleMultiplierChanged;
            if (ScoreManager.Instance != null)
                HandleMultiplierChanged(ScoreManager.Instance.Multiplier);
        }

        private void OnDisable()
        {
            ScoreManager.OnMultiplierChanged -= HandleMultiplierChanged;
        }

        private void HandleMultiplierChanged(float multiplier)
        {
            if (multiplierText != null)
                multiplierText.text = string.Format(format, multiplier);
        }
    }
}
