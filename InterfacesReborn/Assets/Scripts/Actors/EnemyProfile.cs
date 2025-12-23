using UnityEngine;

namespace Actors
{
    /// <summary>
    /// ScriptableObject that defines enemy configuration.
    /// Follows Single Responsibility Principle - only stores enemy data.
    /// Allows for easy creation of different enemy types without code changes.
    /// </summary>
    [CreateAssetMenu(fileName = "New Enemy Profile", menuName = "Actors/Enemy Profile")]
    public class EnemyProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string enemyName = "Enemy";
        [SerializeField] private string description = "";
        
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float chaseSpeed = 5f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float stoppingDistance = 2f;
        
        [Header("Detection")]
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private float loseTargetRange = 15f;
        [SerializeField] private float fieldOfViewAngle = 120f;
        [SerializeField] private LayerMask detectionLayers;
        
        [Header("Combat")]
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private Combat.DamageType damageType = Combat.DamageType.Slash;
        
        [Header("Behavior")]
        [SerializeField] private bool canPatrol = true;
        [SerializeField] private float patrolWaitTime = 2f;
        [SerializeField] private float investigationTime = 5f;
        [SerializeField] private bool retreatWhenLowHealth = false;
        [SerializeField] [Range(0f, 1f)] private float retreatHealthThreshold = 0.2f;
        
        [Header("Visual")]
        [SerializeField] private Color gizmoColor = Color.red;

        // Properties
        public string EnemyName => enemyName;
        public string Description => description;
        public float MoveSpeed => moveSpeed;
        public float ChaseSpeed => chaseSpeed;
        public float RotationSpeed => rotationSpeed;
        public float StoppingDistance => stoppingDistance;
        public float DetectionRange => detectionRange;
        public float LoseTargetRange => loseTargetRange;
        public float FieldOfViewAngle => fieldOfViewAngle;
        public LayerMask DetectionLayers => detectionLayers;
        public float AttackRange => attackRange;
        public float AttackCooldown => attackCooldown;
        public float AttackDamage => attackDamage;
        public Combat.DamageType DamageType => damageType;
        public bool CanPatrol => canPatrol;
        public float PatrolWaitTime => patrolWaitTime;
        public float InvestigationTime => investigationTime;
        public bool RetreatWhenLowHealth => retreatWhenLowHealth;
        public float RetreatHealthThreshold => retreatHealthThreshold;
        public Color GizmoColor => gizmoColor;
    }
}