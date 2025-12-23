namespace Actors
{
    /// <summary>
    /// Defines different states an enemy can be in.
    /// Used for state machine implementation.
    /// </summary>
    public enum EnemyState
    {
        Idle,
        Patrolling,
        Chasing,
        Attacking,
        Retreating,
        Investigating,
        Stunned,
        Dead
    }
}

