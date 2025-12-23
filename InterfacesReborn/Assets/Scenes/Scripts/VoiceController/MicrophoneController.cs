using System.Diagnostics;
using UnityEngine;
using Whisper.Utils;

namespace Whisper.Samples
{
    /// <summary>
    /// Record audio clip from microphone and make a transcription for VR Meta Quest 2.
    /// Press and hold the right controller grip button to record, release to process.
    /// </summary>
    public class MicrophoneController : MonoBehaviour
    {
        public delegate void Action(string actionText);
        public event Action onActionDetected;
        
        [Header("Whisper Settings")]
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public bool streamSegments = true;
        public bool printLanguage = true;

        [Header("VR Input Settings")]
        [Tooltip("Which controller button to use (GripButton, TriggerButton, PrimaryButton, SecondaryButton)")]
        public OVRInput.Button recordButton = OVRInput.Button.Two; // Botón B del mando derecho
        [Tooltip("Which controller to use (RTouch for right, LTouch for left)")]
        public OVRInput.Controller controller = OVRInput.Controller.RTouch; // Mando derecho
        
        [Header("Audio Settings")]
        [Tooltip("Tiempo mínimo de grabación en segundos")]
        [SerializeField] private float minRecordTime = 1f;
        [Tooltip("Volumen mínimo para considerar que hay audio")]
        [SerializeField] private float volumeThreshold = 0.01f;
        
        private string _buffer;
        private bool _wasPressingButton = false;
        private float _recordStartTime;

        private void Awake()
        {
            // Forzar idioma a inglés
            whisper.language = "en";
            
            whisper.OnNewSegment += OnNewSegment;
            whisper.OnProgress += OnProgressHandler;
            microphoneRecord.OnRecordStop += OnRecordStop;

            // Configurar el micrófono para mejor captura
            if (microphoneRecord != null)
            {
                // Asegurarse de que el micrófono esté configurado correctamente
                UnityEngine.Debug.Log($"[MicrophoneController] Micrófonos disponibles: {Microphone.devices.Length}");
                foreach (var device in Microphone.devices)
                {
                    UnityEngine.Debug.Log($"[MicrophoneController] - {device}");
                }
            }
        }

        private async void Start()
        {
            // Inicializar el modelo de Whisper
            UnityEngine.Debug.Log("[MicrophoneController] Cargando modelo de Whisper...");
            await whisper.InitModel();
            UnityEngine.Debug.Log("[MicrophoneController] Modelo de Whisper cargado y listo.");
            UnityEngine.Debug.Log("[MicrophoneController] Mantén presionado el botón B y habla cerca del micrófono de las Quest 2.");
        }

        private void Update()
        {
            // Detectar cuando se presiona el botón
            bool isPressingButton = OVRInput.Get(recordButton, controller);
            
            // Botón presionado (transición de no presionado a presionado)
            if (isPressingButton && !_wasPressingButton)
            {
                StartRecording();
            }
            // Botón soltado (transición de presionado a no presionado)
            else if (!isPressingButton && _wasPressingButton)
            {
                StopRecording();
            }
            
            _wasPressingButton = isPressingButton;
        }

        private void StartRecording()
        {
            if (!microphoneRecord.IsRecording)
            {
                _recordStartTime = Time.time;
                microphoneRecord.StartRecord();
                UnityEngine.Debug.Log("[MicrophoneController] 🎤 Grabación iniciada. Habla AHORA...");
            }
        }

        private void StopRecording()
        {
            if (microphoneRecord.IsRecording)
            {
                float recordDuration = Time.time - _recordStartTime;
                
                if (recordDuration < minRecordTime)
                {
                    UnityEngine.Debug.LogWarning($"[MicrophoneController] ⚠️ Grabación muy corta ({recordDuration:F2}s). Mínimo requerido: {minRecordTime}s");
                }
                
                microphoneRecord.StopRecord();
                UnityEngine.Debug.Log($"[MicrophoneController] ⏹️ Grabación detenida ({recordDuration:F2}s). Procesando...");
            }
        }
        
        private async void OnRecordStop(AudioChunk recordedAudio)
        {
            _buffer = "";

            // Verificar que hay datos de audio
            if (recordedAudio.Data == null || recordedAudio.Data.Length == 0)
            {
                UnityEngine.Debug.LogError("[MicrophoneController] ❌ No hay datos de audio para procesar.");
                return;
            }

            // Calcular el volumen promedio del audio (solo informativo)
            float avgVolume = CalculateAverageVolume(recordedAudio.Data);
            UnityEngine.Debug.Log($"[MicrophoneController] Volumen promedio del audio: {avgVolume:F4}");

            var sw = new Stopwatch();
            sw.Start();
            
            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            if (res == null) 
            {
                UnityEngine.Debug.LogWarning("[MicrophoneController] ❌ No se pudo procesar el audio.");
                return;
            }

            var time = sw.ElapsedMilliseconds;
            var rate = recordedAudio.Length / (time * 0.001f);
            
            var text = res.Result.Trim();

            // Filtrar resultados no deseados
            string[] invalidResults = { "[BLANK_AUDIO]", "(BLANK_AUDIO)", "BLANK_AUDIO", 
                                       "[BELL_RINGING]", "BELL_RINGING", "(bell ringing)",
                                       "click", "Click", "CLICK" };
            
            bool isInvalid = false;
            foreach (var invalid in invalidResults)
            {
                if (text.Contains(invalid) || text.Equals(invalid, System.StringComparison.OrdinalIgnoreCase))
                {
                    isInvalid = true;
                    break;
                }
            }

            if (isInvalid || string.IsNullOrWhiteSpace(text))
            {
                UnityEngine.Debug.LogWarning($"[MicrophoneController] ⚠️ Resultado inválido o vacío: '{text}'. Intenta hablar más alto y más cerca del micrófono.");
                return;
            }
            
            // Imprimir resultado en consola
            UnityEngine.Debug.Log("========== TRANSCRIPCIÓN ==========");
            UnityEngine.Debug.Log($"✅ Texto: {text}");
            if (printLanguage)
                UnityEngine.Debug.Log($"🌐 Idioma detectado: {res.Language}");
            UnityEngine.Debug.Log($"⏱️ Tiempo de procesamiento: {time} ms");
            UnityEngine.Debug.Log($"⚡ Velocidad: {rate:F1}x");
            UnityEngine.Debug.Log($"🔊 Volumen: {avgVolume:F4}");
            UnityEngine.Debug.Log("===================================");
            
            // Invocar evento para otros scripts
            onActionDetected?.Invoke(text);
        }

        private float CalculateAverageVolume(float[] audioData)
        {
            float sum = 0f;
            foreach (float sample in audioData)
            {
                sum += Mathf.Abs(sample);
            }
            return sum / audioData.Length;
        }

        private void OnProgressHandler(int progress)
        {
            UnityEngine.Debug.Log($"[MicrophoneController] 🔄 Progreso de procesamiento: {progress}%");
        }
        
        private void OnNewSegment(WhisperSegment segment)
        {
            if (!streamSegments)
                return;

            _buffer += segment.Text;
            UnityEngine.Debug.Log($"[MicrophoneController] 📝 Segmento parcial: {_buffer}...");
        }
    }
}