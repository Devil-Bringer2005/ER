namespace EndlessRunner.Scoring.UI
{
    using UnityEngine;
    using TMPro;

    /// <summary>
    /// Spawns a short "+0.2x Parry" style popup for bonuses and a
    /// "-0.5x Hit" style popup for penalties - works for every current and
    /// future source/penalty asset with no changes here, since it just
    /// reads whatever SO fired. Purely cosmetic; safe to leave unused.
    /// </summary>
    [AddComponentMenu("Scoring/UI/Multiplier Popup Spawner")]
    public class MultiplierPopupUI : MonoBehaviour
    {
        [SerializeField] private GameObject popupPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private Color bonusColor = Color.green;
        [SerializeField] private Color penaltyColor = Color.red;

        private void OnEnable()
        {
            ScoreManager.OnMultiplierSourceTriggered += HandleBonusTriggered;
            ScoreManager.OnMultiplierPenaltyTriggered += HandlePenaltyTriggered;
        }

        private void OnDisable()
        {
            ScoreManager.OnMultiplierSourceTriggered -= HandleBonusTriggered;
            ScoreManager.OnMultiplierPenaltyTriggered -= HandlePenaltyTriggered;
        }

        private void HandleBonusTriggered(MultiplierSourceSO source)
        {
            SpawnPopup($"+{source.MultiplierIncrease:0.0}x {source.SourceName}", bonusColor);
        }

        private void HandlePenaltyTriggered(MultiplierPenaltySourceSO penalty)
        {
            string label = penalty.ResetsMultiplier ? "Reset" : $"-{penalty.MultiplierDecrease:0.0}x";
            SpawnPopup($"{label} {penalty.SourceName}", penaltyColor);
        }

        private void SpawnPopup(string message, Color color)
        {
            if (popupPrefab == null) return;

            Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
            GameObject popup = Instantiate(popupPrefab, position, Quaternion.identity, transform);

            if (popup.TryGetComponent(out TMP_Text text))
            {
                text.text = message;
                text.color = color;
            }

            Destroy(popup, lifetime);
        }
    }
}