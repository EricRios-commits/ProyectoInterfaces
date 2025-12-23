using UnityEngine;
using Combat;

namespace Actors
{
    /// <summary>
    /// Base enemy class that orchestrates enemy behavior.
    /// Follows Single Responsibility Principle - coordinates components, doesn't implement everything.
    /// Follows Open/Closed Principle - extensible through virtual methods and component injection.
    /// Follows Dependency Inversion Principle - depends on interfaces (IEnemyMovement, IEnemyDetection, etc.)
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(DamageDealer))]
    public class EnemyBase : MonoBehaviour, IEnemy, IHealthObserver
    {
        private static readonly int Die = Animator.StringToHash("Die");
        private static readonly int Attack1 = Animator.StringToHash("Attack");

        [Header("Configuration")]
        [SerializeField] protected EnemyProfile profile;
        
        [Header("References")]
        [SerializeField] protected Transform attackPoint;
        
        [Header("Target")]
        [SerializeField] protected Transform target;
        
        [Header("Debug")]
        [SerializeField] protected bool showDebugInfo = false;

        // Component references
        protected HealthComponent healthComponent;
        protected DamageDealer damageDealer;
        protected IEnemyMovement movement;
        protected IEnemyDetection detection;
        protected Animator animator;

        // State
        protected EnemyState currentState = EnemyState.Idle;
        protected float lastAttackTime;
        protected float stateTimer;
        protected Vector3 lastKnownTargetPosition;
        protected bool isAlerted = false;

        // Properties
        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public bool IsActive => healthComponent != null && healthComponent.IsAlive && enabled;
        
        public bool CanAttack
        {
            get
            {
                if (!IsActive || target == null)
                    return false;
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                float timeSinceLastAttack = Time.time - lastAttackTime;
                return distanceToTarget <= profile.AttackRange && 
                       timeSinceLastAttack >= profile.AttackCooldown;
            }
        }

        public EnemyState CurrentState => currentState;
        public EnemyProfile Profile => profile;

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            InitializeComponents();
        }

        protected virtual void Start()
        {
            if (profile != null)
            {
                ApplyProfile();
            }
        }

        protected virtual void Update()
        {
            if (!IsActive)
                return;
            UpdateBehavior();
            stateTimer += Time.deltaTime;
        }

        #endregion

        #region Initialization

        protected virtual void InitializeComponents()
        {
            healthComponent = GetComponent<HealthComponent>();
            damageDealer = GetComponent<DamageDealer>();
            movement = GetComponent<IEnemyMovement>();
            detection = GetComponent<IEnemyDetection>();
            animator = GetComponentInChildren<Animator>();
            if (healthComponent != null)
            {
                healthComponent.AddObserver(this);
            }
            if (attackPoint == null)
            {
                attackPoint = transform;
            }
        }

        protected virtual void ApplyProfile()
        {
            if (profile == null)
                return;
            if (damageDealer != null)
            {
                damageDealer.BaseDamage = profile.AttackDamage;
                damageDealer.DamageType = profile.DamageType;
            }
            if (detection != null)
            {
                detection.DetectionRange = profile.DetectionRange;
            }
            if (movement is NavMeshEnemyMovement navMeshMovement)
            {
                navMeshMovement.Configure(profile.StoppingDistance, profile.RotationSpeed * 100f);
            }
        }

        #endregion

        #region IEnemy Implementation

        public virtual void UpdateBehavior()
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    UpdateIdleState();
                    break;
                case EnemyState.Patrolling:
                    UpdatePatrollingState();
                    break;
                case EnemyState.Chasing:
                    UpdateChasingState();
                    break;
                case EnemyState.Attacking:
                    UpdateAttackingState();
                    break;
                case EnemyState.Retreating:
                    UpdateRetreatingState();
                    break;
                case EnemyState.Investigating:
                    UpdateInvestigatingState();
                    break;
                case EnemyState.Stunned:
                    UpdateStunnedState();
                    break;
            }
        }

        public virtual void Attack()
        {
            if (!CanAttack)
                return;
            lastAttackTime = Time.time;
            OnAttackExecute();
        }

        public virtual void OnAlerted(Vector3 alertPosition)
        {
            if (!IsActive)
                return;

            isAlerted = true;
            lastKnownTargetPosition = alertPosition;
            
            if (currentState == EnemyState.Idle || currentState == EnemyState.Patrolling)
            {
                ChangeState(EnemyState.Investigating);
            }
        }

        public virtual void OnTargetLost()
        {
            if (currentState == EnemyState.Chasing || currentState == EnemyState.Attacking)
            {
                ChangeState(EnemyState.Investigating);
            }
        }

        #endregion

        #region State Management

        protected virtual void ChangeState(EnemyState newState)
        {
            if (currentState == newState)
                return;
            OnStateExit(currentState);
            currentState = newState;
            stateTimer = 0f;
            OnStateEnter(newState);
        }

        protected virtual void OnStateEnter(EnemyState state)
        {
            switch (state)
            {
                case EnemyState.Idle:
                    movement?.Stop();
                    break;
                case EnemyState.Chasing:
                    isAlerted = true;
                    break;
                case EnemyState.Attacking:
                    movement?.Stop();
                    break;
            }
            UpdateAnimatorState(state);
        }

        protected virtual void OnStateExit(EnemyState state)
        {
            // Override in derived classes if needed
        }

        #endregion

        #region State Updates

        protected virtual void UpdateIdleState()
        {
            if (detection != null)
            {
                Transform detectedTarget = detection.FindClosestTarget();
                if (detectedTarget != null)
                {
                    target = detectedTarget;
                    ChangeState(EnemyState.Chasing);
                    return;
                }
            }
            if (profile.CanPatrol && stateTimer > profile.PatrolWaitTime)
            {
                ChangeState(EnemyState.Patrolling);
            }
        }

        protected virtual void UpdatePatrollingState()
        {
            if (detection != null)
            {
                Transform detectedTarget = detection.FindClosestTarget();
                if (detectedTarget != null)
                {
                    target = detectedTarget;
                    ChangeState(EnemyState.Chasing);
                    return;
                }
            }
            if (movement != null && movement.HasReachedDestination)
            {
                ChangeState(EnemyState.Idle);
            }
        }

        protected virtual void UpdateChasingState()
        {
            if (target == null)
            {
                OnTargetLost();
                return;
            }
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget > profile.LoseTargetRange)
            {
                OnTargetLost();
                return;
            }
            if (ShouldRetreat())
            {
                ChangeState(EnemyState.Retreating);
                return;
            }
            if (distanceToTarget <= profile.AttackRange)
            {
                ChangeState(EnemyState.Attacking);
                return;
            }
            if (movement != null)
            {
                movement.MoveTowards(target.position, profile.ChaseSpeed);
                movement.FaceTarget(target.position, profile.RotationSpeed);
            }
            lastKnownTargetPosition = target.position;
        }

        protected virtual void UpdateAttackingState()
        {
            if (target == null)
            {
                OnTargetLost();
                return;
            }
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (movement != null)
            {
                movement.FaceTarget(target.position, profile.RotationSpeed);
            }
            if (distanceToTarget > profile.AttackRange + 0.5f)
            {
                ChangeState(EnemyState.Chasing);
                return;
            }
            if (ShouldRetreat())
            {
                ChangeState(EnemyState.Retreating);
                return;
            }
            if (CanAttack)
            {
                Attack();
            }
        }

        protected virtual void UpdateRetreatingState()
        {
            if (target != null && movement != null)
            {
                Vector3 retreatDirection = (transform.position - target.position).normalized;
                Vector3 retreatPosition = transform.position + retreatDirection * 5f;
                movement.MoveTowards(retreatPosition, profile.ChaseSpeed);
            }
            if (!ShouldRetreat())
            {
                ChangeState(EnemyState.Chasing);
            }
        }

        protected virtual void UpdateInvestigatingState()
        {
            if (movement != null && lastKnownTargetPosition != Vector3.zero)
            {
                float distance = Vector3.Distance(transform.position, lastKnownTargetPosition);
                
                if (distance > 1f)
                {
                    movement.MoveTowards(lastKnownTargetPosition, profile.MoveSpeed);
                }
            }
            if (detection != null)
            {
                Transform detectedTarget = detection.FindClosestTarget();
                if (detectedTarget != null)
                {
                    target = detectedTarget;
                    ChangeState(EnemyState.Chasing);
                    return;
                }
            }
            if (stateTimer > profile.InvestigationTime)
            {
                isAlerted = false;
                ChangeState(EnemyState.Idle);
            }
        }

        protected virtual void UpdateStunnedState()
        {
            movement?.Stop();
        }

        #endregion

        #region Combat

        protected virtual void OnAttackExecute()
        {
            if (animator != null)
            {
                animator.SetTrigger(Attack1);
            }
            if (target != null && damageDealer != null)
            {
                damageDealer.DealDamage(target.gameObject, profile.AttackDamage, profile.DamageType, 
                    attackPoint.position, (target.position - attackPoint.position).normalized);
            }
        }

        protected virtual bool ShouldRetreat()
        {
            if (!profile.RetreatWhenLowHealth || healthComponent == null)
                return false;
            float healthPercentage = healthComponent.CurrentHealth / healthComponent.MaxHealth;
            return healthPercentage <= profile.RetreatHealthThreshold;
        }

        #endregion

        #region IHealthObserver Implementation

        public virtual void OnHealthChanged(float currentHealth, float maxHealth, float delta)
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnDeath(DamageInfo finalDamage)
        {
            ChangeState(EnemyState.Dead);
            movement?.Stop();
            if (animator != null)
            {
                animator.SetTrigger(Die);
            }
            enabled = false;
        }

        public virtual void OnRevive()
        {
            enabled = true;
            ChangeState(EnemyState.Idle);
        }

        #endregion

        #region Animation

        protected virtual void UpdateAnimatorState(EnemyState state)
        {
            if (animator == null)
                return;
            animator.SetInteger("State", (int)state);
            animator.SetBool("IsMoving", state == EnemyState.Chasing || state == EnemyState.Patrolling);
            animator.SetBool("IsAttacking", state == EnemyState.Attacking);
        }

        #endregion

        #region Debug

        protected virtual void OnDrawGizmos()
        {
            if (!showDebugInfo || profile == null)
                return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, profile.AttackRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, profile.DetectionRange);
            if (target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target.position);
            }
        }

        protected virtual void OnGUI()
        {
            if (!showDebugInfo)
                return;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2);
            if (screenPos.z > 0)
            {
                GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, 200, 40), 
                    $"State: {currentState}\nTarget: {(target != null ? "Yes" : "No")}");
            }
        }

        #endregion
    }
}

