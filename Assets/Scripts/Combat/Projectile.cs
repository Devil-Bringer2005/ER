namespace EndlessRunner.Player.Combat
{
    using UnityEngine;

    [AddComponentMenu("Player/Combat/Projectile")]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        private Vector3 _velocity;
        private float _damage;
        private GameObject _owner;

        private void Awake()
        {
            // Configured here so the prefab works correctly even if it was set up sloppily.
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            GetComponent<Collider>().isTrigger = true;
        }

        public void Launch(Vector3 direction, float speed, float damage, float lifetime, GameObject owner)
        {
            _velocity = direction.normalized * speed;
            _damage = damage;
            _owner = owner;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.position += _velocity * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_owner != null && other.gameObject == _owner) return;

            if (other.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(_damage, _owner);

            Destroy(gameObject);
        }
    }
}