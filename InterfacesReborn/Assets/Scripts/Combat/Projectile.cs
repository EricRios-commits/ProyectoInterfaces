using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Simple projectile component that moves forward and deactivates after lifetime.
    /// Attach this to your projectile prefabs.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : DamageDealer
    {
        private float _lifetime;
        private float _spawnTime;
        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Initialize the projectile with its settings.
        /// Called when the projectile is retrieved from the pool.
        /// </summary>
        public void Initialize(Vector3 position, Vector3 direction, float speed, float lifetime)
        {
            _lifetime = lifetime;
            _spawnTime = Time.time;
            transform.position = position;
            transform.forward = direction;
            _rb.linearVelocity = direction.normalized * speed;
        }

        private void Update()
        {
            if (Time.time - _spawnTime >= _lifetime)
            {
                ReturnToPool();
            }
        }

        protected override void OnCollisionEnter(Collision collision)
        {
            base.OnCollisionEnter(collision);
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            _rb.linearVelocity = Vector3.zero;
            gameObject.SetActive(false);
        }
    }
}

