using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace PTexto
{
    public class TextPetitioner : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI outputText;
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
            string message =
            "Necesito que me ayudes para un videojuego, en donde serás un aldeano NPC llamado Thalendir. " +
            "Funciona de la siguiente manera: Existe un sistema de oleadas de enemigos, en donde tenemos " +
            "que ir matándolos y en cada ola aumenta la dificultad. Estamos en un coliseo y puede haber " +
            "3 tipos de enemigos: Golem de piedra, esqueleto y mago. El gólem de piedra es pesado, lento " +
            "y grande, pero pega MUY fuerte. El esqueleto es más ágil pero tiene menos vida. El mago ataca " +
            "con bolas de fuego a distancia, por lo que puede ser díficil acercarnos a él. Nuestro jugador " +
            "tiene 4 armas: Espada, Mazo, Lanza y Hacha, que podemos ir cambiando. El mazo puede ser " +
            "especialmente eficaz contra el gólem.\n\n" +
            "Desde hace 100 años, vivimos en un pueblo rodeado de monstruos y el jugador se está enfrentando " +
            "a todos ellos. Buscamos liberar a la aldea. Las armas que tengo han sido forjadas en las montañas " +
            "más altas con el metal más puro, y llevas tiempo sin ver a tu familia ni amigos por los monstruos.\n\n" +
            "Quiero que escojas uno de los siguientes 3 apartados aleatoriamente:\n" +
            "- Dime algún consejo de cómo superar a algún enemigo de este juego.\n" +
            "- Dame trivia ficticia de toda la historia que te he contado: Por qué estamos aquí, quién eres, etc.\n" +
            "- Dime otros monstruos que estén azotando al pueblo pero que no te he comentado en las reglas (easter egg o futura adición como enemigo)\n\n" +
            "Responde como si fueses un aldeano triste/aburrido por estar en el Coliseo y te quisieses ir a tu hogar. " +
            "Hazlo atractivo y conciso como comentario entre rondas. Quiero que me respondas como si fueses el NPC. No agregues comillas a tus mensajes. No digas que estás en un videojuego" +
            "Haz que tu respuesta sea de entre 30 y 35 palabras.";

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