using PTexto;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GazeController : MonoBehaviour
{
    public float holdTime;
    public delegate void message();
    public event message GazeAlert;
    public TextPetitioner petitioner;

    public GameObject debugCube;
    private float timer;
    private bool activatedTimer;
    public TextMeshProUGUI debugText;

    void Start()
    {
        timer = 0f;
    }

    void Update()
    {
        if (activatedTimer)
        {
            timer += Time.deltaTime;
        }
        if (timer >= holdTime)
        {
            petitioner.RequestToModel();
            timer = 0;
        }
    }

    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (args.interactorObject is XRGazeInteractor)
        {
            activatedTimer = true;
        }
    }

    public void OnHoverExit(HoverExitEventArgs args)
    {
        activatedTimer = false;
        timer = 0f;
    }
}
