namespace EndlessRunner.Player.Powerups
{
    using System.Collections.Generic;
    using MoreMountains.Tools;
    using UnityEngine;
    using UnityEngine.UI;

    [AddComponentMenu("Player/Powerups/Active Powerup Bars UI")]
    public class ActivePowerupBarsUI : MonoBehaviour
    {
        [SerializeField] private PowerupController _controller;
        [SerializeField] private MMProgressBar _barPrefab;
        [SerializeField] private Transform _container;

        private class BarEntry
        {
            public PowerupData Data;
            public MMProgressBar Bar;
            public bool UpdatedThisFrame;
        }

        private readonly List<BarEntry> _entries = new();

        private void Update()
        {
            if (_controller == null)
                return;

            // Mark all as unused
            foreach (BarEntry entry in _entries)
                entry.UpdatedThisFrame = false;

            // Update existing bars or allocate new ones
            foreach (PowerupController.ActivePowerupInfo info in _controller.GetActivePowerups())
            {
                BarEntry entry = GetOrCreateEntry(info.Data);

                entry.UpdatedThisFrame = true;
                entry.Bar.UpdateBar(info.Remaining, 0f, info.Data.Duration);
            }

            // Disable only bars that disappeared
            foreach (BarEntry entry in _entries)
            {
                if (!entry.UpdatedThisFrame && entry.Bar.gameObject.activeSelf)
                {
                    entry.Bar.gameObject.SetActive(false);
                    entry.Data = null;
                }
            }
        }

        private BarEntry GetOrCreateEntry(PowerupData data)
        {
            // Already displaying this powerup
            foreach (BarEntry entry in _entries)
            {
                if (entry.Data == data)
                {
                    return entry;
                }
            }

            // Reuse an inactive bar
            foreach (BarEntry entry in _entries)
            {
                if (entry.Data == null)
                {
                    entry.Data = data;
                    ConfigureBar(entry.Bar, data);

                    if (!entry.Bar.gameObject.activeSelf)
                        entry.Bar.gameObject.SetActive(true);

                    return entry;
                }
            }

            // Create only if no reusable bar exists
            MMProgressBar bar = Instantiate(_barPrefab, _container);
            ConfigureBar(bar, data);

            BarEntry newEntry = new()
            {
                Data = data,
                Bar = bar,
                UpdatedThisFrame = false
            };

            _entries.Add(newEntry);
            return newEntry;
        }

        private static void ConfigureBar(MMProgressBar bar, PowerupData data)
        {
            Image fg = bar.ForegroundBar.GetComponent<Image>();
            Image dec = bar.DelayedBarDecreasing.GetComponent<Image>();
            Image inc = bar.DelayedBarIncreasing.GetComponent<Image>();

            fg.sprite = data.Icon;
            dec.sprite = data.Icon;
            inc.sprite = data.Icon;
        }
    }
}