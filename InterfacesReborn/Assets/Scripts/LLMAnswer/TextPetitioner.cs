using System.Collections;
using LLMAnswer;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace PTexto
{
    public class TextPetitioner : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI outputText;
        [SerializeField] private TextAsset promptFile;       // optional .txt file assigned in Inspector
        [SerializeField] private PromptSo promptSO;          // optional ScriptableObject containing prompt
        private static string apiUrl = "http://gpu1.esit.ull.es:4000/v1/chat/completions";

        [System.Serializable]
        public class ChatMessage
        {
            public string content;
            public string role;
        }

        [System.Serializable]
        public class ChatChoice
        {
            public int index;
            public string finish_reason;
            public ChatMessage message;
        }

        [System.Serializable]
        public class ChatResponse
        {
            public ChatChoice[] choices;
        }

        void Start() {
            RequestToModel();
        }

        public void SendMessageFromString(string message)
        {
            StartCoroutine(SendMessageToChatbot(message));
        }

        public void RequestToModel()
        {
            // Prefer the ScriptableObject prompt if assigned, then a TextAsset, otherwise fall back to the inline prompt
            string message = null;
            if (promptSO != null && !string.IsNullOrWhiteSpace(promptSO.prompt))
            {
                message = promptSO.prompt;
            }
            else if (promptFile != null && !string.IsNullOrEmpty(promptFile.text))
            {
                message = promptFile.text;
            }
            else
            {
                message = "You're a dungeon master in a roman collosseum. Taunt the gladiators";
            }
            SendMessageFromString(message);
        }

        private IEnumerator SendMessageToChatbot(string message)
        {
            Debug.Log("Entering send message function");
            // Escapar caracteres especiales en el mensaje
            string escapedMessage = message.Replace("\\", "\\\\")
                                           .Replace("\"", "\\\"")
                                           .Replace("\n", "\\n")
                                           .Replace("\r", "\\r")
                                           .Replace("\t", "\\t");

            string jsonPayload = "{"
                                 + "\"model\": \"ollama/llama3.1:8b\"," // Debe coincidir con el modelo cargado en Ollama
                                 + "\"messages\": [{\"role\": \"user\", \"content\": \"" + escapedMessage + "\"}]"
                                 + "}";
            Debug.Log(jsonPayload);
            UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer sk-1234");
            yield return request.SendWebRequest();
            Debug.Log("Petition Sent");
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al conectar con el chatbot: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Respuesta de la IA: " + jsonResponse);
                var parsed = JsonUtility.FromJson<ChatResponse>(jsonResponse);
                if (parsed != null && parsed.choices != null && parsed.choices.Length > 0 && parsed.choices[0].message != null)
                {
                    Debug.Log(parsed.choices[0].message.content);
                    outputText.text = parsed.choices[0].message.content;
                    if (outputText != null)
                    {
                        outputText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    outputText.text = jsonResponse;
                    if (outputText != null)
                    {
                        outputText.gameObject.SetActive(true);
                    }
                }
            }
        }

    }
}