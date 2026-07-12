namespace EndlessRunner.Scoring.UI
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using MoreMountains.Tools;
    using System;

    /// <summary>
    /// Alternate multiplier display: a fill gauge from MinMultiplier to
    /// MaxMultiplier, with a brief color pulse whenever a bonus or penalty
    /// fires. Entirely event-driven, matching the pattern used by
    /// ScoreDisplayUI/MultiplierDisplayUI - only reacts to ScoreManager's
    /// static events, never polls ScoreManager.Instance.Multiplier in
    /// Update. The only Update usage here is the pulse animation itself,
    /// which is purely a local visual effect, not a read of game state.
    /// </summary>
    [AddComponentMenu("Scoring/UI/Multiplier Gauge")]
    public class MultiplierGaugeUI : MonoBehaviour
    {
        [SerializeField] private MMProgressBar finalScoreMultiplier;
        [SerializeField] private TextMeshProUGUI multiplierText;

        private void Start()
        {
            ScoreManager.OnMultiplierChanged += HandleScoreMultiplierUpdate;
        }

        private void HandleScoreMultiplierUpdate(float multiplier)
        {
            string value = multiplier.ToString("0.00");
            int dot = value.IndexOf('.');

            if (dot >= 0)
            {
                string whole = value[..dot];
                string fraction = value[dot..];

                multiplierText.text = $"x{whole}<size=70%>{fraction}</size>";
            }
            else
            {
                multiplierText.text = $"x{value}";
            }

            finalScoreMultiplier.UpdateBar(multiplier,
                                           ScoreManager.Instance.MinMultiplier,
                                           ScoreManager.Instance.MaxMultiplier);
        }

        private void OnDestroy()
        {
            ScoreManager.OnMultiplierChanged -= HandleScoreMultiplierUpdate;
        }
    }
}