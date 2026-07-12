namespace EndlessRunner.Player.Combat
{
    using MoreMountains.Feedbacks;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using EndlessRunner.Scoring;

    [AddComponentMenu("Player/Combat/Player Melee Hitbox")]
    [RequireComponent(typeof(Collider))]
    public class PlayerMeleeHitbox : MonoBehaviour
    {
        [SerializeField] private Collider _hitboxCollider;

        [Header("Feedback")]
        [SerializeField] private MMF_Player _reflectFeedback;
        [SerializeField] private MMF_Player _attackFeedback;

        [Header("Scoring")]
        [SerializeField] private MultiplierSourceSO _parryMultiplierSource;

        private readonly HashSet<IDamageable> _hitThisSwing = new();
        private readonly HashSet<ProjectileDeflector> _deflectedThisSwing = new();

        private Coroutine _activeSwing;
        private MeleeAttackDefinitionSO _activeData;

        private void Reset()
        {
            _hitboxCollider = GetComponent<Collider>();

            if (_hitboxCollider != null)
                _hitboxCollider.isTrigger = true;
        }

        private void Awake()
        {
            if (_hitboxCollider == null)
                _hitboxCollider = GetComponent<Collider>();

            _hitboxCollider.enabled = false;
        }

        public void Swing(MeleeAttackDefinitionSO data)
        {
            if (_activeSwing != null)
                StopCoroutine(_activeSwing);

            _activeSwing = StartCoroutine(SwingRoutine(data));
        }

        private IEnumerator SwingRoutine(MeleeAttackDefinitionSO data)
        {
            _activeData = data;

            yield return new WaitForSeconds(data.StartupDelay);

            _hitThisSwing.Clear();
            _deflectedThisSwing.Clear();

            _hitboxCollider.enabled = true;

            yield return new WaitForSeconds(data.ActiveDuration);

            _hitboxCollider.enabled = false;
            _activeData = null;
            _activeSwing = null;
        }

        // OnTriggerEnter covers the normal case; OnTriggerStay is a fallback
        // for targets that were already overlapping the hitbox the instant
        // it was enabled (Unity doesn't reliably fire Enter for pre-existing
        // overlaps). Both funnel into the same resolution, and the HashSets
        // guarantee each target only resolves once per swing either way.
        private void OnTriggerEnter(Collider other) => TryResolveHit(other);
        private void OnTriggerStay(Collider other) => TryResolveHit(other);

        private void TryResolveHit(Collider other)
        {
            if (!_hitboxCollider.enabled || _activeData == null) return;

            if (other.TryGetComponent(out ProjectileDeflector projectile) &&
                _deflectedThisSwing.Add(projectile))
            {
                _reflectFeedback?.PlayFeedbacks();
                projectile.Deflect(gameObject);

                if (_parryMultiplierSource != null)
                    ScoreManager.Instance?.TriggerMultiplierSource(_parryMultiplierSource);

                return;
            }

            if (other.TryGetComponent(out IDamageable damageable) &&
                _hitThisSwing.Add(damageable))
            {
                _attackFeedback?.PlayFeedbacks();
                damageable.TakeDamage(_activeData.Damage, gameObject);
            }
        }
    }
}