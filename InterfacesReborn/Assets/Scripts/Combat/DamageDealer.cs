using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Component that deals damage to IDamageable objects.
    /// Follows Single Responsibility Principle - only handles damage dealing.
    /// </summary>
    public class DamageDealer : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private float baseDamage = 10f;
        [SerializeField] private DamageType damageType = DamageType.Slash;
        [SerializeField] private bool dealDamageOnCollision = true;
        [SerializeField] private LayerMask damageableLayers = ~0;

        public float BaseDamage { get => baseDamage; set => baseDamage = value; }
        public DamageType DamageType { get => damageType; set => damageType = value; }

        /// <summary>
        /// Deal damage to a specific target.
        /// </summary>
        protected virtual void DealDamage(IDamageable target, Vector3 hitPoint = default, Vector3 hitDirection = default)
        {
            if (target == null || !target.IsAlive)
                return;
            DamageInfo damageInfo = new DamageInfo(
                baseDamage,
                damageType,
                gameObject,
                hitPoint,
                hitDirection
            );
            target.TakeDamage(damageInfo);
        }
        
        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (!dealDamageOnCollision)
                return;
            if (!IsInLayerMask(collision.gameObject.layer, damageableLayers))
                return;
            IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : collision.transform.position;
                Vector3 hitDirection = collision.contacts.Length > 0 ? collision.contacts[0].normal : Vector3.zero;
                DealDamage(damageable, hitPoint, hitDirection);
            }
        }
        
        protected virtual void OnTriggerEnter(Collider other)
        {
            Debug.Log("Dealing damage to " + other.gameObject.name);
            if (!dealDamageOnCollision)
                return;
            if (!IsInLayerMask(other.gameObject.layer, damageableLayers))
                return;
            Debug.Log("Layer mask check passed for " + other.gameObject.name);
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                DealDamage(damageable, other.ClosestPoint(transform.position), (other.transform.position - transform.position).normalized);
            }
        }

        private bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            return ((1 << layer) & layerMask) != 0;
        }
    }
}
