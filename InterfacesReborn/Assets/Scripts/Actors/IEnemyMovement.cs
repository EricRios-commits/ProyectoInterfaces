using UnityEngine;

namespace Actors
{
    /// <summary>
    /// Interface for enemy movement behavior.
    /// Follows Interface Segregation Principle - focused on movement only.
    /// Follows Dependency Inversion Principle - depend on abstraction, not concrete implementation.
    /// </summary>
    public interface IEnemyMovement
    {
        /// <summary>
        /// Move towards a target position.
        /// </summary>
        /// <param name="targetPosition">The position to move towards.</param>
        /// <param name="speed">Movement speed.</param>
        void MoveTowards(Vector3 targetPosition, float speed);
        
        /// <summary>
        /// Stop all movement.
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Rotate towards a target.
        /// </summary>
        /// <param name="target">The target to face.</param>
        /// <param name="rotationSpeed">Speed of rotation.</param>
        void FaceTarget(Vector3 target, float rotationSpeed);
        
        /// <summary>
        /// Check if the enemy has reached the destination.
        /// </summary>
        bool HasReachedDestination { get; }
        
        /// <summary>
        /// Current velocity of the movement.
        /// </summary>
        Vector3 Velocity { get; }
    }
}

