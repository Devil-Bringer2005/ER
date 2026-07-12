namespace EndlessRunner.Scoring.UI
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Spawns one countdown entry per active timed MultiplierSourceSO
    /// instance and removes it the moment that instance ends - whether it
    /// expired naturally or ScoreManager force-cleared everything (e.g. the
    /// speed dropped below threshold). Purely event-driven: this script
    /// never polls ScoreManager, it only reacts to
    /// OnTimedMultiplierStarted/Expired. Each spawned entry then runs its
    /// own local countdown visual (see MultiplierTimerEntryUI).
    /// </summary>
    [AddComponentMenu("Scoring/UI/Multiplier Timer Spawner")]
    public class MultiplierTimerUI : MonoBehaviour
    {
        [Tooltip("Prefab with a MultiplierTimerEntryUI component - one instance is spawned per active timed bonus.")]
        [SerializeField] private MultiplierTimerEntryUI entryPrefab;

        [Tooltip("Parent entries are spawned under. Defaults to this transform if left empty.")]
        [SerializeField] private Transform container;

        private readonly Dictionary<int, MultiplierTimerEntryUI> _activeEntries = new Dictionary<int, MultiplierTimerEntryUI>();

        private void OnEnable()
        {
            ScoreManager.OnTimedMultiplierStarted += HandleStarted;
            ScoreManager.OnTimedMultiplierExpired += HandleExpired;
        }

        private void OnDisable()
        {
            ScoreManager.OnTimedMultiplierStarted -= HandleStarted;
            ScoreManager.OnTimedMultiplierExpired -= HandleExpired;
        }

        private void HandleStarted(MultiplierSourceSO source, float duration, int instanceId)
        {
            if (entryPrefab == null) return;

            Transform parent = container != null ? container : transform;
            MultiplierTimerEntryUI entry = Instantiate(entryPrefab, parent);
            entry.Begin(source, duration);
            _activeEntries[instanceId] = entry;
        }

        private void HandleExpired(MultiplierSourceSO source, int instanceId)
        {
            if (!_activeEntries.TryGetValue(instanceId, out MultiplierTimerEntryUI entry)) return;

            _activeEntries.Remove(instanceId);
            if (entry != null) entry.EndAndDestroy();
        }
    }
}