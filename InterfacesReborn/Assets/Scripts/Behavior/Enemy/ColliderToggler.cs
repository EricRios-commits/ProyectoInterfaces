using UnityEngine;

namespace Behavior.Enemy
{
    public class ColliderToggler : MonoBehaviour
    {
        [SerializeField] private Collider[] targetColliders;
        
        public void EnableCollider()
        {
            foreach (var coll in targetColliders)
            {
                if (coll != null)
                {
                    coll.enabled = true;
                }
            }
        }
        
        public void DisableCollider()
        {
            foreach (var coll in targetColliders)
            {
                if (coll != null)
                {
                    coll.enabled = false;
                }
            }
        }
    }
}