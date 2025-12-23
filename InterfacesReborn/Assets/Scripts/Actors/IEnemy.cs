using UnityEngine;

namespace Actors
{
    /// <summary>
    /// Interface for enemy entities.
    /// Defines the contract for enemy behavior.
    /// Follows Interface Segregation Principle - minimal, focused interface.
    /// </summary>
    public interface IEnemy
    {
        /// <summary>
        /// The current target of this enemy (usually the player).
        /// </summary>
        Transform Target { get; set; }
        
        /// <summary>
        /// Whether this enemy is currently active and can act.
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// Whether this enemy can currently attack.
        /// </summary>
        bool CanAttack { get; }
        
        /// <summary>
        /// Execute enemy AI logic.
        /// </summary>
        void UpdateBehavior();
        
        /// <summary>
        /// Make the enemy attack its target.
        /// </summary>
        void Attack();
        
        /// <summary>
        /// Called when enemy is alerted to a threat.
        /// </summary>
        void OnAlerted(Vector3 alertPosition);
        
        /// <summary>
        /// Called when enemy loses track of its target.
        /// </summary>
        void OnTargetLost();
    }
}

