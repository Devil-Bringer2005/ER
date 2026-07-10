namespace EndlessRunner.Player.Combat
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Timed overlap check for melee attacks. Runs on its own delay/duration window so it
    /// works out of the box with no Animation Event setup required. If you want frame-perfect
    /// timing tied to the swing animation later, call Swing() from an Animation Event instead
    /// of from PlayerAttackController - nothing else needs to change.
    /// </summary>
    [AddComponentMenu("Player/Combat/Player Melee Hitbox")]
    public class PlayerMeleeHitbox : MonoBehaviour
    {
        [SerializeField] private Transform _origin;

        private readonly HashSet<IDamageable> _hitThisSwing = new HashSet<IDamageable>();
        private readonly Collider[] _overlapBuffer = new Collider[16];
        private Coroutine _activeSwing;

        private void Reset() => _origin = transform;

        public void Swing(MeleeAttackDefinitionSO data)
        {
            if (_activeSwing != null) StopCoroutine(_activeSwing);
            _activeSwing = StartCoroutine(SwingRoutine(data));
        }

        private IEnumerator SwingRoutine(MeleeAttackDefinitionSO data)
        {
            yield return new WaitForSeconds(data.StartupDelay);

            _hitThisSwing.Clear();
            float elapsed = 0f;
            while (elapsed < data.ActiveDuration)
            {
                CheckOverlap(data);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _activeSwing = null;
        }

        private void CheckOverlap(MeleeAttackDefinitionSO data)
        {
            Vector3 origin = _origin != null ? _origin.position : transform.position;
            Vector3 center = origin + transform.forward * (data.Range * 0.5f);
            int count = Physics.OverlapSphereNonAlloc(center, data.Radius, _overlapBuffer, data.HittableLayers, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                if (_overlapBuffer[i].TryGetComponent(out IDamageable damageable) && _hitThisSwing.Add(damageable))
                    damageable.TakeDamage(data.Damage, gameObject);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
            Vector3 origin = _origin != null ? _origin.position : transform.position;
            Gizmos.DrawWireSphere(origin + transform.forward * 0.75f, 0.75f);
        }
#endif
    }
}
