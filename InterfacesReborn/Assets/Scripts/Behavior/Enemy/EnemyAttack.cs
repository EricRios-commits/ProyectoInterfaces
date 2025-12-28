using UnityEngine;

namespace Behavior.Enemy
{
    public abstract class EnemyAttack : MonoBehaviour
    {
        public virtual float Cooldown => 1f;
        public abstract bool Perform(GameObject agent, GameObject target);
    }
}
