using UnityEngine;

namespace Combat
{
  /// <summary>
  /// Component that spawns particle effects when a weapon hits a target.
  /// Detects collision/trigger contacts and plays particles at the impact point.
  /// Works with weapons that have the DamageDealer on the parent and the hitbox collider on a child.
  /// </summary>
  public class WeaponParticleEffect : MonoBehaviour
  {
    [Header("Particle Settings")]
    [SerializeField] private ParticleSystem impactParticlePrefab;
    [SerializeField] private bool enableParticles = true;
    [SerializeField] private float particleLifetime = 2f;

    [Header("Effect Scale")]
    [SerializeField] private bool scaleDamageEffect = true;
    [SerializeField] private float minDamageScale = 0.5f;
    [SerializeField] private float maxDamageScale = 2f;
    [SerializeField] private float maxDamageForScale = 50f;

    [Header("Hitbox Configuration")]
    [SerializeField] private Transform hitboxTransform;
    [Tooltip("Si está vacío, buscará automáticamente el primer collider en los hijos")]

    [Header("Debug")]
    [SerializeField] private bool logParticleSpawn = true;

    private DamageDealer damageDealer;
    private Collider hitboxCollider;

    private void Awake()
    {
      damageDealer = GetComponent<DamageDealer>();
      if (damageDealer == null)
      {
        Debug.LogError($"[WeaponParticleEffect] No se encontró DamageDealer en {gameObject.name}!");
        return;
      }

      // Si no se asignó el hitbox, buscar automáticamente en los hijos
      if (hitboxTransform == null)
      {
        hitboxCollider = GetComponentInChildren<Collider>();
        if (hitboxCollider != null)
        {
          hitboxTransform = hitboxCollider.transform;
          Debug.Log($"[WeaponParticleEffect] Hitbox encontrado automáticamente en {hitboxCollider.gameObject.name}");
        }
      }
      else
      {
        hitboxCollider = hitboxTransform.GetComponent<Collider>();
      }

      if (hitboxCollider == null)
      {
        Debug.LogWarning($"[WeaponParticleEffect] No se encontró Collider en el hitbox de {gameObject.name}. Las partículas no se generarán.");
        return;
      }

      Debug.Log($"[WeaponParticleEffect] Iniciado en {gameObject.name} con hitbox en {hitboxTransform.gameObject.name}");
    }

    /// <summary>
    /// Spawn particle effect at the impact point when collision occurs.
    /// </summary>
    public void SpawnImpactParticles(Vector3 hitPoint, Vector3 hitNormal, float damageAmount)
    {
      if (!enableParticles || impactParticlePrefab == null)
        return;

      // Usar el punto de colisión, o la posición del hitbox si no es válido
      Vector3 spawnPosition = hitPoint != Vector3.zero ? hitPoint : hitboxTransform.position;

      // Instanciar las partículas
      ParticleSystem particleInstance = Instantiate(impactParticlePrefab, spawnPosition, Quaternion.identity);

      // Escalar el efecto según el daño
      if (scaleDamageEffect)
      {
        float damageRatio = Mathf.Min(damageAmount / maxDamageForScale, 1f);
        float scale = Mathf.Lerp(minDamageScale, maxDamageScale, damageRatio);
        particleInstance.transform.localScale = Vector3.one * scale;
      }

      // Orientar las partículas en la dirección del golpe
      if (hitNormal != Vector3.zero)
      {
        particleInstance.transform.rotation = Quaternion.LookRotation(hitNormal);
      }

      if (logParticleSpawn)
        Debug.Log($"<color=yellow>✨ Impacto de arma en {spawnPosition}. Daño: {damageAmount:F1}</color>");

      // Destruir después del tiempo de vida
      Destroy(particleInstance.gameObject, particleLifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
      Debug.Log("HolaCollision");
      // Solo procesar si la colisión es en el hitbox
      if (collision.gameObject != hitboxTransform.gameObject)
        return;

      if (collision.contacts.Length > 0)
      {
        ContactPoint contact = collision.contacts[0];
        SpawnImpactParticles(contact.point, contact.normal, damageDealer.BaseDamage);
      }
    }

    private void OnTriggerEnter(Collider other)
    {
      Debug.Log("HolaTrigger");
      // Solo procesar si el trigger es en el hitbox
      if (other.gameObject != hitboxTransform.gameObject)
        return;

      Vector3 closestPoint = other.ClosestPoint(hitboxTransform.position);
      Vector3 hitNormal = (closestPoint - hitboxTransform.position).normalized;
      SpawnImpactParticles(closestPoint, hitNormal, damageDealer.BaseDamage);
    }
  }
}
