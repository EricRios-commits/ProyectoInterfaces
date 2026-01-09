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
        [Tooltip("Raised when enemy dies")]
        [SerializeField] private OnActorDeath onDeathEvent;

        [Header("Thresholds")]
        [Tooltip("Health percentage threshold to trigger events (0-1)")] 
        [Range(0, 1)]
        [SerializeField] private float healthThreshold = 0.25f;

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
            Debug.Log("EnemyAI: OnHealthChanged called. Current Health: " + current + ", Max Health: " + max + ", Delta: " + delta);
            float healthPercent = current / max;
            bool isBelowThreshold = healthPercent <= healthThreshold;
            if (isBelowThreshold && !hasTriggeredThreshold)
            {
                hasTriggeredThreshold = true;
                onHealthThresholdEvent?.SendEventMessage(gameObject, healthPercent);
            }
        }

        public override void OnDamageTaken(DamageInfo info, float current, float max)
        {
            Debug.Log("EnemyAI: OnDamageTaken called. Current Health: " + current + ", Max Health: " + max + ", Damage Info: " + info);
            var damageData = new ActorDamageEventData(info, current, max);
            onDamagedEvent?.SendEventMessage(gameObject, gameObject);
        }

        public override void OnDeath(GameObject dead, DamageInfo finalDamage)
        {
            Debug.Log("EnemyAI: OnDeath called. Final Damage: " + finalDamage);
            onDeathEvent?.SendEventMessage(dead, finalDamage);
            base.OnDeath(dead, finalDamage);
        }
    }
}