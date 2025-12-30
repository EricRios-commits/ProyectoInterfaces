using UnityEngine;

namespace Behavior.Enemy
{
    public class ColliderToggler : MonoBehaviour
    {
        [SerializeField] private Collider targetCollider;
        
        public void EnableCollider()
        {
            if (targetCollider != null)
            {
                targetCollider.enabled = true;
            }
        }
        
        public void DisableCollider()
        {
            if (targetCollider != null)
            {
                targetCollider.enabled = false;
            }
        }
    }
}