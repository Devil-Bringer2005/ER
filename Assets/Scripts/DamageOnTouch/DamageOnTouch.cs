using EndlessRunner.Player.Combat;
using MoreMountains.Feedbacks;
using UnityEngine;

public class DamageOnTouch : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private MMF_Player feedback;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(damage, gameObject);
            feedback.PlayFeedbacks();
        }
    }
}
