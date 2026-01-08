using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoad : MonoBehaviour
{
    public string newScene;
    
    public void Change()
    {
        if (newScene == "")
        {
            newScene = "Coliseo";
        }
        Time.timeScale = 1; // Asegurarse de que el tiempo esté normalizado al cambiar de escena
        SceneManager.LoadScene(newScene);
    }
}
