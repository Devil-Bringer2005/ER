using EndlessRunner.Player.Combat;
using System;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth health;
    [SerializeField] private GameObject endPanel;

    private void Start()
    {
        health.Died += HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        endPanel.SetActive(true);
    }

    private void OnDestroy()
    {
        health.Died -= HandlePlayerDeath;
    }
}
