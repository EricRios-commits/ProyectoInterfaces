using UnityEngine;
using UnityEngine.AI;

namespace Actors
{
    /// <summary>
    /// NavMesh-based implementation of enemy movement.
    /// Follows Single Responsibility Principle - only handles movement.
    /// Follows Liskov Substitution Principle - can be replaced with other IEnemyMovement implementations.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshEnemyMovement : MonoBehaviour, IEnemyMovement
    {
        private NavMeshAgent agent;
        private bool isInitialized = false;

        public bool HasReachedDestination => agent.enabled && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
        public Vector3 Velocity => agent.velocity;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            isInitialized = true;
        }

        public void MoveTowards(Vector3 targetPosition, float speed)
        {
            if (!isInitialized || !agent.enabled)
                return;

            agent.speed = speed;
            agent.SetDestination(targetPosition);
            agent.isStopped = false;
        }

        public void Stop()
        {
            if (!isInitialized || !agent.enabled)
                return;

            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        public void FaceTarget(Vector3 target, float rotationSpeed)
        {
            if (!isInitialized)
                return;

            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0; // Keep rotation on horizontal plane
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Configure the NavMeshAgent with specific settings.
        /// </summary>
        public void Configure(float stoppingDistance, float angularSpeed)
        {
            if (!isInitialized)
                return;

            agent.stoppingDistance = stoppingDistance;
            agent.angularSpeed = angularSpeed;
        }

        /// <summary>
        /// Enable or disable the NavMeshAgent.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (isInitialized)
                agent.enabled = enabled;
        }
    }
}

