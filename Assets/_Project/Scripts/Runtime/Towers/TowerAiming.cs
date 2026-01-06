using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Handles turret rotation and aim prediction for towers.
    /// Smoothly rotates the turret pivot to face the current target with optional lead prediction.
    /// </summary>
    [RequireComponent(typeof(Tower))]
    [RequireComponent(typeof(TowerTargeting))]
    public class TowerAiming : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [SerializeField] private float _rotationSpeed = 180f;
        [SerializeField] private bool _useLeadPrediction = true;
        [SerializeField] private bool _lockYAxis = true;

        [Header("Debug")]
        [SerializeField] private bool _showDebugGizmos;

        private Tower _tower;
        private TowerTargeting _targeting;
        private Transform _turretPivot;
        private Vector3 _predictedPosition;
        private bool _isAimed;

        /// <summary>
        /// Returns true if the turret is currently aimed at the target within tolerance.
        /// </summary>
        public bool IsAimed => _isAimed;

        /// <summary>
        /// The predicted position where the projectile should hit the target.
        /// </summary>
        public Vector3 PredictedTargetPosition => _predictedPosition;

        /// <summary>
        /// The current aim direction (normalized).
        /// </summary>
        public Vector3 AimDirection
        {
            get
            {
                if (_turretPivot == null) return transform.forward;
                return _turretPivot.forward;
            }
        }

        private void Awake()
        {
            _tower = GetComponent<Tower>();
            _targeting = GetComponent<TowerTargeting>();
        }

        private void Start()
        {
            if (_tower != null)
            {
                _turretPivot = _tower.GetTurretPivot();
            }
        }

        private void Update()
        {
            // Ensure we have a valid turret pivot; attempt to re-fetch if not set yet
            if (_turretPivot == null)
            {
                if (_tower != null)
                {
                    _turretPivot = _tower.GetTurretPivot();
                }

                if (_turretPivot == null)
                {
                    _isAimed = false;
                    return;
                }
            }

            // Ensure the tower data is available before proceeding
            if (_tower == null || _tower.Data == null)
            {
                _isAimed = false;
                return;
            }

            ITargetable target = _targeting.CurrentTarget;

            if (target == null || target.TargetPoint == null || !target.IsValidTarget)
            {
                _isAimed = false;
                return;
            }

            // Calculate target position (with or without prediction)
            _predictedPosition = CalculateAimPosition(target);

            // Rotate turret toward target
            RotateTurretToward(_predictedPosition);

            // Check if we're aimed close enough
            _isAimed = CheckAimAlignment(_predictedPosition);
        }

        /// <summary>
        /// Calculates the aim position, optionally using lead prediction for moving targets.
        /// </summary>
        private Vector3 CalculateAimPosition(ITargetable target)
        {
            Vector3 targetPos = target.TargetPoint.position;

            if (!_useLeadPrediction || _tower.Data.ProjectileSpeed <= 0)
            {
                return targetPos;
            }

            // Get target velocity from the transform
            Vector3 targetVelocity = GetTargetVelocity(target);

            if (targetVelocity.sqrMagnitude < 0.01f)
            {
                return targetPos;
            }

            // Calculate lead position using projectile speed
            Vector3 firePoint = _tower.GetFirePoint().position;
            return CalculateLeadPosition(firePoint, targetPos, targetVelocity, _tower.Data.ProjectileSpeed);
        }

        /// <summary>
        /// Calculates the predicted intercept position for a moving target.
        /// Uses iterative refinement for stable convergence.
        /// </summary>
        private Vector3 CalculateLeadPosition(Vector3 shooterPos, Vector3 targetPos, Vector3 targetVelocity, float projectileSpeed)
        {
            // Guard against invalid projectile speed
            if (projectileSpeed <= 0f)
            {
                return targetPos;
            }

            // Use iterative approach for stable lead prediction
            const int maxIterations = 3;
            Vector3 predictedPos = targetPos;

            for (int i = 0; i < maxIterations; i++)
            {
                Vector3 toTarget = predictedPos - shooterPos;
                float distance = toTarget.magnitude;

                // Guard against division by zero
                if (distance < 0.001f)
                {
                    return targetPos;
                }

                float timeToHit = distance / projectileSpeed;

                // Clamp time to reasonable range (max 3 seconds prediction)
                timeToHit = Mathf.Clamp(timeToHit, 0f, 3f);

                predictedPos = targetPos + targetVelocity * timeToHit;
            }

            return predictedPos;
        }

        /// <summary>
        /// Gets the velocity of the target from the ITargetable interface.
        /// </summary>
        private Vector3 GetTargetVelocity(ITargetable target)
        {
            // Use the Velocity property from ITargetable (avoids GetComponent calls)
            return target.Velocity;
        }

        /// <summary>
        /// Smoothly rotates the turret toward the target position.
        /// </summary>
        private void RotateTurretToward(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - _turretPivot.position;

            if (_lockYAxis)
            {
                direction.y = 0f;
            }

            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            float maxRotation = _rotationSpeed * Time.deltaTime;

            _turretPivot.rotation = Quaternion.RotateTowards(
                _turretPivot.rotation,
                targetRotation,
                maxRotation
            );
        }

        /// <summary>
        /// Checks if the turret is aimed at the target within an acceptable tolerance.
        /// </summary>
        private bool CheckAimAlignment(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - _turretPivot.position;

            if (_lockYAxis)
            {
                direction.y = 0f;
            }

            if (direction.sqrMagnitude < 0.001f) return true;

            float angle = Vector3.Angle(_turretPivot.forward, direction.normalized);

            // Consider aimed if within 5 degrees
            return angle < 5f;
        }

        /// <summary>
        /// Immediately snaps the turret to face the target (no smooth rotation).
        /// </summary>
        public void SnapToTarget()
        {
            if (_turretPivot == null) return;

            ITargetable target = _targeting.CurrentTarget;
            if (target == null || target.TargetPoint == null) return;

            Vector3 direction = target.TargetPoint.position - _turretPivot.position;

            if (_lockYAxis)
            {
                direction.y = 0f;
            }

            if (direction.sqrMagnitude > 0.001f)
            {
                _turretPivot.rotation = Quaternion.LookRotation(direction.normalized);
                _isAimed = true;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showDebugGizmos) return;

            Tower tower = _tower != null ? _tower : GetComponent<Tower>();
            TowerTargeting targeting = _targeting != null ? _targeting : GetComponent<TowerTargeting>();

            if (tower == null || targeting == null) return;

            // Draw fire point
            Transform firePoint = tower.GetFirePoint();
            if (firePoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(firePoint.position, 0.15f);
            }

            // Draw predicted position (only when there's a valid target)
            if (targeting.CurrentTarget != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_predictedPosition, 0.3f);

                // Draw line from fire point to predicted position
                if (firePoint != null)
                {
                    Gizmos.DrawLine(firePoint.position, _predictedPosition);
                }
            }

            // Draw aim direction
            Transform turretPivot = tower.GetTurretPivot();
            if (turretPivot != null)
            {
                Gizmos.color = _isAimed ? Color.green : Color.red;
                Gizmos.DrawRay(turretPivot.position, turretPivot.forward * 3f);
            }
        }
    }
}
