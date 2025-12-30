using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Manages the path waypoints that enemies follow.
    /// Singleton with gizmo visualization for path connections.
    /// </summary>
    public class PathManager : MonoBehaviour
    {
        public static PathManager Instance { get; private set; }

        [Header("Waypoints")]
        [SerializeField] private Transform[] _waypoints;

        [Header("Gizmo Settings")]
        [SerializeField] private Color _pathColor = new Color(1f, 0.706f, 0f, 1f);
        [SerializeField] private Color _waypointColor = new Color(1f, 0.706f, 0f, 0.5f);
        [SerializeField] private float _waypointGizmoRadius = 0.5f;

        /// <summary>
        /// Gets the array of waypoint transforms.
        /// </summary>
        public Transform[] Waypoints => _waypoints;

        /// <summary>
        /// Gets the number of waypoints in the path.
        /// </summary>
        public int WaypointCount => _waypoints != null ? _waypoints.Length : 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                UnityEngine.Debug.LogWarning($"Multiple PathManager instances detected. Destroying duplicate on {gameObject.name}.");
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

        /// <summary>
        /// Gets the waypoint at the specified index.
        /// </summary>
        /// <param name="index">The waypoint index.</param>
        /// <returns>The waypoint transform, or null if index is invalid.</returns>
        public Transform GetWaypoint(int index)
        {
            if (_waypoints == null || index < 0 || index >= _waypoints.Length)
            {
                return null;
            }
            return _waypoints[index];
        }

        /// <summary>
        /// Gets the position of the waypoint at the specified index.
        /// </summary>
        /// <param name="index">The waypoint index.</param>
        /// <returns>The world position of the waypoint.</returns>
        public Vector3 GetWaypointPosition(int index)
        {
            Transform waypoint = GetWaypoint(index);
            return waypoint != null ? waypoint.position : Vector3.zero;
        }

        /// <summary>
        /// Calculates the total path length.
        /// </summary>
        /// <returns>The sum of distances between consecutive waypoints.</returns>
        public float GetTotalPathLength()
        {
            if (_waypoints == null || _waypoints.Length < 2)
            {
                return 0f;
            }

            float totalLength = 0f;
            for (int i = 0; i < _waypoints.Length - 1; i++)
            {
                if (_waypoints[i] != null && _waypoints[i + 1] != null)
                {
                    totalLength += Vector3.Distance(_waypoints[i].position, _waypoints[i + 1].position);
                }
            }
            return totalLength;
        }

        private void OnDrawGizmos()
        {
            if (_waypoints == null || _waypoints.Length == 0)
            {
                return;
            }

            // Draw waypoint spheres
            Gizmos.color = _waypointColor;
            for (int i = 0; i < _waypoints.Length; i++)
            {
                if (_waypoints[i] != null)
                {
                    Gizmos.DrawWireSphere(_waypoints[i].position, _waypointGizmoRadius);
                }
            }

            // Draw path lines connecting waypoints
            Gizmos.color = _pathColor;
            for (int i = 0; i < _waypoints.Length - 1; i++)
            {
                if (_waypoints[i] != null && _waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(_waypoints[i].position, _waypoints[i + 1].position);
                }
            }

            // Draw connection from SpawnPoint to first waypoint
            if (SpawnPoint.Instance != null && _waypoints[0] != null)
            {
                Gizmos.color = new Color(0.196f, 0.784f, 0.196f, 0.5f);
                Gizmos.DrawLine(SpawnPoint.Instance.Position, _waypoints[0].position);
            }

            // Draw connection from last waypoint to ExitPoint
            if (ExitPoint.Instance != null && _waypoints.Length > 0 && _waypoints[_waypoints.Length - 1] != null)
            {
                Gizmos.color = new Color(0.784f, 0.196f, 0.196f, 0.5f);
                Gizmos.DrawLine(_waypoints[_waypoints.Length - 1].position, ExitPoint.Instance.Position);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_waypoints == null || _waypoints.Length == 0)
            {
                return;
            }

            // Draw numbered labels and filled spheres when selected
            Gizmos.color = _pathColor;
            for (int i = 0; i < _waypoints.Length; i++)
            {
                if (_waypoints[i] != null)
                {
                    Gizmos.DrawSphere(_waypoints[i].position, _waypointGizmoRadius * 0.5f);
                }
            }
        }
    }
}
