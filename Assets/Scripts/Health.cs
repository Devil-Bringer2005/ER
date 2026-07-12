using EndlessRunner.Player.Combat;
using MoreMountains.Feedbacks;
using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] public MMF_Player hitfeedback;

    private float _currentHealth;

    public event Action<float, float> OnHealthChanged; // current, max
    public event Action OnDeath;

    private void Awake()
    {
        _currentHealth = maxHealth;
    }

    public void TakeDamage(float damage, GameObject source)
    {
        if (_currentHealth <= 0f) return;

        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        if (_currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        OnDeath?.Invoke();
        // Hook up death animation, ragdoll, pooling, etc. here
        //gameObject.SetActive(false);
        hitfeedback?.PlayFeedbacks();
    }
}