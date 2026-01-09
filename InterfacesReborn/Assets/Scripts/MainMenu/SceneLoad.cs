using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoad : MonoBehaviour
{
    public string newScene;
    private ScreenFader screenFader;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name != "Main_menuFadein")
            return;
        screenFader = FindObjectOfType<ScreenFader>();
        if (screenFader == null)
        {
            Debug.LogError("[SceneLoad] No se encontró ScreenFader en la escena!" + SceneManager.GetActiveScene().name);
        }
        // Change();
        StartCoroutine(WaitAndChange());
    }

    private IEnumerator WaitAndChange()
    {
        yield return new WaitForSeconds(5f);
        Change();
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
        // Iniciar fade out
        screenFader.FadeOut(5f);

        // Cargar escena de forma asíncrona
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(newScene);

        // Esperar a que la escena esté completamente cargada
        while (!loadOperation.isDone)
        {
            yield return null;
        }

        // Escena cargada, ahora hacer fade in
        screenFader.FadeIn(3f);
    }
}
