using System.Collections.Generic;
using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Component that deals damage to IDamageable objects based on controller sensor velocity.
    /// Uses the Rigidbody velocity of the XR controller to determine impact force.
    /// Calculates velocity only on horizontal plane (XZ) to ignore gravity effects.
    /// Only deals damage if the velocity exceeds a minimum threshold.
    /// Damage is dealt once per collision until the weapon exits and re-enters the collider.
    /// </summary>
    public class DamageDealerSensor : DamageDealer
    {
        [Header("Velocity Settings")]
        [SerializeField] private float minimumVelocity = 0.5f;
        [Tooltip("Velocidad mínima para causar daño (m/s) - solo plano horizontal")]
        
        [Header("Controller Reference")]
        [SerializeField] private Rigidbody controllerRigidbody;
        [Tooltip("Rigidbody del controlador XR. Si no se asigna, se buscará en los objetos padre.")]
        
        [SerializeField] private bool searchInParent = true;
        [Tooltip("Si está activado, buscará el Rigidbody en los objetos padre")]
        
        [SerializeField] private bool useManualCalculation = true;
        [Tooltip("Si está activado, calcula la velocidad manualmente como respaldo cuando no hay Rigidbody")]
        
        [Header("Debug")]
        [SerializeField] private bool showVelocityDebug = true;
        
        private float _currentVelocity;
        private HashSet<Collider> _hitColliders = new HashSet<Collider>();
        private bool _controllerFound = false;
        private Vector3 _previousPosition;
        private bool _isInitialized = false;

        private void Start()
        {
            _currentVelocity = 0f;
            _previousPosition = transform.position;
            _isInitialized = true;
            FindController();
        }

        private void FindController()
        {
            // Si ya hay un rigidbody asignado manualmente
            if (controllerRigidbody != null)
            {
                _controllerFound = true;
                Debug.Log($"[DamageDealerSensor] Rigidbody asignado manualmente: {controllerRigidbody.gameObject.name}");
                return;
            }

            // Buscar en los padres si está habilitado
            if (searchInParent)
            {
                controllerRigidbody = GetComponentInParent<Rigidbody>();
                if (controllerRigidbody != null)
                {
                    _controllerFound = true;
                    Debug.Log($"[DamageDealerSensor] Rigidbody encontrado en padre: {controllerRigidbody.gameObject.name}");
                    return;
                }
            }

            Debug.LogWarning($"[DamageDealerSensor] No se encontró Rigidbody. Asigna manualmente el Rigidbody del controlador XR en el Inspector.");
        }

        private void FixedUpdate()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[DamageDealerSensor] FixedUpdate - No inicializado");
                return;
            }

            // Si no se encontró el controlador, intentar buscarlo nuevamente
            if (!_controllerFound)
            {
                FindController();
            }

            // Intentar obtener velocidad del Rigidbody del controlador
            bool velocityObtained = false;
            if (controllerRigidbody != null)
            {
                Vector3 deviceVelocity = controllerRigidbody.linearVelocity;
                
                // Solo usar si la velocidad no es exactamente cero (indicativo de que el Rigidbody está activo)
                if (deviceVelocity.sqrMagnitude > 0.0001f)
                {
                    // Calcular velocidad solo en el plano horizontal (XZ) - ignorar gravedad (Y)
                    Vector3 velocityXZ = new Vector3(deviceVelocity.x, 0f, deviceVelocity.z);
                    _currentVelocity = velocityXZ.magnitude;
                    velocityObtained = true;

                    Debug.Log($"[DamageDealerSensor] Velocidad del Rigidbody: {_currentVelocity:F2} m/s");
                }
                else
                {
                    Debug.Log($"[DamageDealerSensor] Rigidbody encontrado pero velocidad es cero: {deviceVelocity}");
                }
            }

            // Si no se obtuvo velocidad del Rigidbody, calcular manualmente
            if (!velocityObtained && useManualCalculation)
            {
                Vector3 currentPosition = transform.position;
                Vector3 currentPositionXZ = new Vector3(currentPosition.x, 0f, currentPosition.z);
                Vector3 previousPositionXZ = new Vector3(_previousPosition.x, 0f, _previousPosition.z);
                
                float distance = (currentPositionXZ - previousPositionXZ).magnitude;
                _currentVelocity = distance / Time.fixedDeltaTime;

                Debug.Log($"[DamageDealerSensor] Cálculo manual - Distancia: {distance:F4}m, DeltaTime: {Time.fixedDeltaTime:F4}s, Velocidad: {_currentVelocity:F2} m/s");
                Debug.Log($"[DamageDealerSensor] Posición actual: {currentPosition}, Anterior: {_previousPosition}");
                
                _previousPosition = currentPosition;
            }
            else if (!velocityObtained)
            {
                _currentVelocity = 0f;
                Debug.Log("[DamageDealerSensor] No se obtuvo velocidad de ninguna fuente");
            }

            // Actualizar posición anterior para el siguiente frame
            if (useManualCalculation)
            {
                _previousPosition = transform.position;
            }
        }

        protected override void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[DamageDealerSensor] OnTriggerEnter detectado con: {other.gameObject.name}");
            
            // Verificar si ya golpeó a este collider
            if (_hitColliders.Contains(other))
            {
                Debug.Log($"[DamageDealerSensor] Ya golpeó a {other.gameObject.name} anteriormente - IGNORADO");
                return;
            }

            // Verificar velocidad mínima
            Debug.Log($"[DamageDealerSensor] Velocidad actual: {_currentVelocity:F2} m/s | Mínima requerida: {minimumVelocity:F2} m/s");
            if (_currentVelocity < minimumVelocity)
            {
                Debug.Log($"[DamageDealerSensor] Velocidad insuficiente - IGNORADO");
                return;
            }

            // Marcar este collider como golpeado
            _hitColliders.Add(other);
            
            Debug.Log($"[DamageDealerSensor] {gameObject.name} GOLPEÓ a {other.gameObject.name} a velocidad {_currentVelocity:F1} m/s - Llamando a base.OnTriggerEnter");

            // Hacer daño sin escalar con velocidad
            base.OnTriggerEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            // Cuando sale del trigger, permitir que pueda golpear de nuevo
            _hitColliders.Remove(other);
        }

        private void OnDisable()
        {
            // Limpiar historial al desactivar
            _hitColliders.Clear();
        }

        /// <summary>
        /// Obtiene la velocidad horizontal actual del sensor del controlador
        /// </summary>
        public float GetCurrentVelocity()
        {
            return _currentVelocity;
        }

        /// <summary>
        /// Asigna manualmente el Rigidbody del controlador a usar
        /// </summary>
        public void SetControllerRigidbody(Rigidbody rigidbody)
        {
            if (controllerRigidbody != rigidbody)
            {
                controllerRigidbody = rigidbody;
                _controllerFound = rigidbody != null;
                if (_controllerFound)
                {
                    Debug.Log($"[DamageDealerSensor] Nuevo Rigidbody asignado: {rigidbody.gameObject.name}");
                }
            }
        }
    }
}
