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
        screenFader.FadeOut(2.5f);
        yield return new WaitForSeconds(2.5f);

        // Cambiar escena
        SceneManager.LoadScene(newScene);

        // Fade in en la nueva escena
        yield return new WaitForSeconds(1.5f);
        screenFader.FadeIn(1.5f);
    }
}
