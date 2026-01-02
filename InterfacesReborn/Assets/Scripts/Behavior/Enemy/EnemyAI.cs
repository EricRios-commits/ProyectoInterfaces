using Combat;
using UnityEngine;

namespace Behavior.Enemy
{
    [RequireComponent(typeof(HealthComponent))]
    public class EnemyAI : EntityHealthObserver
    {
        [Header("Behavior Tree Event Channels")] 
        [Tooltip("Raised when actor takes damage")] 
        [SerializeField] private OnActorDamaged onDamagedEvent;
        [Tooltip("Raised when health drops below threshold")] 
        [SerializeField] private OnActorHealthThresholdReached onHealthThresholdEvent;

        [Header("Thresholds")]
        [Tooltip("Health percentage threshold to trigger events (0-1)")] 
        [Range(0, 1)]
        [SerializeField] private float healthThresholds = 0.25f;

        private bool hasTriggeredThreshold;

        public float HealthPercent
        {
            get
            {
                if (healthComponent != null)
                {
                    return (healthComponent.CurrentHealth / healthComponent.MaxHealth);
                }
                return 1f;
            }
        }

        public override void OnHealthChanged(float current, float max, float delta)
        {
            float healthPercent = current / max;
            bool isBelowThreshold = healthPercent <= healthThresholds;
            if (isBelowThreshold && !hasTriggeredThreshold)
            {
                hasTriggeredThreshold = true;
                onHealthThresholdEvent?.SendEventMessage(gameObject, healthPercent);
            }
        }

        public override void OnDamageTaken(DamageInfo info, float current, float max)
        {
            var damageData = new ActorDamageEventData(info, current, max);
            onDamagedEvent?.SendEventMessage(gameObject, gameObject);
        }
    }
}