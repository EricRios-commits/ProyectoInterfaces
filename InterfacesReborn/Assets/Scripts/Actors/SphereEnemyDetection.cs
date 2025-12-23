using UnityEngine;

namespace Actors
{
    /// <summary>
    /// Sphere-based detection system for enemies.
    /// Follows Single Responsibility Principle - only handles target detection.
    /// Uses Physics.OverlapSphere and raycasts for detection.
    /// </summary>
    public class SphereEnemyDetection : MonoBehaviour, IEnemyDetection
    {
        [Header("Detection Settings")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float fieldOfViewAngle = 120f;
        [SerializeField] private LayerMask targetLayers;
        [SerializeField] private LayerMask obstacleLayers;
        
        [Header("Detection Point")]
        [SerializeField] private Transform detectionOrigin;

        private Transform cachedTransform;

        public float DetectionRange
        {
            get => detectionRange;
            set => detectionRange = Mathf.Max(0, value);
        }

        private void Awake()
        {
            cachedTransform = transform;
            if (detectionOrigin == null)
                detectionOrigin = cachedTransform;
        }

        public bool CanDetect(Transform target)
        {
            if (target == null)
                return false;

            Vector3 origin = detectionOrigin.position;
            float distance = Vector3.Distance(origin, target.position);
            if (distance > detectionRange)
                return false;
            Vector3 directionToTarget = (target.position - origin).normalized;
            float angle = Vector3.Angle(detectionOrigin.forward, directionToTarget);
            if (angle > fieldOfViewAngle * 0.5f)
                return false;
            return HasLineOfSight(target.position);
        }

        public Transform FindClosestTarget()
        {
            Collider[] hitColliders = Physics.OverlapSphere(detectionOrigin.position, detectionRange, targetLayers);
            
            Transform closestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (Collider collider in hitColliders)
            {
                if (collider.transform == cachedTransform)
                    continue;

                if (CanDetect(collider.transform))
                {
                    float distance = Vector3.Distance(detectionOrigin.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = collider.transform;
                    }
                }
            }

            return closestTarget;
        }

        public bool HasLineOfSight(Vector3 position)
        {
            Vector3 origin = detectionOrigin.position;
            Vector3 direction = position - origin;
            float distance = direction.magnitude;
            if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance, obstacleLayers))
            {
                return false;
            }
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            if (detectionOrigin == null)
                detectionOrigin = transform;

            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(detectionOrigin.position, detectionRange);

            // Draw field of view
            Vector3 forward = detectionOrigin.forward;
            Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle * 0.5f, 0) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle * 0.5f, 0) * forward;

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(detectionOrigin.position, leftBoundary * detectionRange);
            Gizmos.DrawRay(detectionOrigin.position, rightBoundary * detectionRange);
        }
    }
}

