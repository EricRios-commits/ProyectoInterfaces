using UnityEngine;
using UnityEngine.Serialization;
using Waves;

public class AlbertoTranslation : MonoBehaviour
{
    [SerializeField]
    private AlbertoTrigger notifier;

    [SerializeField] private string idleAnimationTrigger = "TrIdle";
    [SerializeField] private string sitAnimationTrigger = "TrSit";
    [SerializeField]
    private Transform targetPosition;
    [SerializeField]
    private Transform playerTransform;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    [SerializeField]
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator.SetTrigger(sitAnimationTrigger);
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        notifier.TriggerEnabled += TranslateAlbertoToArena;
        notifier.OnTriggerActivated += TranslateAlbertoToThrone;
    }

    private void TranslateAlbertoToArena()
    {
        transform.position = targetPosition.position;
        transform.LookAt(playerTransform);
        animator.SetTrigger(idleAnimationTrigger);
    }

    private void TranslateAlbertoToThrone()
    {
        Debug.Log("Me piden enviarlo de vuelta al trono");
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        animator.SetTrigger(sitAnimationTrigger);
    }
}
