using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Handles target detection and selection for towers using Physics.OverlapSphereNonAlloc.
    /// Supports multiple targeting priorities: First, Nearest, Strongest, Weakest, Fastest.
    /// </summary>
    [RequireComponent(typeof(Tower))]
    public class TowerTargeting : MonoBehaviour
    {
        private const int MaxTargetBuffer = 32;
        private readonly Collider[] _hitBuffer = new Collider[MaxTargetBuffer];

        [Header("Targeting Configuration")]
        [SerializeField] private LayerMask _enemyLayerMask = 1 << 8; // Layer 8 = Enemy
        [SerializeField] private float _targetUpdateInterval = 0.1f;

        private Tower _tower;
        private ITargetable _currentTarget;
        private float _lastTargetUpdateTime;
        private readonly List<ITargetable> _validTargets = new List<ITargetable>(MaxTargetBuffer);

        /// <summary>
        /// The currently selected target, or null if no valid target exists.
        /// </summary>
        public ITargetable CurrentTarget => _currentTarget;

        /// <summary>
        /// Returns true if there is a valid target in range.
        /// </summary>
        public bool HasTarget => _currentTarget != null && _currentTarget.IsValidTarget;

        /// <summary>
        /// Event fired when the target changes.
        /// </summary>
        public event System.Action<ITargetable> OnTargetChanged;

        private void Awake()
        {
            _tower = GetComponent<Tower>();
        }

        private void Update()
        {
            if (_tower.Data == null) return;

            // Throttle target updates to reduce CPU usage
            if (Time.time - _lastTargetUpdateTime >= _targetUpdateInterval)
            {
                UpdateTarget();
                _lastTargetUpdateTime = Time.time;
            }
            else
            {
                // Validate current target between full updates
                ValidateCurrentTarget();
            }
        }

        /// <summary>
        /// Forces an immediate target update, bypassing the throttle interval.
        /// </summary>
        public void ForceTargetUpdate()
        {
            UpdateTarget();
            _lastTargetUpdateTime = Time.time;
        }

        /// <summary>
        /// Gets all valid targets currently in range.
        /// This method refreshes the internal target list and may affect the tower's targeting
        /// state if called between regular Update cycles.
        /// </summary>
        /// <returns>A snapshot copy of valid targets.</returns>
        public IReadOnlyList<ITargetable> GetTargetsInRange()
        {
            GatherValidTargets();
            // Return a defensive copy so external callers cannot mutate the internal list
            return new List<ITargetable>(_validTargets);
        }

        private void UpdateTarget()
        {
            GatherValidTargets();

            ITargetable newTarget = SelectBestTarget(_validTargets, _tower.CurrentPriority);

            if (newTarget != _currentTarget)
            {
                _currentTarget = newTarget;

                // Update tower's target reference for compatibility
                if (_currentTarget != null && _currentTarget.TargetPoint != null)
                {
                    _tower.SetTarget(_currentTarget.TargetPoint);
                }
                else
                {
                    _tower.SetTarget(null);
                }

                OnTargetChanged?.Invoke(_currentTarget);
            }
        }

        private void ValidateCurrentTarget()
        {
            if (_currentTarget == null) return;

            // Check if target is still valid
            if (!_currentTarget.IsValidTarget)
            {
                _currentTarget = null;
                _tower.SetTarget(null);
                OnTargetChanged?.Invoke(null);
                return;
            }

            // Check if target is still in range
            if (_currentTarget.TargetPoint != null)
            {
                float distanceSqr = (transform.position - _currentTarget.TargetPoint.position).sqrMagnitude;
                float rangeSqr = _tower.Data.Range * _tower.Data.Range;

                if (distanceSqr > rangeSqr)
                {
                    _currentTarget = null;
                    _tower.SetTarget(null);
                    OnTargetChanged?.Invoke(null);
                }
            }
        }

        private void GatherValidTargets()
        {
            _validTargets.Clear();

            if (_tower.Data == null) return;

            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                _tower.Data.Range,
                _hitBuffer,
                _enemyLayerMask
            );

            if (hitCount == MaxTargetBuffer)
            {
                Debug.LogWarning(
                    $"[TowerTargeting] Target buffer may be too small for tower '{name}'. " +
                    $"OverlapSphereNonAlloc returned {hitCount} results, which equals MaxTargetBuffer ({MaxTargetBuffer}). " +
                    "Some enemies in range may not be considered as targets."
                );
            }

            for (int i = 0; i < hitCount; i++)
            {
                // Try to get ITargetable from the collider's GameObject
                if (_hitBuffer[i].TryGetComponent<ITargetable>(out var targetable) &&
                    targetable.IsValidTarget)
                {
                    _validTargets.Add(targetable);
                }
            }
        }

        private ITargetable SelectBestTarget(List<ITargetable> targets, TargetingPriority priority)
        {
            if (targets.Count == 0) return null;
            if (targets.Count == 1) return targets[0];

            return priority switch
            {
                TargetingPriority.First => GetFirstTarget(targets),
                TargetingPriority.Nearest => GetNearestTarget(targets),
                TargetingPriority.Strongest => GetStrongestTarget(targets),
                TargetingPriority.Weakest => GetWeakestTarget(targets),
                TargetingPriority.Fastest => GetFastestTarget(targets),
                _ => GetFirstTarget(targets)
            };
        }

        /// <summary>
        /// Selects the enemy that has traveled the furthest along the path (closest to exit).
        /// </summary>
        private ITargetable GetFirstTarget(List<ITargetable> targets)
        {
            ITargetable best = targets[0];
            float maxDistance = best.DistanceTraveled;

            for (int i = 1; i < targets.Count; i++)
            {
                if (targets[i].DistanceTraveled > maxDistance)
                {
                    maxDistance = targets[i].DistanceTraveled;
                    best = targets[i];
                }
            }

            return best;
        }

        /// <summary>
        /// Selects the enemy closest to the tower.
        /// </summary>
        private ITargetable GetNearestTarget(List<ITargetable> targets)
        {
            ITargetable best = targets[0];
            float minDistSqr = GetDistanceSquared(best);

            for (int i = 1; i < targets.Count; i++)
            {
                float distSqr = GetDistanceSquared(targets[i]);
                if (distSqr < minDistSqr)
                {
                    minDistSqr = distSqr;
                    best = targets[i];
                }
            }

            return best;
        }

        /// <summary>
        /// Selects the enemy with the highest current health.
        /// </summary>
        private ITargetable GetStrongestTarget(List<ITargetable> targets)
        {
            ITargetable best = targets[0];
            int maxHealth = best.CurrentHealth;

            for (int i = 1; i < targets.Count; i++)
            {
                if (targets[i].CurrentHealth > maxHealth)
                {
                    maxHealth = targets[i].CurrentHealth;
                    best = targets[i];
                }
            }

            return best;
        }

        /// <summary>
        /// Selects the enemy with the lowest current health.
        /// </summary>
        private ITargetable GetWeakestTarget(List<ITargetable> targets)
        {
            ITargetable best = targets[0];
            int minHealth = best.CurrentHealth;

            for (int i = 1; i < targets.Count; i++)
            {
                if (targets[i].CurrentHealth < minHealth)
                {
                    minHealth = targets[i].CurrentHealth;
                    best = targets[i];
                }
            }

            return best;
        }

        /// <summary>
        /// Selects the enemy with the highest current speed.
        /// </summary>
        private ITargetable GetFastestTarget(List<ITargetable> targets)
        {
            ITargetable best = targets[0];
            float maxSpeed = best.CurrentSpeed;

            for (int i = 1; i < targets.Count; i++)
            {
                if (targets[i].CurrentSpeed > maxSpeed)
                {
                    maxSpeed = targets[i].CurrentSpeed;
                    best = targets[i];
                }
            }

            return best;
        }

        private float GetDistanceSquared(ITargetable target)
        {
            if (target.TargetPoint == null) return float.MaxValue;
            return (transform.position - target.TargetPoint.position).sqrMagnitude;
        }

        private void OnDrawGizmosSelected()
        {
            Tower tower = _tower != null ? _tower : GetComponent<Tower>();
            if (tower == null || tower.Data == null) return;

            // Draw range circle
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
            DrawWireSphere(transform.position, tower.Data.Range);

            // Draw line to current target
            if (_currentTarget != null && _currentTarget.TargetPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, _currentTarget.TargetPoint.position);
            }
        }

        private void DrawWireSphere(Vector3 center, float radius)
        {
            const int segments = 32;
            float angleStep = 360f / segments;

            // Horizontal circle
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }
    }
}
