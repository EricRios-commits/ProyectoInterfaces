using UnityEngine;
using Waves;

public class AlbertoTranslation : MonoBehaviour
{
    [SerializeField]
    private AlbertoTrigger notifier;

    [SerializeField]
    private Transform targetPosition;
    [SerializeField]
    private Transform playerTransform;
    private Transform originalPosition;
    [SerializeField]
    private Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform;
        notifier.OnTriggerActivated += TranslateAlbertoToThrone;
        notifier.TriggerEnabled += TranslateAlbertoToArena;
    }

    private void TranslateAlbertoToArena()
    {
        transform.position = targetPosition.position;
        transform.LookAt(playerTransform);
        animator.SetTrigger("TrIdle");
    }

    private void TranslateAlbertoToThrone()
    {
        transform.position = originalPosition.position;
        transform.rotation = originalPosition.rotation;
        animator.SetTrigger("TrSeat");
    }
}
