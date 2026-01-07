using UnityEngine;
using Combat;

namespace Player
{
    /// <summary>
    /// Handles player death by teleporting them to the death room when health reaches zero.
    /// Implements IHealthObserver to listen for death events.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class PlayerDeathHandler : MonoBehaviour, IHealthObserver
    {
        [Header("Death Room Settings")]
        [SerializeField] private Vector3 deathRoomPosition = new Vector3(50f, -1f, 0f);
        [Tooltip("Posición de la sala de muerte donde el jugador será teletransportado")]
        
        [Header("Teleport Settings")]
        [SerializeField] private bool resetRotation = true;
        [Tooltip("Si está activado, resetea la rotación del jugador al teletransportar")]
        
        [SerializeField] private float teleportDelay = 0.5f;
        [Tooltip("Tiempo de espera antes de teletransportar (en segundos)")]
        
        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;
        
        private HealthComponent healthComponent;
        private CharacterController characterController;
        private Transform playerTransform;
        private bool hasDied = false;

        private void Awake()
        {
            // Obtener componentes necesarios
            healthComponent = GetComponent<HealthComponent>();
            characterController = GetComponent<CharacterController>();
            playerTransform = transform;
            
            if (healthComponent == null)
            {
                Debug.LogError("[PlayerDeathHandler] No se encontró HealthComponent. Este componente es requerido.");
                enabled = false;
                return;
            }
            
            // Registrarse como observador del HealthComponent
            healthComponent.AddObserver(this);
            
            if (showDebugLogs)
            {
                Debug.Log($"[PlayerDeathHandler] Iniciado. Sala de muerte configurada en: {deathRoomPosition}");
            }
        }

        private void OnDestroy()
        {
            // Desregistrarse cuando se destruye el objeto
            if (healthComponent != null)
            {
                healthComponent.RemoveObserver(this);
            }
        }

        public void OnHealthChanged(float currentHealth, float maxHealth, float delta)
        {
            // No necesitamos hacer nada aquí, solo nos interesa OnDeath
        }

        public void OnDamageTaken(DamageInfo damageInfo, float currentHealth, float maxHealth)
        {
            // No necesitamos hacer nada aquí, solo nos interesa OnDeath
        }

        public void OnDeath(GameObject dead, DamageInfo finalDamage)
        {
            if (hasDied)
                return;
            
            hasDied = true;
            
            if (showDebugLogs)
            {
                string killerName = finalDamage.Instigator != null ? finalDamage.Instigator.name : "Desconocido";
                Debug.Log($"<color=red>💀 [PlayerDeathHandler] JUGADOR MUERTO | Causa: {finalDamage.Type} | Causado por: {killerName}</color>");
                Debug.Log($"[PlayerDeathHandler] Teletransportando a sala de muerte en {teleportDelay}s...");
            }
            
            // Teletransportar después del delay
            if (teleportDelay > 0)
            {
                Invoke(nameof(TeleportToDeathRoom), teleportDelay);
            }
            else
            {
                TeleportToDeathRoom();
            }
        }

        private void TeleportToDeathRoom()
        {
            if (playerTransform == null)
            {
                Debug.LogError("[PlayerDeathHandler] No se puede teletransportar: Transform del jugador es null");
                return;
            }
            
            // Si hay CharacterController, deshabilitarlo temporalmente para permitir el teletransporte
            bool hadCharacterController = false;
            if (characterController != null && characterController.enabled)
            {
                hadCharacterController = true;
                characterController.enabled = false;
            }
            
            // Teletransportar
            playerTransform.position = deathRoomPosition;
            
            // Resetear rotación si está configurado
            if (resetRotation)
            {
                playerTransform.rotation = Quaternion.identity;
            }
            
            // Reactivar CharacterController
            if (hadCharacterController && characterController != null)
            {
                characterController.enabled = true;
            }
            
            if (showDebugLogs)
            {
                Debug.Log($"<color=cyan>🚪 [PlayerDeathHandler] Jugador teletransportado a sala de muerte: {deathRoomPosition}</color>");
            }
        }
        
        /// <summary>
        /// Método de debug para probar el teletransporte manualmente desde el Inspector
        /// </summary>
        [ContextMenu("Debug: Test Teleport to Death Room")]
        public void DebugTeleportToDeathRoom()
        {
            hasDied = false; // Reset para permitir múltiples pruebas
            TeleportToDeathRoom();
        }
        
        /// <summary>
        /// Resetea el estado de muerte para permitir que el jugador vuelva a morir
        /// Útil si el jugador es revivido
        /// </summary>
        public void ResetDeathState()
        {
            hasDied = false;
            
            if (showDebugLogs)
            {
                Debug.Log("[PlayerDeathHandler] Estado de muerte reseteado");
            }
        }
        
        /// <summary>
        /// Cambia la posición de la sala de muerte dinámicamente
        /// </summary>
        public void SetDeathRoomPosition(Vector3 newPosition)
        {
            deathRoomPosition = newPosition;
            
            if (showDebugLogs)
            {
                Debug.Log($"[PlayerDeathHandler] Nueva posición de sala de muerte: {deathRoomPosition}");
            }
        }
    }
}
