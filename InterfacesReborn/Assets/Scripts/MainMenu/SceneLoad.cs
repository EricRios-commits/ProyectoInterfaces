using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Oculus.Interaction.Samples;

public class SceneLoad : MonoBehaviour
{
    private ScreenFader screenFader;
    [SerializeField] private string targetScene;

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
        Time.timeScale = 1;
        StartCoroutine(ChangeSceneWithFade());
    }

    private IEnumerator ChangeSceneWithFade()
    {
        // screenFader.FadeOut(5f);
        // yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(targetScene);
        // yield return new WaitForSeconds(3f);
        // screenFader.FadeIn(3f);
        yield return null;
    }
}
