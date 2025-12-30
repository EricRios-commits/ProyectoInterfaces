using Combat;
using UnityEngine;

namespace Behavior.Enemy
{
    public class AnimationProjectileAttack : AnimationAttack
    {
        [SerializeField] private ProjectileAttack projectileAttack;

        private GameObject currentAgent;
        private GameObject currentTarget;
        
        public override bool Perform(GameObject agent, GameObject target)
        {
            currentAgent = agent;
            currentTarget = target;
            return base.Perform(agent, target);
        }
        
        public void ShootProjectile()
        {
            if (projectileAttack != null && currentTarget != null && currentAgent != null)
            {
                projectileAttack.Perform(currentAgent, currentTarget);
            }
        }
    }
}