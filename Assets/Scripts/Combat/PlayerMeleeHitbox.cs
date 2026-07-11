namespace EndlessRunner.Player.Combat
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [AddComponentMenu("Player/Combat/Player Melee Hitbox")]
    [RequireComponent(typeof(Collider))]
    public class PlayerMeleeHitbox : MonoBehaviour
    {
        [SerializeField] private Collider _hitboxCollider;

        private readonly HashSet<IDamageable> _hitThisSwing = new();
        private readonly HashSet<ProjectileDeflector> _deflectedThisSwing = new();

        private Coroutine _activeSwing;

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
            yield return new WaitForSeconds(data.StartupDelay);

            _hitThisSwing.Clear();
            _deflectedThisSwing.Clear();

            _hitboxCollider.enabled = true;

            yield return new WaitForSeconds(data.ActiveDuration);

            _hitboxCollider.enabled = false;

            _activeSwing = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_hitboxCollider.enabled)
                return;
            if (other.TryGetComponent(out ProjectileDeflector projectile) &&
                _deflectedThisSwing.Add(projectile))
            {
                projectile.Deflect(gameObject);
                return;
            }

            if (other.TryGetComponent(out IDamageable damageable) &&
                _hitThisSwing.Add(damageable))
            {
                damageable.TakeDamage(0, gameObject); // Replaced below
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_hitboxCollider.enabled)
                return;

            if (other.TryGetComponent(out ProjectileDeflector projectile) &&
                _deflectedThisSwing.Add(projectile))
            {
                projectile.Deflect(gameObject);
                return;
            }

            if (other.TryGetComponent(out IDamageable damageable) &&
                _hitThisSwing.Add(damageable))
            {
                damageable.TakeDamage(_currentDamage, gameObject);
            }
        }

        private float _currentDamage;

        private IEnumerator SwingRoutineInternal(MeleeAttackDefinitionSO data)
        {
            _currentDamage = data.Damage;

            yield return new WaitForSeconds(data.StartupDelay);

            _hitThisSwing.Clear();
            _deflectedThisSwing.Clear();

            _hitboxCollider.enabled = true;

            yield return new WaitForSeconds(data.ActiveDuration);

            _hitboxCollider.enabled = false;

            _activeSwing = null;
        }
    }
}