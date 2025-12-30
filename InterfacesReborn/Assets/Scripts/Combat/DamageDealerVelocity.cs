using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Component that deals damage to IDamageable objects based on velocity.
    /// Only deals damage if the velocity exceeds a minimum threshold.
    /// Requires a Rigidbody component to track velocity.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class DamageDealerVelocity : DamageDealer
    {
        [Header("Velocity Settings")]
        [SerializeField] private float minimumVelocity = 2f;
        [Tooltip("Multiply damage by velocity factor")]
        [SerializeField] private bool scaleWithVelocity = true;
        [SerializeField] private float velocityDamageMultiplier = 0.5f;
        
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                Debug.LogError($"[DamageDealerVelocity] {gameObject.name} requiere un Rigidbody para funcionar!");
            }
        }

        protected override void OnCollisionEnter(Collision collision)
        {
            // Solo hacer daño si la velocidad supera el umbral
            if (_rigidbody != null && _rigidbody.linearVelocity.magnitude >= minimumVelocity)
            {
                // Escalar el daño basado en la velocidad
                if (scaleWithVelocity)
                {
                    float velocityFactor = _rigidbody.linearVelocity.magnitude * velocityDamageMultiplier;
                    float originalDamage = BaseDamage;
                    BaseDamage = originalDamage * velocityFactor;
                    
                    base.OnCollisionEnter(collision);
                    
                    // Restaurar daño base original
                    BaseDamage = originalDamage;
                }
                else
                {
                    base.OnCollisionEnter(collision);
                }
            }
        }
    }
}

