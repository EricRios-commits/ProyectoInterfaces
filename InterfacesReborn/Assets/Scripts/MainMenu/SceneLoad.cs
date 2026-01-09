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
            Debug.LogError("[SceneLoad] No se encontró ScreenFader en la escena!" + SceneManager.GetActiveScene().name);
            return;
        }
        if (screenFader.fadeCanvasGroup.alpha >= 1f)
        {
            screenFader.FadeIn(3f);
        }

        // StartCoroutine(WaitAndChange());
    }

    private IEnumerator WaitAndChange()
    {
        yield return new WaitForSeconds(3f);
        Change();
    }

    public void Change()
    {
        if (newScene != "Coliseo")
            return;
        Debug.Log("Cambiando a " + newScene);
        Time.timeScale = 1; // Asegurarse de que el tiempo esté normalizado al cambiar de escena
        StartCoroutine(ChangeSceneWithFade());
    }

    private IEnumerator ChangeSceneWithFade()
    {
        screenFader.FadeOut(3f);
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(newScene);
        yield return null;
        while (!loadOperation.isDone)
        {
            yield return null;
        }
    }
}
