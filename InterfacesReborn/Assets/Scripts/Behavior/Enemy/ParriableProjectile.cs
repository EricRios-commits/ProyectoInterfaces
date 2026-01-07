using Combat;
using UnityEngine;

namespace Behavior.Enemy
{
    public class ParriableProjectile : MonoBehaviour, IDamageable
    {
        [SerializeField] private Projectile projectile;
        [SerializeField] private LayerMask damageableLayersAfterParry = LayerMask.GetMask("Player");
        
        public void TakeDamage(DamageInfo damageInfo)
        {
            projectile.Reflect(damageableLayersAfterParry);
        }

        public bool IsAlive { get; }
        public float CurrentHealth { get; }
        public float MaxHealth { get; }
    }
}