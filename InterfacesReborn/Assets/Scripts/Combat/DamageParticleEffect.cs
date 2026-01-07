using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
  /// <summary>
  /// Observer component that creates particle effects at damage impact points.
  /// Spawns visual feedback when damage is taken using the Observer pattern.
  /// </summary>
  [RequireComponent(typeof(HealthComponent))]
  public class DamageParticleEffect : MonoBehaviour, IHealthObserver
  {
    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem damagePrefab;
    [SerializeField] private bool enableParticles = true;
    [SerializeField] private float particleLifetime = 2f;

    [Header("Effect Scale")]
    [SerializeField] private bool scaleDamageEffect = true;
    [SerializeField] private float minDamageScale = 0.5f;
    [SerializeField] private float maxDamageScale = 2f;
    [SerializeField] private float maxDamageForScale = 50f;

    [Header("Debug")]
    [SerializeField] private bool logParticleSpawn = true;

    private HealthComponent healthComponent;
    private List<ParticleSystem> activeParticles = new List<ParticleSystem>();

    private void Awake()
    {
      healthComponent = GetComponent<HealthComponent>();
      if (healthComponent == null)
      {
        Debug.LogError($"[DamageParticleEffect] No se encontró HealthComponent en {gameObject.name}!");
        return;
      }

      // Registrarse como observador
      healthComponent.AddObserver(this);
      Debug.Log($"[DamageParticleEffect] Iniciado en {gameObject.name}");
    }

    private void OnDestroy()
    {
      // Desregistrarse cuando se destruye
      if (healthComponent != null)
      {
        healthComponent.RemoveObserver(this);
      }
    }

    public void OnHealthChanged(float currentHealth, float maxHealth, float delta)
    {

    }

    public void OnDamageTaken(DamageInfo damageInfo, float currentHealth, float maxHealth)
    {
      // Crear efecto de partículas en el punto de impacto
      if (enableParticles && damagePrefab != null)
      {
        SpawnDamageParticles(damageInfo);
      }
    }

    public void OnDamageTaken(DamageInfo damageInfo)
    {
    }

    public void OnDeath(GameObject dead, DamageInfo finalDamage)
    {
      if (enableParticles && damagePrefab != null)
      {
        SpawnDamageParticles(finalDamage);
      }
    }

    private void SpawnDamageParticles(DamageInfo damageInfo)
    {
      if (damagePrefab == null)
        return;

      // Usar el punto de impacto, o la posición del objeto si no está disponible
      Vector3 spawnPosition = damageInfo.HitPoint;
      if (spawnPosition == Vector3.zero)
      {
        spawnPosition = transform.position;
      }

      // Instanciar las partículas
      ParticleSystem particleInstance = Instantiate(damagePrefab, spawnPosition, Quaternion.identity);

      // Escalar el efecto según el daño
      if (scaleDamageEffect)
      {
        float damageRatio = Mathf.Min(damageInfo.Amount / maxDamageForScale, 1f);
        float scale = Mathf.Lerp(minDamageScale, maxDamageScale, damageRatio);
        particleInstance.transform.localScale = Vector3.one * scale;
      }

      // Orientar las partículas en la dirección del golpe si está disponible
      if (damageInfo.HitDirection != Vector3.zero)
      {
        particleInstance.transform.rotation = Quaternion.LookRotation(damageInfo.HitDirection);
      }

      activeParticles.Add(particleInstance);

      if (logParticleSpawn)
        Debug.Log($"<color=yellow>✨ [{gameObject.name}] Partículas de daño creadas en {spawnPosition}. Daño: {damageInfo.Amount:F1} ({damageInfo.Type})</color>");

      // Destruir después del tiempo de vida
      Destroy(particleInstance.gameObject, particleLifetime);
      activeParticles.Remove(particleInstance);
    }
  }
}
