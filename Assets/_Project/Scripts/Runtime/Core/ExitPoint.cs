using System;
using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Marks the exit location where enemies escape.
    /// Singleton with trigger collider for detecting enemy escapes.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class ExitPoint : MonoBehaviour
    {
        public static ExitPoint Instance { get; private set; }

        [Header("Gizmo Settings")]
        [SerializeField] private float _gizmoRadius = 1f;
        [SerializeField] private Color _gizmoColor = new Color(0.784f, 0.196f, 0.196f, 0.8f);

        /// <summary>
        /// Event fired when an enemy enters the exit trigger.
        /// </summary>
        public event Action<GameObject> OnEnemyReachedExit;

        /// <summary>
        /// Gets the world position of the exit point.
        /// </summary>
        public Vector3 Position => transform.position;

        private BoxCollider _triggerCollider;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                UnityEngine.Debug.LogWarning($"Multiple ExitPoint instances detected. Destroying duplicate on {gameObject.name}.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _triggerCollider = GetComponent<BoxCollider>();
            if (_triggerCollider != null)
            {
                _triggerCollider.isTrigger = true;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if the entering object is on the Enemy layer
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                OnEnemyReachedExit?.Invoke(other.gameObject);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _gizmoRadius);

            // Draw X mark for exit
            Vector3 right = transform.right * _gizmoRadius * 0.7f;
            Vector3 forward = transform.forward * _gizmoRadius * 0.7f;
            Gizmos.DrawLine(transform.position - right - forward, transform.position + right + forward);
            Gizmos.DrawLine(transform.position - right + forward, transform.position + right - forward);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawSphere(transform.position, _gizmoRadius * 0.3f);

            // Draw trigger bounds if collider exists
            BoxCollider col = GetComponent<BoxCollider>();
            if (col != null)
            {
                Gizmos.color = new Color(_gizmoColor.r, _gizmoColor.g, _gizmoColor.b, 0.3f);
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(col.center, col.size);
            }
        }
    }
}
