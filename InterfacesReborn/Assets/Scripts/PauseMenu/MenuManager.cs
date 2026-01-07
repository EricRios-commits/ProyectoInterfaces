using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private GameObject menu;

    private bool lastState = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        menu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        var leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        if (leftHand.TryGetFeatureValue(CommonUsages.menuButton, out bool pressed))
        {
            if (pressed && !lastState)
            {
                TogglePauseMenu();
            }

            lastState = pressed;
        }
    }

    public void ContinuePressed()
    {
        menu.SetActive(false);
    }

    public void ExitPressed()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }

    public void ResumeTime()
    {
        Time.timeScale = 1;
        lastState = false;
    }

    public Transform head;      
    public float distance = 1.5f;
    public float heightOffset = -0.1f;

    private void TogglePauseMenu()
    {
        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;

        transform.position =
            head.position +
            forward * distance +
            Vector3.up * heightOffset;

        transform.rotation = Quaternion.LookRotation(forward);

        Time.timeScale = 0;
        Debug.Log("Botón de menú izquierdo pulsado");
        menu.SetActive(true);
    }
}
