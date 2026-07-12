namespace EndlessRunner.Scoring
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Holds current score and multiplier. Anything can call AddScore or
    /// IncreaseMultiplier directly - this class never needs to know about
    /// the things that call it, which is what keeps new multiplier factors
    /// a pure add, not an edit.
    ///
    /// Multiplier sources can be instant (apply once, stays until the next
    /// reset/penalty - the original behaviour) or timed (see
    /// MultiplierSourceSO.Duration): a timed source's contribution reverts
    /// itself automatically after its own duration. Either way, how much a
    /// single source can be contributing AT ONCE is capped by that source's
    /// own MaxStackedContribution (summed across every live trigger of it,
    /// timed or not) - so a source that's re-triggered repeatedly, like a
    /// sustained-speed bonus firing every couple seconds, plateaus instead
    /// of climbing forever.
    /// </summary>
    [AddComponentMenu("Scoring/Score Manager")]
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        [Header("Multiplier")]
        [SerializeField] private float startingMultiplier = 1f;
        [SerializeField] private float minMultiplier = 1f;
        [SerializeField] private float maxMultiplier = 10f;

        public float Score { get; private set; }
        public float Multiplier { get; private set; }
        public float MinMultiplier => minMultiplier;
        public float MaxMultiplier => maxMultiplier;

        public static event Action<float> OnScoreChanged;
        public static event Action<float> OnMultiplierChanged;
        public static event Action<MultiplierSourceSO> OnMultiplierSourceTriggered;
        public static event Action<MultiplierPenaltySourceSO> OnMultiplierPenaltyTriggered;

        /// <summary>
        /// Raised when a TIMED source's bonus starts counting down (instant
        /// sources never raise this). instanceId uniquely identifies THIS
        /// trigger, so UI can tell apart multiple concurrent stacks of the
        /// same source.
        /// </summary>
        public static event Action<MultiplierSourceSO, float, int> OnTimedMultiplierStarted;

        /// <summary>
        /// Raised when a timed source's bonus ends - naturally, or because
        /// everything was force-cleared (e.g. ResetMultiplier / a speed-gate
        /// reset). Always paired with exactly one prior
        /// OnTimedMultiplierStarted carrying the same instanceId.
        /// </summary>
        public static event Action<MultiplierSourceSO, int> OnTimedMultiplierExpired;

        private class ActiveBonus
        {
            public int InstanceId;
            public MultiplierSourceSO Source;
            public float Amount;

            /// <summary>Null for instant/permanent contributions - only timed ones auto-revert via a coroutine.</summary>
            public Coroutine Routine;
        }

        private readonly List<ActiveBonus> _activeBonuses = new List<ActiveBonus>();
        private int _nextBonusId;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Multiplier = startingMultiplier;
        }

        /// <summary>Adds score, scaled by the current multiplier.</summary>
        public void AddScore(float baseAmount)
        {
            if (baseAmount <= 0f) return;

            Score += baseAmount * Multiplier;
            OnScoreChanged?.Invoke(Score);
        }

        /// <summary>Bumps the multiplier by a raw amount, no source asset needed - not tracked or capped per-source. Use IncreaseMultiplier(MultiplierSourceSO) when a source's MaxStackedContribution should apply.</summary>
        public void IncreaseMultiplier(float amount)
        {
            if (amount <= 0f) return;

            Multiplier = Mathf.Min(Multiplier + amount, maxMultiplier);
            OnMultiplierChanged?.Invoke(Multiplier);
        }

        /// <summary>
        /// Bumps the multiplier from a configured source and reports which
        /// one, for UI/feedback. Instant or timed depending on the asset -
        /// see MultiplierSourceSO.Duration - and always respects that
        /// source's own MaxStackedContribution cap.
        /// </summary>
        public void IncreaseMultiplier(MultiplierSourceSO source)
        {
            if (source == null) return;

            ApplyBonus(source);

            OnMultiplierSourceTriggered?.Invoke(source);
        }

        private void ApplyBonus(MultiplierSourceSO source)
        {
            float amount = source.MultiplierIncrease;

            // Respect this source's OWN cap on how much it can be
            // contributing at once - summed across every currently-live
            // instance of it, timed or instant - independent of the global
            // maxMultiplier below. Stops one repeatedly-triggerable source
            // (a timed buff that stacks, or a plain instant bump fired on
            // an interval, e.g. HighSpeedMultiplierTrigger) from dominating
            // the whole multiplier by itself.
            if (source.MaxStackedContribution > 0f)
            {
                float alreadyContributing = _activeBonuses
                    .Where(b => b.Source == source)
                    .Sum(b => b.Amount);

                amount = Mathf.Clamp(source.MaxStackedContribution - alreadyContributing, 0f, amount);
                if (amount <= 0f) return; // already at this source's own cap - nothing more to add or track
            }

            float before = Multiplier;
            Multiplier = Mathf.Min(Multiplier + amount, maxMultiplier);
            float applied = Multiplier - before; // may be less than `amount` if the global max clamped it
            OnMultiplierChanged?.Invoke(Multiplier);

            if (applied <= 0f) return; // already at the global max too - nothing to actually track/revert later

            var bonus = new ActiveBonus
            {
                InstanceId = _nextBonusId++,
                Source = source,
                Amount = applied
            };

            if (source.IsTimed)
            {
                bonus.Routine = StartCoroutine(RevertBonus(bonus, source.Duration));
                OnTimedMultiplierStarted?.Invoke(source, source.Duration, bonus.InstanceId);
            }
            // Instant sources get no coroutine - their contribution simply
            // stays tracked (and counted against MaxStackedContribution)
            // until the next ResetMultiplier.

            _activeBonuses.Add(bonus);
        }

        private IEnumerator RevertBonus(ActiveBonus bonus, float duration)
        {
            yield return new WaitForSeconds(duration);

            _activeBonuses.Remove(bonus);
            Multiplier = Mathf.Max(Multiplier - bonus.Amount, minMultiplier);
            OnMultiplierChanged?.Invoke(Multiplier);
            OnTimedMultiplierExpired?.Invoke(bonus.Source, bonus.InstanceId);
        }

        /// <summary>Reduces the multiplier by a raw amount, no source asset needed. Floors at minMultiplier. Does NOT cancel or untrack active bonuses - use ResetMultiplier for a full wipe.</summary>
        public void DecreaseMultiplier(float amount)
        {
            if (amount <= 0f) return;

            Multiplier = Mathf.Max(Multiplier - amount, minMultiplier);
            OnMultiplierChanged?.Invoke(Multiplier);
        }

        /// <summary>
        /// Call from wherever the combo should break completely - e.g. on
        /// taking damage, or when speed drops below your gameplay threshold
        /// (see SpeedMultiplierGate). Wipes the value back to
        /// startingMultiplier AND clears every tracked bonus first
        /// (cancelling timed ones' coroutines, dropping instant ones'
        /// tracked contribution) so nothing tries to revert - or counts
        /// against a source's cap - after it's already gone.
        /// </summary>
        public void ResetMultiplier()
        {
            foreach (ActiveBonus bonus in _activeBonuses)
            {
                if (bonus.Routine != null)
                {
                    StopCoroutine(bonus.Routine);
                    OnTimedMultiplierExpired?.Invoke(bonus.Source, bonus.InstanceId);
                }
                // Instant bonuses (Routine == null) never raised
                // OnTimedMultiplierStarted, so there's nothing to pair an
                // expire event with - just drop their tracked contribution.
            }
            _activeBonuses.Clear();

            Multiplier = startingMultiplier;
            OnMultiplierChanged?.Invoke(Multiplier);
        }

        /// <summary>
        /// Convenience for the common case: award a source's direct score
        /// bonus AND bump the multiplier in one call. Prefer this over
        /// calling AddScore/IncreaseMultiplier separately at call sites.
        /// </summary>
        public void TriggerMultiplierSource(MultiplierSourceSO source)
        {
            if (source == null) return;

            AddScore(source.DirectScoreBonus);
            IncreaseMultiplier(source);
        }

        /// <summary>
        /// Applies a configured penalty - either a partial reduction or a
        /// full reset, depending on the asset - and reports which one, for
        /// UI/feedback.
        /// </summary>
        public void ApplyPenalty(MultiplierPenaltySourceSO penalty)
        {
            if (penalty == null) return;

            if (penalty.ResetsMultiplier)
                ResetMultiplier();
            else
                DecreaseMultiplier(penalty.MultiplierDecrease);

            OnMultiplierPenaltyTriggered?.Invoke(penalty);
        }
    }
}