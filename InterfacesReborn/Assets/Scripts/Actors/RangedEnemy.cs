using UnityEngine;
using Combat;

namespace Actors
{
    /// <summary>
    /// A ranged enemy that keeps distance and shoots projectiles.
    /// Demonstrates how to create a different enemy type by extending EnemyBase.
    /// </summary>
    public class RangedEnemy : EnemyBase
    {
        [Header("Ranged Specific")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform shootPoint;
        [SerializeField] private float projectileSpeed = 20f;
        [SerializeField] private float optimalRange = 8f;
        [SerializeField] private float minRange = 4f;
        [SerializeField] private float maxRange = 15f;

        protected override void Start()
        {
            base.Start();
            if (shootPoint == null)
                shootPoint = attackPoint;
        }

        protected override void UpdateChasingState()
        {
            if (target == null)
            {
                OnTargetLost();
                return;
            }
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget > profile.LoseTargetRange)
            {
                OnTargetLost();
                return;
            }
            if (distanceToTarget <= maxRange && distanceToTarget >= minRange)
            {
                ChangeState(EnemyState.Attacking);
                return;
            }
            if (movement != null)
            {
                if (distanceToTarget < minRange)
                {
                    Vector3 retreatDirection = (transform.position - target.position).normalized;
                    Vector3 retreatPosition = transform.position + retreatDirection * 2f;
                    movement.MoveTowards(retreatPosition, profile.MoveSpeed);
                }
                else if (distanceToTarget > maxRange)
                {
                    movement.MoveTowards(target.position, profile.ChaseSpeed);
                }
                
                movement.FaceTarget(target.position, profile.RotationSpeed);
            }

            lastKnownTargetPosition = target.position;
        }

        protected override void UpdateAttackingState()
        {
            if (target == null)
            {
                OnTargetLost();
                return;
            }
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (movement != null)
            {
                movement.FaceTarget(target.position, profile.RotationSpeed);
            }

            // Check if target is out of attack range
            if (distanceToTarget < minRange || distanceToTarget > maxRange)
            {
                ChangeState(EnemyState.Chasing);
                return;
            }

            // Attempt to attack
            if (CanAttack)
            {
                Attack();
            }
        }

        protected override void OnAttackExecute()
        {
            // Trigger animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            // Shoot projectile
            if (projectilePrefab != null && target != null && shootPoint != null)
            {
                ShootProjectile();
            }
        }

        private void ShootProjectile()
        {
            GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            
            // Set up projectile
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null && target != null)
            {
                Vector3 direction = (target.position - shootPoint.position).normalized;
                rb.linearVelocity = direction * projectileSpeed;
            }

            // Set up damage dealer on projectile
            DamageDealer projectileDamage = projectile.GetComponent<DamageDealer>();
            if (projectileDamage != null)
            {
                projectileDamage.BaseDamage = profile.AttackDamage;
                projectileDamage.DamageType = profile.DamageType;
            }

            // Destroy projectile after some time
            Destroy(projectile, 5f);
        }

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (!showDebugInfo)
                return;

            // Draw min and max range
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, minRange);
            
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, maxRange);

            // Draw optimal range
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, optimalRange);
        }
    }
}

