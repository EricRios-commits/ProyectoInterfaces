using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Whisper.Samples
{
    /// <summary>
    /// Cliente para enviar audio al servidor de Whisper remoto usando API compatible con OpenAI.
    /// Evita el procesamiento local lento en Quest 2.
    /// Compatible con LiteLLM y otros servidores que usan el formato de OpenAI.
    /// </summary>
    public class WhisperServerClient : MonoBehaviour
    {
        [Header("Configuración del Servidor")]
        [Tooltip("URL del endpoint. Groq: https://api.groq.com/openai/v1/audio/transcriptions")]
        public string serverUrl = "https://api.groq.com/openai/v1/audio/transcriptions";
        
        [Tooltip("Modelo a usar. Groq: whisper-large-v3")]
        public string modelName = "whisper-large-v3";
        
        [Tooltip("API Key (obtener gratis en: https://console.groq.com/keys)")]
        public string apiKey = "";
        
        [Tooltip("Timeout en segundos")]
        public int timeoutSeconds = 30;
        
        [Header("Alternativa: Servidor Universidad")]
        [Tooltip("Usar servidor ULL en lugar de Groq (requiere que esté configurado)")]
        public bool useUniversityServer = false;
        
        [Header("Configuración de Audio")]
        [SerializeField] private int sampleRate = 16000; // Whisper espera 16kHz típicamente
        
        private void Start()
        {
            if (useUniversityServer)
            {
                serverUrl = "http://gpu1.esit.ull.es:4000/v1/audio/transcriptions";
                modelName = "";
                apiKey = "";
            }
            
            // Validar API Key
            if (string.IsNullOrEmpty(apiKey) && (serverUrl.Contains("groq.com") || serverUrl.Contains("openai.com")))
            {
                UnityEngine.Debug.LogError("[WhisperServerClient] ⚠️ API Key requerida. Groq gratis: https://console.groq.com/keys");
            }
        }
        
        /// <summary>
        /// Envía audio al servidor y obtiene la transcripción usando formato OpenAI API.
        /// </summary>
        /// <param name="audioData">Datos de audio como array de floats</param>
        /// <param name="frequency">Frecuencia de muestreo original</param>
        /// <param name="channels">Número de canales</param>
        /// <returns>Respuesta con la transcripción</returns>
        public async Task<WhisperServerResponse> TranscribeAudioAsync(float[] audioData, int frequency, int channels)
        {
            try
            {
                // Convertir audio a WAV
                byte[] wavData = ConvertToWav(audioData, frequency, channels);
                UnityEngine.Debug.Log($"[WhisperServerClient] Enviando {wavData.Length / 1024}KB de audio a servidor...");
                
                // Crear formulario multipart/form-data
                List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
                formData.Add(new MultipartFormFileSection("file", wavData, "audio.wav", "audio/wav"));
                
                if (!string.IsNullOrEmpty(modelName))
                {
                    formData.Add(new MultipartFormDataSection("model", modelName));
                }
                
                // Crear petición HTTP
                UnityWebRequest request = UnityWebRequest.Post(serverUrl, formData);
                request.timeout = timeoutSeconds;
                
                // Agregar API Key si está configurada
                if (!string.IsNullOrEmpty(apiKey))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
                }
                
                // Enviar la petición de forma asíncrona
                var operation = request.SendWebRequest();
                
                // Esperar a que complete
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                // Verificar resultado
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    UnityEngine.Debug.Log($"[WhisperServerClient] Respuesta recibida: {responseText}");
                    
                    // Parsear la respuesta JSON de OpenAI
                    WhisperServerResponse response = JsonUtility.FromJson<WhisperServerResponse>(responseText);
                    
                    if (response != null && !string.IsNullOrEmpty(response.text))
                    {
                        UnityEngine.Debug.Log($"[WhisperServerClient] Transcripción exitosa: '{response.text}'");
                        return response;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"[WhisperServerClient] Respuesta inválida o vacía");
                        return null;
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError($"[WhisperServerClient] ❌ Error en la petición: {request.error}");
                    UnityEngine.Debug.LogError($"[WhisperServerClient] Código HTTP: {request.responseCode}");
                    
                    if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
                    {
                        string errorText = request.downloadHandler.text;
                        UnityEngine.Debug.LogError($"[WhisperServerClient] Respuesta completa del servidor:\n{errorText}");
                        
                        // Intentar parsear el error para mostrar el mensaje limpio
                        try
                        {
                            var errorResponse = JsonUtility.FromJson<ErrorResponse>(errorText);
                            if (errorResponse != null && errorResponse.error != null)
                            {
                                UnityEngine.Debug.LogError($"[WhisperServerClient] 💡 Mensaje de error: {errorResponse.error.message}");
                                
                                if (errorResponse.error.message.Contains("Unmapped provider"))
                                {
                                    UnityEngine.Debug.LogError($"[WhisperServerClient] 🔧 SOLUCIÓN: El servidor no reconoce el modelo '{modelName}'. Intenta:");
                                    UnityEngine.Debug.LogError($"   1. Dejar el campo 'Model Name' vacío");
                                    UnityEngine.Debug.LogError($"   2. Usar Groq: https://api.groq.com/openai/v1/audio/transcriptions con model 'whisper-large-v3'");
                                    UnityEngine.Debug.LogError($"   3. Verificar configuración del servidor");
                                }
                                else if (errorResponse.error.message.Contains("Incorrect API key") || 
                                         errorResponse.error.message.Contains("invalid_api_key") ||
                                         request.responseCode == 401)
                                {
                                    UnityEngine.Debug.LogError($"[WhisperServerClient] 🔑 API Key inválida o faltante.");
                                    UnityEngine.Debug.LogError($"   → Para Groq: Obtén una gratis en https://console.groq.com/keys");
                                    UnityEngine.Debug.LogError($"   → Verifica que la key esté configurada en el campo 'Api Key'");
                                }
                            }
                        }
                        catch
                        {
                            // Si no se puede parsear, ya mostramos el texto completo arriba
                        }
                    }
                    
                    return null;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[WhisperServerClient] Excepción: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }
        
        /// <summary>
        /// Convierte un array de floats a formato WAV.
        /// </summary>
        private byte[] ConvertToWav(float[] audioData, int frequency, int channels)
        {
            int subchunk2Size = audioData.Length * 2; // 16-bit = 2 bytes por muestra
            int chunkSize = 36 + subchunk2Size;
            
            byte[] wav = new byte[44 + subchunk2Size];
            
            // Header WAV
            // "RIFF"
            wav[0] = 0x52; wav[1] = 0x49; wav[2] = 0x46; wav[3] = 0x46;
            // Chunk size
            BitConverter.GetBytes(chunkSize).CopyTo(wav, 4);
            // "WAVE"
            wav[8] = 0x57; wav[9] = 0x41; wav[10] = 0x56; wav[11] = 0x45;
            // "fmt "
            wav[12] = 0x66; wav[13] = 0x6D; wav[14] = 0x74; wav[15] = 0x20;
            // Subchunk1Size (16 para PCM)
            BitConverter.GetBytes(16).CopyTo(wav, 16);
            // AudioFormat (1 = PCM)
            BitConverter.GetBytes((short)1).CopyTo(wav, 20);
            // NumChannels
            BitConverter.GetBytes((short)channels).CopyTo(wav, 22);
            // SampleRate
            BitConverter.GetBytes(frequency).CopyTo(wav, 24);
            // ByteRate
            BitConverter.GetBytes(frequency * channels * 2).CopyTo(wav, 28);
            // BlockAlign
            BitConverter.GetBytes((short)(channels * 2)).CopyTo(wav, 32);
            // BitsPerSample
            BitConverter.GetBytes((short)16).CopyTo(wav, 34);
            // "data"
            wav[36] = 0x64; wav[37] = 0x61; wav[38] = 0x74; wav[39] = 0x61;
            // Subchunk2Size
            BitConverter.GetBytes(subchunk2Size).CopyTo(wav, 40);
            
            // Datos de audio (convertir float [-1, 1] a int16)
            for (int i = 0; i < audioData.Length; i++)
            {
                short sample = (short)(Mathf.Clamp(audioData[i], -1f, 1f) * short.MaxValue);
                BitConverter.GetBytes(sample).CopyTo(wav, 44 + i * 2);
            }
            
            return wav;
        }
    }
    
    /// <summary>
    /// Estructura de respuesta del servidor de Whisper compatible con OpenAI API.
    /// </summary>
    [Serializable]
    public class WhisperServerResponse
    {
        // Campo principal de OpenAI API
        public string text;
        
        // Campos opcionales que puede devolver el servidor
        public string language;
        public float duration;
        public Segment[] segments;
    }
    
    /// <summary>
    /// Segmento de transcripción con timestamps (opcional).
    /// </summary>
    [Serializable]
    public class Segment
    {
        public int id;
        public float start;
        public float end;
        public string text;
    }
    
    /// <summary>
    /// Estructura para parsear errores del servidor.
    /// </summary>
    [Serializable]
    public class ErrorResponse
    {
        public ErrorDetail error;
    }
    
    [Serializable]
    public class ErrorDetail
    {
        public string message;
        public string type;
        public string param;
        public string code;
    }
}
