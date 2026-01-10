using UnityEngine;

namespace Waves
{
    public class AlbertoTrigger : WaveTrigger
    {
        
        public GazeController gazeNotifier;
        public delegate void  message();
        public event message TriggerEnabled;
        [SerializeField] private float triggerDelay = 15f;
        [SerializeField] private bool autoStartTimerOnEnable = true;
        [Tooltip("Si está activado, inicia el timer automáticamente al habilitar el trigger (sin esperar la mirada de Alberto)")]
        
        private WaveManager waveManager;
        private float startedTime;
        private bool isEnabled;
        private bool timerStarted;

        void Start()
        {
            timerStarted = false;
        }

        public override void Initialize(WaveManager manager)
        {
            waveManager = manager;
        }
        
        public override void Enable()
        {
            isEnabled = true;
            gazeNotifier.GazeAlert += StartTimer;
            TriggerEnabled?.Invoke(); // Usar ?. para evitar null reference si no hay suscriptores
            
            // Si autoStartTimerOnEnable está activado, iniciar el timer inmediatamente
            if (autoStartTimerOnEnable)
            {
                StartTimer();
            }
            else
            {
                Debug.Log("[AlbertoTrigger] Trigger habilitado. Esperando mirada de Alberto...");
            }
        }
        
        public override void Disable()
        {
            isEnabled = false;
            gazeNotifier.GazeAlert -= StartTimer;
        }
        
        public override bool CanTrigger()
        {
            if (!isEnabled)
            {
                return false;
            }
            
            if (!timerStarted)
            {
                return false;
            }
            
            float timeElapsed = Time.time - startedTime;
            bool canTrigger = timeElapsed >= triggerDelay;
            
            return canTrigger;
        }

        public void StartTimer()
        {
            startedTime = Time.time;
            timerStarted = true;
            Debug.Log($"[AlbertoTrigger] Timer iniciado. Siguiente oleada en {triggerDelay}s");
        }
        
        private void Update()
        {
            if (isEnabled && timerStarted)
            {
                float timeRemaining = triggerDelay - (Time.time - startedTime);
                
                if (timeRemaining <= 5f && timeRemaining > 4.9f)
                {
                    Debug.Log($"[AlbertoTrigger] ⏱️ {timeRemaining:F1}s para siguiente oleada");
                }
            }
            
            if (CanTrigger())
            {
                Debug.Log("<color=yellow>⚡ [AlbertoTrigger] Trigger activado - Iniciando siguiente oleada</color>");
                InvokeTriggerActivated();
                timerStarted = false;
                Disable();
            }
        }
    }
}

