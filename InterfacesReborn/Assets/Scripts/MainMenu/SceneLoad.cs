using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoad : MonoBehaviour
{
    public string newScene;
    private ScreenFader screenFader;

    private void Start()
    {
        screenFader = FindObjectOfType<ScreenFader>();
        if (screenFader == null)
        {
            Debug.LogError("[SceneLoad] No se encontró ScreenFader en la escena!");
        }
    }

    public void Change()
    {
        if (newScene == "")
        {
            newScene = "Coliseo";
        }
        Time.timeScale = 1; // Asegurarse de que el tiempo esté normalizado al cambiar de escena
        StartCoroutine(ChangeSceneWithFade());
    }

    private IEnumerator ChangeSceneWithFade()
    {
        // Fade out
        screenFader.FadeOut(5f);
        yield return new WaitForSeconds(5f);

        // Cambiar escena
        SceneManager.LoadScene(newScene);

        // Fade in en la nueva escena
        yield return new WaitForSeconds(3f);
        screenFader.FadeIn(3f);
    }
}
