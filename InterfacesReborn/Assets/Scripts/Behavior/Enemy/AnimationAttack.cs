using UnityEngine;

namespace Behavior.Enemy
{
    public class AnimationAttack : EnemyAttack
    {
        [SerializeField] private string[] attackTriggerNames = {"TrAttack"};
        [SerializeField] private float attackCooldown = 1f;
        
        public override float Cooldown => attackCooldown;
        private Animator animator;
        
        private void Awake()
        {
            animator = GetComponent<Animator>();
        }
        
        public override bool Perform(GameObject agent, GameObject target)
        {
            if (animator == null)
                return false;
            var selectIndex = Random.Range(0, attackTriggerNames.Length);
            var attackTriggerName = attackTriggerNames[selectIndex];
            animator.SetTrigger(attackTriggerName);
            return true;
        }
    }
}