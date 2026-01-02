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
        SceneManager.LoadScene(newScene);
    }
}
