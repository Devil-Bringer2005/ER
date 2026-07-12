namespace EndlessRunner.Scoring.UI
{
    using UnityEngine;
    using TMPro;
    using MoreMountains.Tools;

    /// <summary>
    /// One row/badge for a single active timed multiplier bonus. Spawned and
    /// torn down entirely by MultiplierTimerUI in response to ScoreManager's
    /// start/expire events - this component only owns the local visual
    /// countdown once told to Begin(); it never talks to ScoreManager
    /// itself.
    /// </summary>
    [AddComponentMenu("Scoring/UI/Multiplier Timer Entry")]
    public class MultiplierTimerEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private MMProgressBar progressBar;
        [SerializeField] private string format = "{0} {1:0.0}s";

        private MultiplierSourceSO _source;
        private float _totalDuration;
        private float _remaining;
        private bool _running;

        [SerializeField] private float threshold = 0.3f;
        private float lastTimeRefreshed = -10f;

        public void Begin(MultiplierSourceSO source, float duration)
        {
            _source = source;
            _totalDuration = Mathf.Max(duration, 0.01f);
            _remaining = _totalDuration;
            _running = true;

            Refresh();
        }

        /// <summary>
        /// Called by MultiplierTimerUI when the real bonus ends so this visual
        /// never outlives the thing it represents.
        /// </summary>
        public void EndAndDestroy()
        {
            _running = false;
            Destroy(gameObject);
        }

        private void Update()
        {
            if (!_running)
                return;

            _remaining = Mathf.Max(0f, _remaining - Time.deltaTime);

            Refresh();

            // Safety net only.
            if (_remaining <= 0f)
                _running = false;
        }

        private void Refresh()
        {
            if (label != null && _source != null)
                label.text = string.Format(format, _source.SourceName, _remaining);

            if (lastTimeRefreshed + threshold > Time.time)
                return;

            lastTimeRefreshed = Time.time;

            if (progressBar != null)
            {
                progressBar.UpdateBar01(_remaining / _totalDuration);
                //progressBar.UpdateBar(
                //    _remaining,
                //    0f,
                //    _totalDuration);
            }
        }
    }
}