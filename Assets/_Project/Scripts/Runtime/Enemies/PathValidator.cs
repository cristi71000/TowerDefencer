using UnityEngine;
using UnityEngine.AI;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// Validates and visualizes NavMesh paths between spawn and exit points.
    /// Attach this component to any GameObject in the scene to enable path validation.
    ///
    /// USAGE:
    /// 1. Add this component to any GameObject (e.g., create an empty "PathValidation" object)
    /// 2. Assign the SpawnPoint and ExitPoint transforms, or leave null to use singletons
    /// 3. The component will automatically visualize the path in the Scene view:
    ///    - GREEN path = valid path exists
    ///    - RED line = invalid/incomplete path
    /// </summary>
    public class PathValidator : MonoBehaviour
    {
        [Header("Path Points")]
        [Tooltip("The starting point for path validation. If null, uses SpawnPoint.Instance.")]
        [SerializeField] private Transform _spawnPoint;

        [Tooltip("The destination point for path validation. If null, uses ExitPoint.Instance.")]
        [SerializeField] private Transform _exitPoint;

        [Header("Gizmo Settings")]
        [SerializeField] private Color _validPathColor = Color.green;
        [SerializeField] private Color _invalidPathColor = Color.red;
        [SerializeField] private float _pathWidth = 0.2f;
        [SerializeField] private float _waypointRadius = 0.3f;
        [SerializeField] private bool _showWaypoints = true;
        [SerializeField] private bool _alwaysShowGizmos = true;

        [Header("Debug")]
        [SerializeField] private bool _logPathStatus = false;

        private NavMeshPath _cachedPath;
        private bool _lastPathValid;
        private float _lastValidationTime;
        private const float VALIDATION_COOLDOWN = 0.5f;

        /// <summary>
        /// Gets whether the last validated path was valid.
        /// </summary>
        public bool IsLastPathValid => _lastPathValid;

        private void Awake()
        {
            _cachedPath = new NavMeshPath();
        }

        private void OnValidate()
        {
            // Force revalidation when inspector values change
            _lastValidationTime = 0f;
        }

        /// <summary>
        /// Checks if a valid NavMesh path exists from spawn to exit.
        /// </summary>
        /// <returns>True if a complete path exists, false otherwise.</returns>
        public bool IsPathValid()
        {
            return IsPathValid(out _);
        }

        /// <summary>
        /// Checks if a valid NavMesh path exists from spawn to exit.
        /// </summary>
        /// <param name="path">The calculated NavMesh path if valid, empty path if invalid.</param>
        /// <returns>True if a complete path exists, false otherwise.</returns>
        public bool IsPathValid(out NavMeshPath path)
        {
            path = new NavMeshPath();

            Vector3? startPos = GetSpawnPosition();
            Vector3? endPos = GetExitPosition();

            if (!startPos.HasValue || !endPos.HasValue)
            {
                if (_logPathStatus)
                {
                    UnityEngine.Debug.LogWarning("[PathValidator] Cannot validate path: SpawnPoint or ExitPoint not found.");
                }
                return false;
            }

            bool calculated = NavMesh.CalculatePath(startPos.Value, endPos.Value, NavMesh.AllAreas, path);

            if (!calculated)
            {
                if (_logPathStatus)
                {
                    UnityEngine.Debug.LogWarning("[PathValidator] NavMesh.CalculatePath failed. Is the NavMesh baked?");
                }
                return false;
            }

            bool isComplete = path.status == NavMeshPathStatus.PathComplete;

            if (_logPathStatus)
            {
                if (isComplete)
                {
                    UnityEngine.Debug.Log($"[PathValidator] Valid path found with {path.corners.Length} waypoints.");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[PathValidator] Path is incomplete. Status: {path.status}");
                }
            }

            return isComplete;
        }

        /// <summary>
        /// Gets the path distance from spawn to exit.
        /// </summary>
        /// <returns>Total path distance, or -1 if path is invalid.</returns>
        public float GetPathDistance()
        {
            if (!IsPathValid(out NavMeshPath path))
            {
                return -1f;
            }

            float distance = 0f;
            Vector3[] corners = path.corners;

            for (int i = 0; i < corners.Length - 1; i++)
            {
                distance += Vector3.Distance(corners[i], corners[i + 1]);
            }

            return distance;
        }

        /// <summary>
        /// Gets the estimated travel time for an enemy to traverse the path.
        /// </summary>
        /// <param name="moveSpeed">The enemy's movement speed.</param>
        /// <returns>Estimated time in seconds, or -1 if path is invalid.</returns>
        public float GetEstimatedTravelTime(float moveSpeed)
        {
            float distance = GetPathDistance();
            if (distance < 0f || moveSpeed <= 0f)
            {
                return -1f;
            }
            return distance / moveSpeed;
        }

        private Vector3? GetSpawnPosition()
        {
            if (_spawnPoint != null)
            {
                return _spawnPoint.position;
            }

            if (TowerDefense.Core.SpawnPoint.Instance != null)
            {
                return TowerDefense.Core.SpawnPoint.Instance.Position;
            }

            return null;
        }

        private Vector3? GetExitPosition()
        {
            if (_exitPoint != null)
            {
                return _exitPoint.position;
            }

            if (TowerDefense.Core.ExitPoint.Instance != null)
            {
                return TowerDefense.Core.ExitPoint.Instance.Position;
            }

            return null;
        }

        private void UpdateCachedPath()
        {
            if (Time.realtimeSinceStartup - _lastValidationTime < VALIDATION_COOLDOWN)
            {
                return;
            }

            _lastValidationTime = Time.realtimeSinceStartup;

            Vector3? startPos = GetSpawnPosition();
            Vector3? endPos = GetExitPosition();

            if (!startPos.HasValue || !endPos.HasValue)
            {
                _lastPathValid = false;
                return;
            }

            if (_cachedPath == null)
            {
                _cachedPath = new NavMeshPath();
            }

            bool calculated = NavMesh.CalculatePath(startPos.Value, endPos.Value, NavMesh.AllAreas, _cachedPath);
            _lastPathValid = calculated && _cachedPath.status == NavMeshPathStatus.PathComplete;
        }

        private void OnDrawGizmos()
        {
            if (_alwaysShowGizmos)
            {
                DrawPathGizmos();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_alwaysShowGizmos)
            {
                DrawPathGizmos();
            }
        }

        private void DrawPathGizmos()
        {
            UpdateCachedPath();

            Vector3? startPos = GetSpawnPosition();
            Vector3? endPos = GetExitPosition();

            if (!startPos.HasValue || !endPos.HasValue)
            {
                return;
            }

            if (_cachedPath != null && _cachedPath.corners.Length > 0 && _lastPathValid)
            {
                // Draw valid path
                Gizmos.color = _validPathColor;
                Vector3[] corners = _cachedPath.corners;

                for (int i = 0; i < corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(corners[i], corners[i + 1]);

                    // Draw waypoint spheres
                    if (_showWaypoints)
                    {
                        Gizmos.DrawWireSphere(corners[i], _waypointRadius);
                    }
                }

                // Draw last waypoint
                if (_showWaypoints && corners.Length > 0)
                {
                    Gizmos.DrawWireSphere(corners[corners.Length - 1], _waypointRadius);
                }

                // Draw path direction arrows
                for (int i = 0; i < corners.Length - 1; i++)
                {
                    DrawArrow(corners[i], corners[i + 1], _validPathColor);
                }
            }
            else
            {
                // Draw invalid path indicator (straight line from spawn to exit)
                Gizmos.color = _invalidPathColor;
                Gizmos.DrawLine(startPos.Value, endPos.Value);

                // Draw X marks at both ends
                DrawXMark(startPos.Value, _waypointRadius);
                DrawXMark(endPos.Value, _waypointRadius);
            }

            // Draw labels
            DrawPointLabel(startPos.Value, "SPAWN", _validPathColor);
            DrawPointLabel(endPos.Value, "EXIT", new Color(1f, 0.5f, 0f));
        }

        private void DrawArrow(Vector3 from, Vector3 to, Color color)
        {
            Vector3 direction = (to - from).normalized;
            Vector3 midPoint = (from + to) * 0.5f;

            if (direction == Vector3.zero) return;

            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
            if (right == Vector3.zero)
            {
                right = Vector3.Cross(Vector3.forward, direction).normalized;
            }

            float arrowSize = 0.3f;
            Vector3 arrowLeft = midPoint - direction * arrowSize + right * arrowSize * 0.5f;
            Vector3 arrowRight = midPoint - direction * arrowSize - right * arrowSize * 0.5f;

            Gizmos.color = color;
            Gizmos.DrawLine(midPoint, arrowLeft);
            Gizmos.DrawLine(midPoint, arrowRight);
        }

        private void DrawXMark(Vector3 position, float size)
        {
            Gizmos.DrawLine(
                position + new Vector3(-size, 0, -size),
                position + new Vector3(size, 0, size)
            );
            Gizmos.DrawLine(
                position + new Vector3(-size, 0, size),
                position + new Vector3(size, 0, -size)
            );
        }

        private void DrawPointLabel(Vector3 position, string label, Color color)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.Label(position + Vector3.up * 1.5f, label);
#endif
        }
    }
}
