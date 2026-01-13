using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Whisper.Samples
{
    /// <summary>
    /// Script de prueba para encontrar la configuración correcta del servidor Whisper.
    /// Adjuntar a un GameObject y llamar TestAllConfigurations() desde el Inspector o código.
    /// </summary>
    public class WhisperServerTester : MonoBehaviour
    {
        [Header("Configuración Base")]
        public string baseUrl = "http://gpu1.esit.ull.es:4000";
        
        [Header("Audio de Prueba")]
        [Tooltip("Asignar un AudioClip corto para probar (o dejar null para generar uno)")]
        public AudioClip testAudioClip;
        
        [Header("Resultados")]
        [TextArea(10, 20)]
        public string testResults = "Presiona 'Test All Configurations' para empezar...";
        
        [Header("Búsqueda Automática de Modelos")]
        [Tooltip("Consultar /v1/models para obtener modelos disponibles")]
        public bool fetchModelsFromServer = true;
        
        private List<string> endpoints = new List<string>()
        {
            "/v1/audio/transcriptions",
            "/audio/transcriptions",
            "/openai/v1/audio/transcriptions"
        };
        
        private List<string> fallbackModelNames = new List<string>()
        {
            "", // Sin modelo (usar default)
            "whisper-1",
            "whisper",
            "whisper-large-v3",
            "whisper/whisper-1",
            "whisper/whisper-large-v3",
            "openai/whisper-1",
            "groq/whisper-large-v3",
            "groq/whisper-large-v3-turbo",
            "assemblyai/best"
        };
        
        /// <summary>
        /// Llama este método desde el Inspector (botón) o código para probar todas las configuraciones.
        /// </summary>
        [ContextMenu("Test All Configurations")]
        public async void TestAllConfigurations()
        {
            testResults = "=== INICIANDO PRUEBAS ===\n\n";
            Debug.Log("[WhisperServerTester] Iniciando pruebas de configuración...");
            
            // Obtener modelos del servidor
            List<string> modelNames = new List<string>();
            
            if (fetchModelsFromServer)
            {
                Debug.Log("[WhisperServerTester] Consultando modelos disponibles en el servidor...");
                testResults += "📋 CONSULTANDO MODELOS DEL SERVIDOR...\n\n";
                
                var serverModels = await FetchServerModels();
                
                if (serverModels != null && serverModels.Count > 0)
                {
                    testResults += $"✅ Encontrados {serverModels.Count} modelos en el servidor:\n";
                    foreach (var model in serverModels)
                    {
                        testResults += $"   - {model}\n";
                    }
                    testResults += "\n";
                    
                    modelNames = serverModels;
                    Debug.Log($"[WhisperServerTester] ✅ Encontrados {serverModels.Count} modelos en el servidor");
                }
                else
                {
                    testResults += "⚠️ No se pudieron obtener modelos del servidor. Usando modelos por defecto.\n\n";
                    modelNames = fallbackModelNames;
                    Debug.LogWarning("[WhisperServerTester] No se pudieron obtener modelos del servidor. Usando fallback.");
                }
            }
            else
            {
                modelNames = fallbackModelNames;
                testResults += "ℹ️ Usando lista de modelos por defecto (no consultando servidor).\n\n";
            }
            
            // Generar audio de prueba si no hay uno asignado
            float[] audioData = GenerateTestAudio();
            
            Debug.Log($"[WhisperServerTester] Probando {endpoints.Count} endpoints x {modelNames.Count} modelos = {endpoints.Count * modelNames.Count} combinaciones");
            testResults += $"🔍 PROBANDO {endpoints.Count} endpoints x {modelNames.Count} modelos = {endpoints.Count * modelNames.Count} combinaciones\n\n";
            
            int successCount = 0;
            int totalTests = 0;
            
            foreach (string endpoint in endpoints)
            {
                foreach (string model in modelNames)
                {
                    totalTests++;
                    string url = baseUrl + endpoint;
                    string modelDisplay = string.IsNullOrEmpty(model) ? "(default)" : model;
                    
                    Debug.Log($"[WhisperServerTester] Prueba {totalTests}: {endpoint} con modelo '{modelDisplay}'");
                    
                    var result = await TestConfiguration(url, model, audioData);
                    
                    string status = result.success ? "✅ ÉXITO" : "❌ FALLO";
                    string logEntry = $"{status} | Endpoint: {endpoint} | Modelo: {modelDisplay}\n";
                    
                    if (result.success)
                    {
                        successCount++;
                        logEntry += $"   → Transcripción: \"{result.transcription}\"\n";
                        logEntry += $"   → Tiempo: {result.responseTime}ms\n";
                        Debug.Log($"[WhisperServerTester] ✅ CONFIGURACIÓN QUE FUNCIONA:\n   URL: {url}\n   Modelo: {modelDisplay}\n   Resultado: {result.transcription}");
                    }
                    else
                    {
                        logEntry += $"   → Error: {result.errorMessage}\n";
                    }
                    
                    logEntry += "\n";
                    testResults += logEntry;
                    
                    // Pequeña pausa entre pruebas para no saturar el servidor
                    await Task.Delay(500);
                }
            }
            
            string summary = $"\n=== RESUMEN ===\n" +
                           $"Total de pruebas: {totalTests}\n" +
                           $"Exitosas: {successCount}\n" +
                           $"Fallidas: {totalTests - successCount}\n";
            
            testResults += summary;
            Debug.Log($"[WhisperServerTester] {summary}");
            
            if (successCount > 0)
            {
                Debug.Log("[WhisperServerTester] 🎉 ¡Encontramos al menos una configuración que funciona! Revisa los logs o el campo 'Test Results'.");
            }
            else
            {
                Debug.LogWarning("[WhisperServerTester] ⚠️ Ninguna configuración funcionó. Posibles causas:\n" +
                               "1. El servidor no tiene Whisper configurado\n" +
                               "2. Requiere autenticación (API key)\n" +
                               "3. El endpoint es diferente\n" +
                               "4. Los modelos disponibles no son de audio");
            }
        }
        
        /// <summary>
        /// Consulta los modelos disponibles en el servidor.
        /// </summary>
        private async Task<List<string>> FetchServerModels()
        {
            var models = new List<string>();
            
            try
            {
                string url = baseUrl + "/v1/models";
                Debug.Log($"[WhisperServerTester] Consultando: {url}");
                
                UnityWebRequest request = UnityWebRequest.Get(url);
                request.timeout = 10;
                
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    Debug.Log($"[WhisperServerTester] Respuesta de /v1/models: {responseText}");
                    
                    // Parsear la respuesta
                    var response = JsonUtility.FromJson<ModelsResponse>(responseText);
                    
                    if (response != null && response.data != null)
                    {
                        foreach (var model in response.data)
                        {
                            if (!string.IsNullOrEmpty(model.id))
                            {
                                models.Add(model.id);
                                Debug.Log($"[WhisperServerTester] Modelo encontrado: {model.id}");
                            }
                        }
                    }
                    
                    // Si no se pudo parsear con JsonUtility, intentar parseo manual básico
                    if (models.Count == 0 && responseText.Contains("\"id\""))
                    {
                        Debug.Log("[WhisperServerTester] JsonUtility falló, intentando parseo manual...");
                        models = ParseModelsManually(responseText);
                    }
                }
                else
                {
                    Debug.LogWarning($"[WhisperServerTester] Error al consultar modelos: {request.error}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[WhisperServerTester] Excepción al consultar modelos: {ex.Message}");
            }
            
            // Agregar siempre la opción de "sin modelo" (default)
            if (!models.Contains(""))
            {
                models.Insert(0, "");
            }
            
            return models;
        }
        
        /// <summary>
        /// Parseo manual básico de la respuesta JSON de modelos.
        /// </summary>
        private List<string> ParseModelsManually(string json)
        {
            var models = new List<string>();
            
            try
            {
                // Buscar todos los "id": "..." en el JSON
                int index = 0;
                while ((index = json.IndexOf("\"id\"", index)) != -1)
                {
                    int colonIndex = json.IndexOf(":", index);
                    if (colonIndex == -1) break;
                    
                    int openQuoteIndex = json.IndexOf("\"", colonIndex);
                    if (openQuoteIndex == -1) break;
                    
                    int closeQuoteIndex = json.IndexOf("\"", openQuoteIndex + 1);
                    if (closeQuoteIndex == -1) break;
                    
                    string modelId = json.Substring(openQuoteIndex + 1, closeQuoteIndex - openQuoteIndex - 1);
                    
                    if (!string.IsNullOrEmpty(modelId) && !models.Contains(modelId))
                    {
                        models.Add(modelId);
                        Debug.Log($"[WhisperServerTester] Modelo parseado manualmente: {modelId}");
                    }
                    
                    index = closeQuoteIndex + 1;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[WhisperServerTester] Error en parseo manual: {ex.Message}");
            }
            
            return models;
        }
        
        private async Task<TestResult> TestConfiguration(string url, string modelName, float[] audioData)
        {
            var result = new TestResult();
            var startTime = DateTime.Now;
            
            try
            {
                // Convertir audio a WAV
                byte[] wavData = ConvertToWav(audioData, 16000, 1);
                
                // Crear formulario multipart/form-data
                List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
                formData.Add(new MultipartFormFileSection("file", wavData, "test.wav", "audio/wav"));
                
                if (!string.IsNullOrEmpty(modelName))
                {
                    formData.Add(new MultipartFormDataSection("model", modelName));
                }
                
                // Crear y enviar petición
                UnityWebRequest request = UnityWebRequest.Post(url, formData);
                request.timeout = 10;
                
                var operation = request.SendWebRequest();
                
                while (!operation.isDone)
                {
                    await Task.Yield();
                }
                
                result.responseTime = (int)(DateTime.Now - startTime).TotalMilliseconds;
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    
                    // Intentar parsear respuesta
                    try
                    {
                        var response = JsonUtility.FromJson<WhisperServerResponse>(responseText);
                        if (response != null && !string.IsNullOrEmpty(response.text))
                        {
                            result.success = true;
                            result.transcription = response.text;
                            result.fullResponse = responseText;
                        }
                        else
                        {
                            result.errorMessage = "Respuesta vacía o inválida";
                        }
                    }
                    catch (Exception ex)
                    {
                        result.errorMessage = $"Error parseando JSON: {ex.Message}";
                    }
                }
                else
                {
                    result.errorMessage = $"HTTP {request.responseCode}: {request.error}";
                    
                    if (request.downloadHandler != null)
                    {
                        string errorBody = request.downloadHandler.text;
                        if (!string.IsNullOrEmpty(errorBody) && errorBody.Length < 200)
                        {
                            result.errorMessage += $" | {errorBody}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.errorMessage = $"Excepción: {ex.Message}";
            }
            
            return result;
        }
        
        private float[] GenerateTestAudio()
        {
            if (testAudioClip != null)
            {
                float[] samples = new float[testAudioClip.samples * testAudioClip.channels];
                testAudioClip.GetData(samples, 0);
                return samples;
            }
            
            // Generar 1 segundo de silencio con un pequeño tono
            int sampleRate = 16000;
            int duration = 1;
            float[] audio = new float[sampleRate * duration];
            
            // Agregar un tono breve para que no sea completamente silencio
            for (int i = 0; i < sampleRate / 4; i++)
            {
                audio[i] = 0.1f * Mathf.Sin(2 * Mathf.PI * 440 * i / sampleRate);
            }
            
            return audio;
        }
        
        private byte[] ConvertToWav(float[] audioData, int frequency, int channels)
        {
            int subchunk2Size = audioData.Length * 2;
            int chunkSize = 36 + subchunk2Size;
            byte[] wav = new byte[44 + subchunk2Size];
            
            // Header WAV
            wav[0] = 0x52; wav[1] = 0x49; wav[2] = 0x46; wav[3] = 0x46;
            BitConverter.GetBytes(chunkSize).CopyTo(wav, 4);
            wav[8] = 0x57; wav[9] = 0x41; wav[10] = 0x56; wav[11] = 0x45;
            wav[12] = 0x66; wav[13] = 0x6D; wav[14] = 0x74; wav[15] = 0x20;
            BitConverter.GetBytes(16).CopyTo(wav, 16);
            BitConverter.GetBytes((short)1).CopyTo(wav, 20);
            BitConverter.GetBytes((short)channels).CopyTo(wav, 22);
            BitConverter.GetBytes(frequency).CopyTo(wav, 24);
            BitConverter.GetBytes(frequency * channels * 2).CopyTo(wav, 28);
            BitConverter.GetBytes((short)(channels * 2)).CopyTo(wav, 32);
            BitConverter.GetBytes((short)16).CopyTo(wav, 34);
            wav[36] = 0x64; wav[37] = 0x61; wav[38] = 0x74; wav[39] = 0x61;
            BitConverter.GetBytes(subchunk2Size).CopyTo(wav, 40);
            
            for (int i = 0; i < audioData.Length; i++)
            {
                short sample = (short)(Mathf.Clamp(audioData[i], -1f, 1f) * short.MaxValue);
                BitConverter.GetBytes(sample).CopyTo(wav, 44 + i * 2);
            }
            
            return wav;
        }
        
        private class TestResult
        {
            public bool success = false;
            public string transcription = "";
            public string errorMessage = "";
            public string fullResponse = "";
            public int responseTime = 0;
        }
        
        /// <summary>
        /// Estructura para parsear la respuesta de /v1/models
        /// </summary>
        [Serializable]
        private class ModelsResponse
        {
            public ModelData[] data;
        }
        
        [Serializable]
        private class ModelData
        {
            public string id;
            public string @object;
            public long created;
            public string owned_by;
        }
    }
}
