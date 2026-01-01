using UnityEngine;
using Combat;

/// <summary>
/// Enemigo de prueba simple para testear el sistema de combate.
/// Muestra feedback visual cuando recibe daño.
/// </summary>
[RequireComponent(typeof(HealthComponent))]
public class TestDummy : MonoBehaviour, IDamageable, IHealthObserver
{
    [Header("Referencias")]
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private Renderer meshRenderer;
    
    [Header("Feedback Visual")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float flashDuration = 0.2f;
    
    private Color originalColor;
    private Material material;
    
    // IDamageable interface properties
    public bool IsAlive => healthComponent != null && healthComponent.IsAlive;
    public float CurrentHealth => healthComponent != null ? healthComponent.CurrentHealth : 0f;
    public float MaxHealth => healthComponent != null ? healthComponent.MaxHealth : 0f;
    
    void Awake()
    {
        if (healthComponent == null)
        {
            healthComponent = GetComponent<HealthComponent>();
        }
        
        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<Renderer>();
        }
        
        if (meshRenderer != null)
        {
            material = meshRenderer.material;
            originalColor = material.color;
        }
    }
    
    void OnEnable()
    {
        if (healthComponent != null)
        {
            healthComponent.AddObserver(this);
        }
    }
    
    void OnDisable()
    {
        if (healthComponent != null)
        {
            healthComponent.RemoveObserver(this);
        }
    }
    
    // IHealthObserver implementation
    public void OnHealthChanged(float currentHealth, float maxHealth, float delta)
    {
        // Optional: Handle health changes
    }

    public void OnDamageTaken(DamageInfo damageInfo, float currentHealth, float maxHealth)
    {
        throw new System.NotImplementedException();
    }

    public void OnDamageTaken(DamageInfo damageInfo)
    {
        throw new System.NotImplementedException();
    }

    public void OnDeath(DamageInfo finalDamage)
    {
        HandleDeath();
    }
    
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (healthComponent == null) return;
        
        // Aplicar daño al componente de salud
        healthComponent.TakeDamage(damageInfo);
        
        // Feedback visual
        ShowDamageFlash();
        
        // Log para debug
        Debug.Log($"[TestDummy] Recibió {damageInfo.Amount} de daño {damageInfo.Type}. Salud restante: {healthComponent.CurrentHealth}/{healthComponent.MaxHealth}");
    }
    
    private void ShowDamageFlash()
    {
        if (material == null) return;
        
        // Cancelar flash anterior si existe
        CancelInvoke(nameof(ResetColor));
        
        // Cambiar a color de daño
        material.color = damageColor;
        
        // Volver al color original después del flash
        Invoke(nameof(ResetColor), flashDuration);
    }
    
    private void ResetColor()
    {
        if (material != null)
        {
            material.color = originalColor;
        }
    }
    
    private void HandleDeath()
    {
        Debug.Log($"[TestDummy] ¡Muerto! Desactivando...");
        
        // Opcional: animación de muerte, efectos, etc.
        
        // Desactivar después de un delay
        Invoke(nameof(DeactivateDummy), 2f);
    }
    
    private void DeactivateDummy()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Resetea la salud del dummy (útil para pruebas).
    /// </summary>
    [ContextMenu("Reset Health")]
    public void ResetHealth()
    {
        if (healthComponent != null)
        {
            healthComponent.Revive(healthComponent.MaxHealth);
            ResetColor();
            gameObject.SetActive(true);
            Debug.Log("[TestDummy] Salud reseteada");
        }
    }
}