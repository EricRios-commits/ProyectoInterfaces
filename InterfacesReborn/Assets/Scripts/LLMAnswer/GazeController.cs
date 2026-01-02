// using System.Collections;
// using PTexto;
// using UnityEngine;
// using UnityEngine.XR.Interaction.Toolkit;
// using UnityEngine.XR.Interaction.Toolkit.Interactors;

// public class GazeController : MonoBehaviour
// {
//     public float holdTime = 2f;
//     public delegate void message();
//     public event message GazeAlert;
//     public TextPetitioner petitioner;

//     public GameObject debugCube;

//     // XRBaseInteractable interactable;
//     Coroutine routine;

//     // void Awake()
//     // {
//     //     interactable = GetComponent<XRBaseInteractable>();

//     //     interactable.hoverEntered.AddListener(OnHoverEnter);
//     //     interactable.hoverExited.AddListener(OnHoverExit);
//     // }

//     public void OnHoverEnter(HoverEnterEventArgs args)
//     {
//         if (args.interactorObject is XRGazeInteractor)
//         {
//             debugCube.transform.position = new Vector3 (0, 0, 0);
//             Debug.Log("Comenzamos Corutina");
//             routine = StartCoroutine(HoldTimer());
//         }
//     }

//     public void OnHoverExit(HoverExitEventArgs args)
//     {
//         if (args.interactorObject is XRGazeInteractor && routine != null)
//         {
//             Debug.Log("Interrumpimos Corutina");
//             StopCoroutine(routine);
//             routine = null;
//         }
//     }

//     IEnumerator HoldTimer()
//     {
//         float t = 0f;

//         while (t < holdTime)
//         {
//             t += Time.deltaTime;
//             yield return null;
//         }
//         petitioner.RequestToModel();
//         routine = null;
//     }
// }
