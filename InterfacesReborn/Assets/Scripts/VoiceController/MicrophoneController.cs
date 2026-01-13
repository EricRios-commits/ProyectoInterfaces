using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using Whisper.Utils;

namespace Whisper.Samples
{
    /// <summary>
    /// Record audio clip from microphone and make a transcription for VR Meta Quest 2.
    /// Press and hold the right controller button to record, release to process.
    /// Uses XR Interaction Toolkit Input System.
    /// </summary>
    public class MicrophoneController : MonoBehaviour
    {
        public delegate void Action(string actionText);
        public event Action onActionDetected;
        
        public delegate void WeaponCommand(string weaponName);
        public event WeaponCommand onWeaponCommand;
        
        [Header("Whisper Settings")]
        [Tooltip("Usar servidor remoto en lugar de procesamiento local")]
        public bool useRemoteServer = true;
        [Tooltip("Cliente para el servidor de Whisper (solo si useRemoteServer = true)")]
        public WhisperServerClient whisperServer;
        [Tooltip("Whisper local (solo si useRemoteServer = false)")]
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public bool streamSegments = true;
        public bool printLanguage = true;

        [Header("XR Input Settings")]
        [Tooltip("Input action for recording (assign Right Primary Button or any button action)")]
        public InputActionReference recordButtonAction;
        
        [Header("Audio Settings")]
        [Tooltip("Tiempo mínimo de grabación en segundos")]
        [SerializeField] private float minRecordTime = 0.5f;
        [Tooltip("Volumen mínimo para considerar que hay audio")]
        [SerializeField] private float volumeThreshold = 0.01f;
        [Tooltip("Ganancia de amplificación del audio (1.0 = sin cambios, 2.0 = doble volumen)")]
        [SerializeField] private float audioGain = 4.0f;
        
        private string _buffer;
        private bool _wasPressingButton = false;
        private float _recordStartTime;

        private void Awake()
        {
            // Validar que los componentes requeridos están asignados
            if (useRemoteServer)
            {
                if (whisperServer == null)
                {
                    UnityEngine.Debug.LogError("[MicrophoneController] ❌ WhisperServerClient no asignado. Asígnalo en el Inspector o desactiva 'useRemoteServer'.");
                    return;
                }
            }
            else
            {
                if (whisper == null)
                {
                    UnityEngine.Debug.LogError("[MicrophoneController] ❌ WhisperManager no asignado. Asígnalo en el Inspector o activa 'useRemoteServer'.");
                    return;
                }
                
                // Forzar idioma a inglés para Whisper local
                whisper.language = "en";
                whisper.OnNewSegment += OnNewSegment;
                whisper.OnProgress += OnProgressHandler;
            }
            
            if (microphoneRecord == null)
            {
                UnityEngine.Debug.LogError("[MicrophoneController] ❌ MicrophoneRecord no asignado. Asígnalo en el Inspector.");
                return;
            }
            
            microphoneRecord.OnRecordStop += OnRecordStop;

            // Configurar el micrófono para mejor captura
            UnityEngine.Debug.Log($"[MicrophoneController] Micrófonos disponibles: {Microphone.devices.Length}");
            foreach (var device in Microphone.devices)
            {
                UnityEngine.Debug.Log($"[MicrophoneController] - {device}");
            }
        }
        
        private void TryFindRecordButtonAction()
        {
            UnityEngine.Debug.Log("[MicrophoneController] Buscando controlador XR...");
            
            // Buscar el controlador derecho
            var xrControllers = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.XRBaseController>(FindObjectsSortMode.None);
            
            UnityEngine.Debug.Log($"[MicrophoneController] Controladores XR encontrados: {xrControllers.Length}");
            
            foreach (var controller in xrControllers)
            {
                UnityEngine.Debug.Log($"[MicrophoneController] Revisando controlador: {controller.name}");
                
                if (controller.name.ToLower().Contains("right"))
                {
                    UnityEngine.Debug.Log($"[MicrophoneController] ✓ Controlador derecho encontrado: {controller.name}");
                    
                    // Intentar obtener el ActionBasedController
                    var actionController = controller.GetComponent<UnityEngine.XR.Interaction.Toolkit.ActionBasedController>();
                    if (actionController != null)
                    {
                        UnityEngine.Debug.Log("[MicrophoneController] ActionBasedController encontrado");
                        
                        // Intentar usar activateAction (típicamente el gatillo)
                        if (actionController.activateAction.action != null)
                        {
                            // Crear un InputActionReference desde la acción
                            recordButtonAction = ScriptableObject.CreateInstance<InputActionReference>();
                            var actionField = typeof(InputActionReference).GetField("m_Action", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (actionField != null)
                            {
                                actionField.SetValue(recordButtonAction, actionController.activateAction.action);
                                UnityEngine.Debug.Log($"[MicrophoneController] ✓ Input Action asignado desde activateAction de {controller.name}");
                                return;
                            }
                        }
                        
                        // Alternativa: usar selectAction
                        if (actionController.selectAction.action != null)
                        {
                            recordButtonAction = ScriptableObject.CreateInstance<InputActionReference>();
                            var actionField = typeof(InputActionReference).GetField("m_Action", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (actionField != null)
                            {
                                actionField.SetValue(recordButtonAction, actionController.selectAction.action);
                                UnityEngine.Debug.Log($"[MicrophoneController] ✓ Input Action asignado desde selectAction de {controller.name}");
                                return;
                            }
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"[MicrophoneController] No se encontró ActionBasedController en {controller.name}");
                    }
                    break;
                }
            }
            
            UnityEngine.Debug.LogWarning("[MicrophoneController] No se pudo encontrar el Input Action automáticamente. Asígnalo manualmente en el Inspector.");
        }

        private async void Start()
        {
            // Buscar automáticamente el input action si no está asignado
            if (recordButtonAction == null || recordButtonAction.action == null)
            {
                UnityEngine.Debug.Log("[MicrophoneController] Buscando Input Action automáticamente...");
                TryFindRecordButtonAction();
            }

            // Habilitar el input action
            if (recordButtonAction != null && recordButtonAction.action != null)
            {
                recordButtonAction.action.Enable();
                UnityEngine.Debug.Log("[MicrophoneController] Input Action habilitado");
            }
            else
            {
                UnityEngine.Debug.LogWarning("[MicrophoneController] ⚠️ recordButtonAction no asignado. Asígnalo manualmente en el Inspector para usar comandos de voz.");
                UnityEngine.Debug.LogWarning("[MicrophoneController] Si estás en el editor, asegúrate de que el XR Origin esté en la escena y XR Simulator esté activo.");
            }
            
            // Inicializar el modelo de Whisper
            if (useRemoteServer)
            {
                UnityEngine.Debug.Log("[MicrophoneController] Usando servidor remoto de Whisper. Listo para transcribir.");
            }
            else
            {
                UnityEngine.Debug.Log("[MicrophoneController] Cargando modelo de Whisper local...");
                await whisper.InitModel();
                UnityEngine.Debug.Log("[MicrophoneController] Modelo de Whisper cargado y listo.");
            }
            
            if (recordButtonAction != null && recordButtonAction.action != null)
            {
                UnityEngine.Debug.Log("[MicrophoneController] Mantén presionado el botón asignado y habla cerca del micrófono de las Quest 2.");
            }
            else
            {
                UnityEngine.Debug.Log("[MicrophoneController] Asigna 'Record Button Action' en el Inspector para activar la grabación por voz.");
            }
        }

        private void Update()
        {
            if (recordButtonAction == null || recordButtonAction.action == null) return;
            
            // Detectar cuando se presiona el botón
            bool isPressingButton = recordButtonAction.action.ReadValue<float>() > 0.5f;
            
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

            UnityEngine.Debug.Log("[MicrophoneController] ⏳ Iniciando procesamiento de audio... (esto puede tardar en Quest 2)");

            // Calcular el volumen promedio del audio antes de amplificar
            float avgVolumeBefore = CalculateAverageVolume(recordedAudio.Data);
            
            // Amplificar el audio para mejorar el reconocimiento
            float[] amplifiedAudio = AmplifyAudio(recordedAudio.Data, audioGain);
            
            float avgVolumeAfter = CalculateAverageVolume(amplifiedAudio);
            UnityEngine.Debug.Log($"[MicrophoneController] Volumen antes: {avgVolumeBefore:F4} | Después: {avgVolumeAfter:F4} (Ganancia: {audioGain}x)");

            var sw = new Stopwatch();
            sw.Start();
            
            string text = "";
            string detectedLanguage = "en";
            
            if (useRemoteServer)
            {
                // Usar servidor remoto
                UnityEngine.Debug.Log("[MicrophoneController] 🌐 Enviando audio al servidor de Whisper...");
                var serverResponse = await whisperServer.TranscribeAudioAsync(amplifiedAudio, recordedAudio.Frequency, recordedAudio.Channels);
                
                if (serverResponse == null || string.IsNullOrEmpty(serverResponse.text))
                {
                    UnityEngine.Debug.LogWarning("[MicrophoneController] ❌ No se pudo procesar el audio en el servidor.");
                    UnityEngine.Debug.LogWarning("[MicrophoneController] Verifica que el servidor esté accesible desde Quest 2.");
                    return;
                }
                
                text = serverResponse.text.Trim();
                detectedLanguage = !string.IsNullOrEmpty(serverResponse.language) ? serverResponse.language : "unknown";
                
                var time = sw.ElapsedMilliseconds;
                UnityEngine.Debug.Log($"[MicrophoneController] ✅ Transcripción del servidor completada en {time}ms");
            }
            else
            {
                // Usar Whisper local
                UnityEngine.Debug.Log("[MicrophoneController] 🔄 Enviando audio a Whisper local para transcripción...");
                var res = await whisper.GetTextAsync(amplifiedAudio, recordedAudio.Frequency, recordedAudio.Channels);
                
                if (res == null) 
                {
                    UnityEngine.Debug.LogWarning("[MicrophoneController] ❌ No se pudo procesar el audio localmente.");
                    return;
                }
                
                var time = sw.ElapsedMilliseconds;
                UnityEngine.Debug.Log($"[MicrophoneController] ✅ Transcripción local completada en {time}ms");
                var rate = recordedAudio.Length / (time * 0.001f);
                UnityEngine.Debug.Log($"⚡ Velocidad: {rate:F1}x");
                
                text = res.Result.Trim();
                detectedLanguage = res.Language;
            }
            
            sw.Stop();

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
                UnityEngine.Debug.Log($"🌐 Idioma detectado: {detectedLanguage}");
            UnityEngine.Debug.Log($"⏱️ Tiempo de procesamiento: {sw.ElapsedMilliseconds} ms");
            UnityEngine.Debug.Log($"🔊 Volumen: {avgVolumeAfter:F4}");
            UnityEngine.Debug.Log($"🖥️ Modo: {(useRemoteServer ? "Servidor Remoto" : "Procesamiento Local")}");
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
        
        /// <summary>
        /// Amplifica las muestras de audio aplicando una ganancia.
        /// Normaliza automáticamente si supera el rango [-1, 1].
        /// </summary>
        private float[] AmplifyAudio(float[] audioData, float gain)
        {
            if (gain <= 0f)
            {
                UnityEngine.Debug.LogWarning("[MicrophoneController] Ganancia inválida, usando 1.0");
                gain = 1f;
            }
            
            float[] amplified = new float[audioData.Length];
            float maxSample = 0f;
            
            // Primera pasada: amplificar y encontrar el valor máximo
            for (int i = 0; i < audioData.Length; i++)
            {
                amplified[i] = audioData[i] * gain;
                float absSample = Mathf.Abs(amplified[i]);
                if (absSample > maxSample)
                    maxSample = absSample;
            }
            
            // Si superamos el rango, normalizar para evitar clipping
            if (maxSample > 1f)
            {
                float normalizationFactor = 1f / maxSample;
                UnityEngine.Debug.Log($"[MicrophoneController] Normalizando audio (factor: {normalizationFactor:F3}) para evitar clipping");
                
                for (int i = 0; i < amplified.Length; i++)
                {
                    amplified[i] *= normalizationFactor;
                }
            }
            
            return amplified;
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
            
            UnityEngine.Debug.Log($"[MicrophoneController] 🔍 Analizando comando: '{lowerText}'");
            
            // Patrones de detección para cada arma (incluye variantes comunes)
            var weaponPatterns = new System.Collections.Generic.Dictionary<string, string[]>
            {
                { "sword", new[] { "sword", "sord", "sort", "swort", "sworn", "so what", "swarp" } },
                { "axe", new[] { "axe", "ax", "acts", "ask", "ex" } },
                { "spear", new[] { "spear", "speer", "sphere", "spere", "pier", "peer" } },
                { "mace", new[] { "mace", "maze", "mais", "maize", "miss" } },
                { "hand", new[] { "hand", "hands", "fang", "hang", "and", "end" } }
            };
            
            // Separar el texto en palabras
            string[] words = lowerText.Split(new char[] { ' ', ',', '.', '!', '?' }, System.StringSplitOptions.RemoveEmptyEntries);
            
            // Variables para encontrar la mejor coincidencia
            string bestMatch = null;
            float bestSimilarity = 0f;
            float similarityThreshold = 0.5f; // Umbral reducido a 50% para ser más permisivo
            
            // 1. Buscar coincidencias exactas en patrones
            foreach (var weaponPattern in weaponPatterns)
            {
                string weaponName = weaponPattern.Key;
                string[] patterns = weaponPattern.Value;
                
                foreach (string pattern in patterns)
                {
                    if (lowerText.Contains(pattern))
                    {
                        UnityEngine.Debug.Log($"[MicrophoneController] ⚔️ Comando detectado (patrón exacto '{pattern}'): {weaponName}");
                        onWeaponCommand?.Invoke(weaponName);
                        return;
                    }
                }
            }
            
            // 2. Buscar similitudes palabra por palabra
            foreach (string word in words)
            {
                if (word.Length < 2) continue; // Ignorar palabras muy cortas
                
                foreach (var weaponPattern in weaponPatterns)
                {
                    string weaponName = weaponPattern.Key;
                    string[] patterns = weaponPattern.Value;
                    
                    foreach (string pattern in patterns)
                    {
                        float similarity = CalculateSimilarity(word, pattern);
                        
                        if (similarity > bestSimilarity)
                        {
                            bestSimilarity = similarity;
                            bestMatch = weaponName;
                            UnityEngine.Debug.Log($"[MicrophoneController] 📊 '{word}' ~ '{pattern}' = {similarity:P0} (mejor hasta ahora: {weaponName})");
                        }
                    }
                }
            }
            
            // Si encontramos una similitud suficiente, usar esa arma
            if (bestMatch != null && bestSimilarity >= similarityThreshold)
            {
                UnityEngine.Debug.Log($"[MicrophoneController] ⚔️ Comando detectado (similitud {bestSimilarity:P0}): {bestMatch}");
                onWeaponCommand?.Invoke(bestMatch);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[MicrophoneController] ❌ No se detectó ningún comando de arma válido. Mejor coincidencia: {bestMatch} ({bestSimilarity:P0})");
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