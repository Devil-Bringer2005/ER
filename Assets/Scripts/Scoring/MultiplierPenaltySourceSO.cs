namespace EndlessRunner.Scoring
{
    using UnityEngine;

    /// <summary>
    /// Data-only definition of one thing that penalizes the multiplier
    /// (taking damage, missing a dodge, hitting an obstacle, etc). Kept as
    /// its own type rather than reusing MultiplierSourceSO so a bonus and a
    /// penalty asset can never be mixed up by accident.
    ///
    /// Set resetsMultiplier to fully break the combo back to the starting
    /// value instead of a partial reduction - lets one asset type cover
    /// both "small penalty" and "hard reset" cases via data, not code.
    /// </summary>
    [CreateAssetMenu(fileName = "MultiplierPenalty", menuName = "Player/Scoring/Multiplier Penalty")]
    public class MultiplierPenaltySourceSO : ScriptableObject
    {
        [SerializeField] private string sourceName = "Penalty";
        [SerializeField] private float multiplierDecrease = 0.5f;
        [SerializeField] private bool resetsMultiplier = false;

        public string SourceName => sourceName;
        public float MultiplierDecrease => multiplierDecrease;
        public bool ResetsMultiplier => resetsMultiplier;
    }
}