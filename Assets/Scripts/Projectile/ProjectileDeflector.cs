namespace EndlessRunner.Player.Combat
{
    using MoreMountains.Feedbacks;
    using UnityEngine;

    /// <summary>
    /// Standalone, deflectable projectile - does not depend on Projectile.
    /// Handles its own movement, launch, and hit resolution. Use this
    /// instead of Projectile on prefabs that should be reflectable; use
    /// plain Projectile where deflection doesn't matter.
    ///
    /// Deflection is not self-detected - something else (e.g.
    /// PlayerMeleeHitbox) must call Deflect() directly once it determines
    /// this projectile was hit. Anything that touches this object's own
    /// trigger collider is resolved the normal way instead: damage any
    /// IDamageable found, then destroy.
    /// </summary>
    [AddComponentMenu("Player/Combat/Projectile Deflector")]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class ProjectileDeflector : MonoBehaviour
    {
        [Header("Reflect")]
        [SerializeField] private float returnSpeedMultiplier = 1.5f;
        [SerializeField] private float fallbackSpeed = 10f; // used if current speed is ~0 when reflected

        [Header("Homing")]
        [SerializeField] private float homingDuration = 0.5f; // seconds after launch the projectile actively steers
        [SerializeField] private float homingTurnRate = 180f;  // degrees/sec max turn while homing

        [Header("Feedback")]
        [SerializeField] private MMF_Player hitFeedback;
        [SerializeField] private MMF_Player deflectFeedback;
        [SerializeField] private MMF_Player deflectHitFeedback;

        private Vector3 _velocity;
        private float _damage;
        private GameObject _owner;
        private Transform _homingTarget;
        private float _homingTimer;
        private bool _deflected;

        private void Awake()
        {
            // Configured here so the prefab works correctly even if it was set up sloppily.
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            GetComponent<Collider>().isTrigger = true;
        }

        public void Launch(Vector3 direction, float speed, float damage, float lifetime, GameObject owner, Transform homingTarget = null)
        {
            _velocity = direction.normalized * speed;
            _damage = damage;
            _owner = owner;
            _homingTarget = homingTarget;
            _homingTimer = 0f;
            _deflected = false;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (_homingTarget != null && _homingTimer < homingDuration)
            {
                _homingTimer += Time.deltaTime;

                float speed = _velocity.magnitude;
                Vector3 currentDirection = _velocity.normalized;
                Vector3 desiredDirection = (_homingTarget.position - transform.position).normalized;

                Vector3 newDirection = Vector3.RotateTowards(
                    currentDirection,
                    desiredDirection,
                    homingTurnRate * Mathf.Deg2Rad * Time.deltaTime,
                    0f);

                _velocity = newDirection * speed;
            }

            transform.position += _velocity * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_owner != null && other.gameObject == _owner) return;

            if (other.TryGetComponent(out IDamageable damageable))
            {
                if (_deflected)
                {
                    deflectFeedback?.transform.SetParent(null);
                    deflectHitFeedback?.PlayFeedbacks();
                }
                else
                {
                    hitFeedback?.transform.SetParent(null);
                    hitFeedback?.PlayFeedbacks();
                }

                damageable.TakeDamage(_damage, _owner);
                Destroy(gameObject);
            }
        }

        /// <summary>Called by whatever hit this projectile (e.g. PlayerMeleeHitbox) to redirect it back toward its owner.</summary>
        public void Deflect(GameObject deflectedBy)
        {
            if (_owner == null) return;

            deflectFeedback?.PlayFeedbacks();

            Vector3 direction = (_owner.transform.position - transform.position).normalized;
            float speed = _velocity.magnitude;
            if (speed < 0.1f) speed = fallbackSpeed;

            _velocity = direction * (speed * returnSpeedMultiplier);
            _homingTarget = null; // fly straight back - don't keep homing on the original target
            _deflected = true;

            // Ownership flips to whoever deflected it - this is what lets the
            // projectile damage its original owner once it arrives (they're
            // no longer the exempt "owner"), and stops the same deflector
            // from immediately re-triggering it.
            _owner = deflectedBy;
        }
    }
}