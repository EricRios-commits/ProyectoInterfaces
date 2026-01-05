using UnityEngine;

namespace Waves
{
    public class WaveSystemQuickStart : MonoBehaviour
    {
        [Header("Required Components")]
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private TimeBasedWaveTrigger waveTrigger;
        
        [Header("Settings")]
        [SerializeField] private bool startFirstWaveImmediately = true;
        
        private void Start()
        {
            if (waveManager == null)
            {
                Debug.LogError("WaveSystemQuickStart: WaveManager not assigned!");
                return;
            }
            if (waveTrigger == null)
            {
                Debug.LogError("WaveSystemQuickStart: WaveTrigger not assigned!");
                return;
            }
            waveManager.SetWaveTrigger(waveTrigger);
            if (startFirstWaveImmediately)
            {
                StartFirstWave();
            }
        }
        
        private void StartFirstWave()
        {
            Debug.Log("Starting first wave...");
            waveManager.StartNextWave();
        }
    }
}

