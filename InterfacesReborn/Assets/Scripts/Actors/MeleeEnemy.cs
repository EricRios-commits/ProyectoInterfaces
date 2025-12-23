using UnityEngine;
using Combat;

namespace Actors
{
    /// <summary>
    /// A melee enemy that aggressively pursues and attacks targets.
    /// Demonstrates how to extend EnemyBase for specialized behavior.
    /// Follows Open/Closed Principle - extends base without modifying it.
    /// </summary>
    public class MeleeEnemy : EnemyBase
    {
        [Header("Melee Specific")]
        [SerializeField] private float lungeDistance = 3f;
        [SerializeField] private float lungeCooldown = 5f;
        [SerializeField] private float lungeSpeed = 10f;
        
        private float lastLungeTime;
        private bool isLunging;

        protected override void UpdateAttackingState()
        {
            base.UpdateAttackingState();
            if (CanLunge() && target != null)
            {
                PerformLunge();
            }
        }

        private bool CanLunge()
        {
            if (isLunging)
                return false;
            float timeSinceLunge = Time.time - lastLungeTime;
            if (timeSinceLunge < lungeCooldown)
                return false;
            if (target == null)
                return false;
            float distance = Vector3.Distance(transform.position, target.position);
            return distance > profile.AttackRange && distance <= lungeDistance;
        }

        private void PerformLunge()
        {
            lastLungeTime = Time.time;
            isLunging = true;
            if (movement != null && target != null)
            {
                movement.MoveTowards(target.position, lungeSpeed);
            }
            if (animator != null)
            {
                animator.SetTrigger("Lunge");
            }

            Invoke(nameof(EndLunge), 0.5f);
        }

        private void EndLunge()
        {
            isLunging = false;
        }

        protected override void OnAttackExecute()
        {
            base.OnAttackExecute();

            // Melee enemies might have combo attacks
            // This is just an example - can be expanded
        }
    }
}

