using System.Diagnostics;
using UnityEngine;
using Whisper;
using Whisper.Utils;

namespace Scenes.Scripts.VoiceController
{
    /// <summary>
    /// Record audio clip from microphone and make a transcription for VR Meta Quest 2.
    /// Press and hold the right controller grip button to record, release to process.
    /// </summary>
    public class MicrophoneController : MonoBehaviour
    {
        public delegate void Action(string actionText);
        public event Action onActionDetected;
        
        public delegate void WeaponCommand(string weaponName);
        public event WeaponCommand onWeaponCommand;
        
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
            
            // Detectar comandos de armas
            DetectWeaponCommand(text);
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
        
        private void DetectWeaponCommand(string text)
        {
            // Convertir a minúsculas para comparación
            string lowerText = text.ToLower().Trim();
            
            // Lista de armas disponibles
            string[] weapons = { "axe", "spear", "sword", "mace", "hand" };
            
            // Separar el texto en palabras
            string[] words = lowerText.Split(new char[] { ' ', ',', '.', '!', '?' }, System.StringSplitOptions.RemoveEmptyEntries);
            
            // Variables para encontrar la mejor coincidencia
            string bestMatch = null;
            float bestSimilarity = 0f;
            float similarityThreshold = 0.6f; // Umbral de similitud (60%)
            
            // Buscar coincidencias exactas primero
            foreach (string weapon in weapons)
            {
                if (lowerText.Contains(weapon))
                {
                    UnityEngine.Debug.Log($"[MicrophoneController] ⚔️ Comando de arma detectado (exacto): {weapon}");
                    onWeaponCommand?.Invoke(weapon);
                    return;
                }
            }
            
            // Si no hay coincidencia exacta, buscar similitudes
            foreach (string word in words)
            {
                foreach (string weapon in weapons)
                {
                    float similarity = CalculateSimilarity(word, weapon);
                    
                    if (similarity > bestSimilarity)
                    {
                        bestSimilarity = similarity;
                        bestMatch = weapon;
                    }
                }
            }
            
            // Si encontramos una similitud suficiente, usar esa arma
            if (bestMatch != null && bestSimilarity >= similarityThreshold)
            {
                UnityEngine.Debug.Log($"[MicrophoneController] ⚔️ Comando de arma detectado (similar {bestSimilarity:P0}): {bestMatch}");
                onWeaponCommand?.Invoke(bestMatch);
            }
        }
        
        /// <summary>
        /// Calcula la similitud entre dos cadenas usando distancia de Levenshtein normalizada.
        /// Retorna un valor entre 0 (sin similitud) y 1 (idénticas).
        /// </summary>
        private float CalculateSimilarity(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return 0f;
            
            int distance = LevenshteinDistance(s1, s2);
            int maxLength = Mathf.Max(s1.Length, s2.Length);
            
            // Normalizar la distancia a un valor de similitud entre 0 y 1
            return 1f - (float)distance / maxLength;
        }
        
        /// <summary>
        /// Calcula la distancia de Levenshtein entre dos cadenas.
        /// Representa el número mínimo de ediciones (inserción, eliminación, sustitución) 
        /// necesarias para transformar una cadena en otra.
        /// </summary>
        private int LevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];
            
            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;
            
            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;
            
            for (int j = 1; j <= s2.Length; j++)
            {
                for (int i = 1; i <= s1.Length; i++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    
                    d[i, j] = Mathf.Min(
                        Mathf.Min(d[i - 1, j] + 1,      // Eliminación
                                  d[i, j - 1] + 1),     // Inserción
                        d[i - 1, j - 1] + cost          // Sustitución
                    );
                }
            }
            
            return d[s1.Length, s2.Length];
        }
    }
}