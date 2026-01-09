using UnityEngine;

namespace Waves
{
    public class AlbertoTrigger : WaveTrigger
    {
        
        public GazeController gazeNotifier;
        public delegate void  message();
        public event message TriggerEnabled;
        [SerializeField] private float triggerDelay = 15f;
        private WaveManager waveManager;
        private float startedTime;
        private bool isEnabled;
        
        public override void Initialize(WaveManager manager)
        {
            waveManager = manager;
        }
        
        public override void Enable()
        {
            isEnabled = true;
            TriggerEnabled();
            gazeNotifier.GazeAlert += StartTimer;
        }
        
        public override void Disable()
        {
            isEnabled = false;
            gazeNotifier.GazeAlert -= StartTimer;
        }
        
        public override bool CanTrigger()
        {
            return isEnabled && Time.time >= startedTime + triggerDelay;
        }

        public void StartTimer()
        {
            startedTime = Time.time;
        }
        
        private void Update()
        {
            if (CanTrigger())
            {
                Debug.Log("Trigger activated");
                InvokeTriggerActivated();
                Disable();
            }
        }
    }
}

