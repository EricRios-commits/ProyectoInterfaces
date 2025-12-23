using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Example entity that uses the health system.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class CombatEntity : MonoBehaviour, IHealthObserver
    {
        [Header("Configuration")]
        [Tooltip("Optional: Use a stats profile to configure this entity")]
        [SerializeField] private EntityStatsProfile statsProfile;

        [Header("Entity Settings")]
        [SerializeField] private string entityName = "Combat Entity";
        [SerializeField] private GameObject deathEffectPrefab;
        [SerializeField] private float destroyDelay = 2f;

        [Header("Manual Resistance Profile (if no stats profile)")]
        [Tooltip("Optional: Use a resistance profile for manual configuration")]
        [SerializeField] private DamageResistanceProfile resistanceProfile;

        private HealthComponent healthComponent;
        private AudioSource audioSource;

        private void Awake()
        {
            healthComponent = GetComponent<HealthComponent>();
            healthComponent.AddObserver(this);
            audioSource = GetComponent<AudioSource>();
            if (statsProfile != null)
            {
                ApplyStatsProfile(statsProfile);
            }
            else if (resistanceProfile != null)
            {
                ApplyResistanceProfile(resistanceProfile);
            }
        }

        /// <summary>
        /// Apply a complete stats profile to this entity.
        /// </summary>
        public void ApplyStatsProfile(EntityStatsProfile profile)
        {
            if (profile == null) return;
            if (entityName == "Combat Entity" && !string.IsNullOrEmpty(profile.EntityTypeName))
            {
                entityName = profile.EntityTypeName;
            }
            if (profile.DeathEffectPrefab != null)
            {
                deathEffectPrefab = profile.DeathEffectPrefab;
            }
            destroyDelay = profile.DestroyDelay;
            profile.ApplyToEntity(this);
        }

        /// <summary>
        /// Apply just a resistance profile to this entity.
        /// </summary>
        public void ApplyResistanceProfile(DamageResistanceProfile profile)
        {
            if (profile == null || healthComponent == null) return;

            profile.ApplyToHealthComponent(healthComponent);
        }

        private void OnDestroy()
        {
            if (healthComponent != null)
            {
                healthComponent.RemoveObserver(this);
            }
        }

        public void OnHealthChanged(float currentHealth, float maxHealth, float delta)
        {
            if (delta < 0)
            {
                Debug.Log($"{entityName} took {-delta} damage. Health: {currentHealth}/{maxHealth}");
            }
            else if (delta > 0)
            {
                Debug.Log($"{entityName} healed {delta}. Health: {currentHealth}/{maxHealth}");
            }
        }

        public void OnDeath(DamageInfo finalDamage)
        {
            Debug.Log($"{entityName} was killed by {finalDamage.Type} damage from {finalDamage.Instigator?.name ?? "unknown"}");
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }
            if (statsProfile != null && statsProfile.DeathSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(statsProfile.DeathSound);
            }
            if (destroyDelay > 0)
            {
                Destroy(gameObject, destroyDelay);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        #region Runtime Configuration Methods

        /// <summary>
        /// Change the entity's resistance profile at runtime.
        /// </summary>
        public void ChangeResistanceProfile(DamageResistanceProfile newProfile)
        {
            if (newProfile == null) return;
            foreach (DamageType type in System.Enum.GetValues(typeof(DamageType)))
            {
                healthComponent.SetDamageResistance(type, 1.0f);
            }
            ApplyResistanceProfile(newProfile);
            resistanceProfile = newProfile;
        }

        /// <summary>
        /// Add a custom damage modifier at runtime.
        /// </summary>
        public void AddDamageModifier(IDamageModifier modifier)
        {
            if (healthComponent != null)
            {
                healthComponent.AddDamageModifier(modifier);
            }
        }

        /// <summary>
        /// Remove a damage modifier at runtime.
        /// </summary>
        public void RemoveDamageModifier(IDamageModifier modifier)
        {
            if (healthComponent != null)
            {
                healthComponent.RemoveDamageModifier(modifier);
            }
        }

        /// <summary>
        /// Get the current health component (for advanced configuration).
        /// </summary>
        public HealthComponent GetHealthComponent()
        {
            return healthComponent;
        }

        #endregion
    }
}

