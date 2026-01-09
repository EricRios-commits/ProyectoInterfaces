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

    private float timer;
    private bool activatedTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (activatedTimer)
        {
            timer += Time.deltaTime;
        }
        if (timer >= holdTime)
        {
            petitioner.RequestToModel();
            GazeAlert();
            timer = 0;
        }
    }

    /// <summary>
    /// Listener for the OnHoverEnter callback of the XR Simple Interactable
    /// </summary>
    /// <param name="args"></param>
    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (args.interactorObject is XRGazeInteractor)
        {
            activatedTimer = true;
        }
    }

    /// <summary>
    /// Listener of the OnHoverExit callback of the XR Simple Interactable 
    /// </summary>
    /// <param name="args"></param>
    public void OnHoverExit(HoverExitEventArgs args)
    {
        activatedTimer = false;
        timer = 0f;
    }
}
