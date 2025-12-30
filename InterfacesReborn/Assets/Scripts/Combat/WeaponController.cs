using UnityEngine;

/// <summary>
/// Controlador principal del arma que gestiona el ataque y las hitboxes.
/// Se debe colocar en cada arma (Sword, Axe, Spear, Mace).
/// Soporta múltiples hitboxes para armas con varios filos.
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private WeaponHitbox[] weaponHitboxes; // Cambiado a array
    
    [Header("Configuración de Ataque")]
    [SerializeField] private float attackDuration = 0.3f; // Duración del ataque
    [SerializeField] private float attackCooldown = 0.5f; // Tiempo entre ataques
    
    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    
    void Awake()
    {
        Debug.Log($"[WeaponController] Awake llamado en {gameObject.name}");
        InitializeHitboxes();
    }
    
    void Start()
    {
        Debug.Log($"[WeaponController] Start llamado en {gameObject.name}");
        // Verificar hitboxes en Start también por si Awake no se ejecutó
        if (weaponHitboxes == null || weaponHitboxes.Length == 0)
        {
            InitializeHitboxes();
        }
    }
    
    void OnEnable()
    {
        Debug.Log($"[WeaponController] OnEnable llamado en {gameObject.name}");
        Debug.Log($"[WeaponController] Script enabled: {enabled}, GameObject active: {gameObject.activeInHierarchy}");
        
        // Re-inicializar hitboxes si es necesario
        if (weaponHitboxes == null || weaponHitboxes.Length == 0)
        {
            InitializeHitboxes();
        }
        
        // Limpiar estado al equipar el arma
        foreach (var hitbox in weaponHitboxes)
        {
            hitbox?.ClearHitHistory();
        }
        isAttacking = false;
    }
    
    private void InitializeHitboxes()
    {
        // Si no hay hitboxes asignadas, buscar todas en hijos
        if (weaponHitboxes == null || weaponHitboxes.Length == 0)
        {
            Debug.Log($"[WeaponController] Buscando hitboxes en {gameObject.name}...");
            weaponHitboxes = GetComponentsInChildren<WeaponHitbox>(true); // true = incluir desactivados
            
            if (weaponHitboxes.Length == 0)
            {
                Debug.LogError($"[WeaponController] ⚠️⚠️⚠️ NO se encontraron WeaponHitbox en {gameObject.name}");
            }
            else
            {
                Debug.Log($"[WeaponController] ✓ {weaponHitboxes.Length} hitbox(es) encontrada(s) en {gameObject.name}");
                for (int i = 0; i < weaponHitboxes.Length; i++)
                {
                    Debug.Log($"  - Hitbox {i + 1}: {weaponHitboxes[i].gameObject.name}");
                }
            }
        }
    }
    
    void Update()
    {
        // Solo procesar si el arma está activa
        if (!gameObject.activeInHierarchy) return;
        
        // Detectar ataque con trigger del controlador derecho
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            Debug.Log($"[WeaponController] Trigger detectado en {gameObject.name}");
            TryAttack();
        }
    }
    
    /// <summary>
    /// Intenta ejecutar un ataque si no está en cooldown.
    /// </summary>
    public void TryAttack()
    {
        if (isAttacking) return;
        
        float timeSinceLastAttack = Time.time - lastAttackTime;
        if (timeSinceLastAttack < attackCooldown)
        {
            Debug.Log($"[WeaponController] Ataque en cooldown: {attackCooldown - timeSinceLastAttack:F2}s restantes");
            return;
        }
        
        StartAttack();
    }
    
    private void StartAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Activar todas las hitboxes
        foreach (var hitbox in weaponHitboxes)
        {
            hitbox?.ActivateHitbox();
        }
        
        Debug.Log($"[WeaponController] Ataque iniciado con {gameObject.name} ({weaponHitboxes.Length} hitboxes activas)");
        
        // Desactivar hitboxes después de la duración del ataque
        Invoke(nameof(EndAttack), attackDuration);
    }
    
    private void EndAttack()
    {
        isAttacking = false;
        
        // Desactivar todas las hitboxes
        foreach (var hitbox in weaponHitboxes)
        {
            hitbox?.DeactivateHitbox();
        }
        
        Debug.Log($"[WeaponController] Ataque finalizado");
    }
    
    
    void OnDisable()
    {
        // Asegurar que las hitboxes se desactiven al desequipar
        if (weaponHitboxes != null)
        {
            foreach (var hitbox in weaponHitboxes)
            {
                hitbox?.DeactivateHitbox();
            }
        }
        
        // Cancelar ataques pendientes
        CancelInvoke(nameof(EndAttack));
        isAttacking = false;
    }
    
    /// <summary>
    /// Obtiene si el arma está actualmente atacando.
    /// </summary>
    public bool IsAttacking => isAttacking;
}
