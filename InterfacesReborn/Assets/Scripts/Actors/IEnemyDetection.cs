using UnityEngine;

namespace Actors
{
    /// <summary>
    /// Interface for enemy detection/perception system.
    /// Follows Interface Segregation Principle - focused on detection only.
    /// </summary>
    public interface IEnemyDetection
    {
        /// <summary>
        /// Check if a target is detectable.
        /// </summary>
        /// <param name="target">The target to check.</param>
        /// <returns>True if target is detected.</returns>
        bool CanDetect(Transform target);
        
        /// <summary>
        /// Find the closest detectable target.
        /// </summary>
        /// <returns>The closest target transform, or null if none found.</returns>
        Transform FindClosestTarget();
        
        /// <summary>
        /// Check if the enemy has line of sight to a position.
        /// </summary>
        bool HasLineOfSight(Vector3 position);
        
        /// <summary>
        /// The current detection range.
        /// </summary>
        float DetectionRange { get; set; }
    }
}

