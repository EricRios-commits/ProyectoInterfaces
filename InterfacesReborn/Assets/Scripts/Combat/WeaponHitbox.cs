using UnityEngine;
using System.Collections.Generic;
using Combat;

/// <summary>
/// Hitbox de un arma que detecta colisiones y aplica daño a enemigos.
/// Debe estar en un hijo del arma con un Collider configurado como Trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
public class WeaponHitbox : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [SerializeField] private float baseDamage = 25f;
    [SerializeField] private DamageType damageType = DamageType.Slash;
    
    [Header("Configuración de Colisión")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float hitCooldown = 0.5f; // Tiempo entre golpes al mismo enemigo
    
    private Collider hitboxCollider;
    private bool isActive = false;
    private Dictionary<IDamageable, float> lastHitTimes = new Dictionary<IDamageable, float>();
    
    void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;
        
        // Desactivar hitbox por defecto
        hitboxCollider.enabled = false;
        
        Debug.Log($"[WeaponHitbox] Hitbox configurada en {gameObject.name}");
    }
    
    /// <summary>
    /// Activa la hitbox del arma (llamar cuando empieza el ataque).
    /// </summary>
    public void ActivateHitbox()
    {
        isActive = true;
        hitboxCollider.enabled = true;
        Debug.Log($"[WeaponHitbox] Hitbox activada: {gameObject.name}");
    }
    
    /// <summary>
    /// Desactiva la hitbox del arma (llamar cuando termina el ataque).
    /// </summary>
    public void DeactivateHitbox()
    {
        isActive = false;
        hitboxCollider.enabled = false;
        Debug.Log($"[WeaponHitbox] Hitbox desactivada: {gameObject.name}");
    }
    
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[WeaponHitbox] Trigger detectado con {other.name} (Capa: {LayerMask.LayerToName(other.gameObject.layer)}, Activa: {isActive})");
        
        if (!isActive)
        {
            Debug.Log($"[WeaponHitbox] Hitbox no activa, ignorando colisión");
            return;
        }
        
        // Verificar si está en la capa de enemigos
        int objectLayer = 1 << other.gameObject.layer;
        bool isInEnemyLayer = (objectLayer & enemyLayer) != 0;
        
        Debug.Log($"[WeaponHitbox] Layer check - Object: {objectLayer}, Enemy Mask: {enemyLayer.value}, Match: {isInEnemyLayer}");
        
        if (!isInEnemyLayer)
        {
            Debug.Log($"[WeaponHitbox] {other.name} no está en la capa de enemigos");
            return;
        }
        
        // Buscar componente IDamageable
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable == null)
        {
            damageable = other.GetComponentInParent<IDamageable>();
        }
        
        if (damageable == null)
        {
            Debug.LogWarning($"[WeaponHitbox] Objeto {other.name} en capa de enemigos pero sin IDamageable");
            return;
        }
        
        Debug.Log($"[WeaponHitbox] IDamageable encontrado en {other.name}");
        
        // Verificar cooldown para evitar múltiples hits
        if (CanHit(damageable))
        {
            ApplyDamage(damageable, other);
            lastHitTimes[damageable] = Time.time;
        }
    }
    
    private bool CanHit(IDamageable target)
    {
        if (!lastHitTimes.ContainsKey(target))
            return true;
        
        return Time.time - lastHitTimes[target] >= hitCooldown;
    }
    
    private void ApplyDamage(IDamageable target, Collider hitCollider)
    {
        // Calcular punto y dirección de impacto
        Vector3 hitPoint = hitCollider.ClosestPoint(transform.position);
        Vector3 hitDirection = (hitPoint - transform.position).normalized;
        
        // Crear información de daño
        DamageInfo damageInfo = new DamageInfo(
            baseDamage,
            damageType,
            gameObject, // Instigator (el arma)
            hitPoint,
            hitDirection
        );
        
        // Aplicar daño
        target.TakeDamage(damageInfo);
        
        Debug.Log($"[WeaponHitbox] {gameObject.name} golpeó a {hitCollider.name} por {baseDamage} de daño");
    }
    
    /// <summary>
    /// Limpia el historial de golpes (útil al cambiar de arma).
    /// </summary>
    public void ClearHitHistory()
    {
        lastHitTimes.Clear();
    }
    
    void OnDisable()
    {
        DeactivateHitbox();
    }
}