using UnityEngine;
using System;
using Utility;
using Utility.Timers;

namespace Behavior
{
    [RequireComponent(typeof(Collider))]
    public class Sensor : MonoBehaviour
    {
        [SerializeField] private float timerInterval = 0.5f;
        [SerializeField] private LayerMask layerMask;
        
        [SerializeField] private Collider detectionCollider;

        public event Action OnTargetChanged = delegate { };
        
        public Vector3 TargetPosition => target ? target.transform.position : Vector3.zero;
        
        public bool IsTargetInRange => TargetPosition != Vector3.zero;
        
        private GameObject target;
        private Vector3 lastKnownPosition;
        private CountdownTimer timer;
        
        
        private void Awake()
        {
            detectionCollider = GetComponent<Collider>();
            detectionCollider.isTrigger = true;
        }

        private void Start()
        {
            timer = new CountdownTimer(timerInterval);
            timer.OnTimerStop += () =>
            {
                UpdateTargetPosition(target);
                timer.Start();
            };
            timer.Start();
        }
        
        public GameObject GetClosestTarget()
        {
            return IsTargetInRange ? target : null;
        }

        private void UpdateTargetPosition(GameObject target)
        {
            this.target = target;
            if (IsTargetInRange && (lastKnownPosition != TargetPosition || lastKnownPosition != Vector3.zero))
            {
                lastKnownPosition = TargetPosition;
                OnTargetChanged.Invoke();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (layerMask == (layerMask | (1 << other.gameObject.layer)))
            {
                Debug.Log($"{other.gameObject.name} has entered the sensor");
                UpdateTargetPosition(other.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (layerMask == (layerMask | (1 << other.gameObject.layer)))
            {
                Debug.Log($"{other.gameObject.name} has exited the sensor");
                UpdateTargetPosition(null);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = IsTargetInRange ? Color.red : Color.blue;
            switch (detectionCollider)
            {
                case SphereCollider sphere:
                    Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
                    break;
                case BoxCollider box:
                    Gizmos.DrawWireCube(transform.position + box.center, box.size);
                    break;
            }
        }
    }
}
