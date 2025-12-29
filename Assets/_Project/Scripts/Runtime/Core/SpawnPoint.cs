using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Marks the spawn location for enemies in the level.
    /// Singleton with editor gizmo visualization.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        public static SpawnPoint Instance { get; private set; }

        [Header("Gizmo Settings")]
        [SerializeField] private float _gizmoRadius = 1f;
        [SerializeField] private Color _gizmoColor = new Color(0.196f, 0.784f, 0.196f, 0.8f);

        /// <summary>
        /// Gets the world position of the spawn point.
        /// </summary>
        public Vector3 Position => transform.position;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"Multiple SpawnPoint instances detected. Destroying duplicate on {gameObject.name}.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, _gizmoRadius);

            // Draw arrow pointing forward (spawn direction)
            Vector3 forward = transform.forward * _gizmoRadius * 1.5f;
            Gizmos.DrawLine(transform.position, transform.position + forward);

            // Draw arrowhead
            Vector3 right = transform.right * _gizmoRadius * 0.3f;
            Vector3 arrowTip = transform.position + forward;
            Gizmos.DrawLine(arrowTip, arrowTip - forward * 0.3f + right);
            Gizmos.DrawLine(arrowTip, arrowTip - forward * 0.3f - right);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _gizmoColor;
            Gizmos.DrawSphere(transform.position, _gizmoRadius * 0.3f);
        }
    }
}
