using PTexto;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Waves;

public class GazeController : MonoBehaviour
{
    public float holdTime;
    public delegate void message();
    public event message GazeAlert = delegate { };
    public TextPetitioner petitioner;

    private float timer;
    private bool activatedTimer;
    private bool alreadySpoken;

    [SerializeField]
    private AlbertoTrigger triggerNotifier;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = 0f;
        alreadySpoken = false;
        triggerNotifier.TriggerEnabled += RestartInteraction;
        activatedTimer = false;
    }

    private void RestartInteraction()
    {
        alreadySpoken = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (activatedTimer)
        {
            timer += Time.deltaTime;
        }
        if (timer >= holdTime && !alreadySpoken)
        {
            Debug.Log("llamada al modelo");
            petitioner.RequestToModel();
            GazeAlert();
            timer = 0;
            alreadySpoken = true;
        }
    }

    /// <summary>
    /// Listener for the OnHoverEnter callback of the XR Simple Interactable
    /// </summary>
    /// <param name="args"></param>
    public void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (args.interactorObject is XRGazeInteractor)
        Debug.Log("Llamada a función Enter");
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
        if (args.interactorObject is XRGazeInteractor)
        {
            Debug.Log("Llamada a función Exit");
            activatedTimer = false;
            timer = 0f;   
        }
    }
}
