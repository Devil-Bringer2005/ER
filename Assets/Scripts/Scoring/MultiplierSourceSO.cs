namespace EndlessRunner.Scoring
{
    using UnityEngine;

    /// <summary>
    /// Data-only definition of one thing that bumps the score multiplier
    /// (high speed, parry, dodge, close dodge, collectible, etc).
    /// Adding a new factor never touches ScoreManager - create a new asset
    /// here, then call ScoreManager.Instance.IncreaseMultiplier(asset) (or
    /// TriggerMultiplierSource(asset) to also award its direct score bonus)
    /// from wherever that event actually happens in gameplay.
    ///
    /// Set duration above 0 to make this a TIMED bonus: its contribution
    /// automatically reverts that many seconds after being triggered,
    /// instead of staying until the next reset/penalty. Leave it at 0 for
    /// the original instant/permanent behaviour.
    /// </summary>
    [CreateAssetMenu(fileName = "MultiplierSource", menuName = "Player/Scoring/Multiplier Source")]
    public class MultiplierSourceSO : ScriptableObject
    {
        [SerializeField] private string sourceName = "Source";
        [SerializeField] private float multiplierIncrease = 0.1f;
        [SerializeField] private float directScoreBonus = 0f;

        [Header("Timed Bonus (optional)")]
        [Tooltip("0 = instant/permanent, applied once like before. Above 0 = this bonus reverts itself automatically after this many seconds.")]
        [SerializeField, Min(0f)] private float duration = 0f;

        [Tooltip("Caps how much THIS source can be contributing at once, summed across every currently-live trigger of it - " +
            "whether timed or instant - independent of ScoreManager's overall max multiplier.")]
        [SerializeField, Min(0f)] private float maxStackedContribution = 0f;

        public string SourceName => sourceName;
        public float MultiplierIncrease => multiplierIncrease;
        public float DirectScoreBonus => directScoreBonus;
        public float Duration => duration;
        public bool IsTimed => duration > 0f;
        public float MaxStackedContribution => maxStackedContribution;
    }
}