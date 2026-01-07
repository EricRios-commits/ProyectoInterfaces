using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject menu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        menu.SetActive(false);
    }

    // Update is called once per frame
    bool lastState = false;

    void Update()
    {
        var leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        if (leftHand.TryGetFeatureValue(CommonUsages.menuButton, out bool pressed))
        {
            if (pressed && !lastState)
            {
                TogglePauseMenu(); // Aquí activas tu menú
            }

            lastState = pressed;
        }
    }

    public void ContinuePressed()
    {
        menu.SetActive(false);
        Time.timeScale = 1;
    }

    public void ExitPressed()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }

    private void TogglePauseMenu()
    {
        Time.timeScale = 0;
        Debug.Log("Botón de menú izquierdo pulsado");
        menu.SetActive(true);
    }
}
