using UnityEngine;
using Combat;
using UnityEngine.Serialization;

namespace Player
{
    /// <summary>
    /// Handles player death by teleporting them to the death room when health reaches zero.
    /// Implements IHealthObserver to listen for death events.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class PlayerDeathHandler : MonoBehaviour, IHealthObserver
    {
        [Header("Death UI")]
        [SerializeField] private GameObject deathPanel;
        [Tooltip("Panel de muerte que se mostrará en la cara del jugador")]
        
        [SerializeField] private Transform cameraTransform;
        [Tooltip("Transform de la cámara (Main Camera) para posicionar el panel")]
        
        [SerializeField] private float panelDistance = 2f;
        [Tooltip("Distancia del panel respecto a la cámara")]
        
        [SerializeField] private float panelHeightOffset = 0f;
        [Tooltip("Offset vertical del panel")]
        
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
            
            // Buscar la cámara automáticamente si no está asignada
            if (cameraTransform == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    cameraTransform = mainCamera.transform;
                }
                else
                {
                    Debug.LogError("[PlayerDeathHandler] No se encontró la cámara principal. Asigna cameraTransform manualmente.");
                }
            }
            if (deathPanel != null)
            {
                deathPanel.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[PlayerDeathHandler] No se ha asignado el Death Panel. Asígnalo en el Inspector.");
            }
            if (healthComponent == null)
            {
                Debug.LogError("[PlayerDeathHandler] No se encontró HealthComponent. Este componente es requerido.");
                enabled = false;
                return;
            }
            // Registrarse como observador del HealthComponent
            healthComponent.AddObserver(this);
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
            
            var target = GameObject.FindGameObjectWithTag("DeathPosition");
            if (target == null)
            {
                Debug.LogError(
                    "[PlayerDeathHandler] No se encontró ningún GameObject con la etiqueta 'DeathPosition'. Asegúrate de que exista en la escena.");
                return;
            }
            
            var targetPosition = target.transform.position;
            playerTransform.position = targetPosition;
            
            if (resetRotation)
            {
                playerTransform.rotation = Quaternion.identity;
            }
            
            if (hadCharacterController && characterController != null)
            {
                characterController.enabled = true;
            }
            
            // Activar el panel de muerte y posicionarlo frente al jugador
            if (deathPanel != null)
            {
                PositionDeathPanel();
                deathPanel.SetActive(true);
                if (showDebugLogs)
                {
                    Debug.Log("[PlayerDeathHandler] Panel de muerte activado y posicionado frente al jugador");
                }
            }
            else
            {
                Debug.LogWarning("[PlayerDeathHandler] No se puede mostrar el panel de muerte: no está asignado");
            }
            
            Time.timeScale = 0; // Pausar el juego al morir
        }
        
        /// <summary>
        /// Posiciona el panel de muerte frente al jugador (similar a MenuManager)
        /// </summary>
        private void PositionDeathPanel()
        {
            if (cameraTransform == null || deathPanel == null)
            {
                Debug.LogWarning("[PlayerDeathHandler] No se puede posicionar el panel: cámara o panel no asignado");
                return;
            }
            
            // Calcular la dirección forward proyectada en el plano horizontal
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            
            // Posicionar el panel frente a la cámara
            deathPanel.transform.position = 
                cameraTransform.position + 
                forward * panelDistance + 
                Vector3.up * panelHeightOffset;
            
            // Rotar el panel para que mire hacia el jugador
            deathPanel.transform.rotation = Quaternion.LookRotation(forward);
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
            
            // Desactivar el panel de muerte si está activo
            if (deathPanel != null)
            {
                deathPanel.SetActive(false);
            }
            
            if (showDebugLogs)
            {
                Debug.Log("[PlayerDeathHandler] Estado de muerte reseteado");
            }
        }
    }
}
