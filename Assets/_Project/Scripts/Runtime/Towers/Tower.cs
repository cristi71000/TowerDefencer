using UnityEngine;

namespace TowerDefense.Towers
{
    public class Tower : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private TowerData _towerData;

        [Header("Runtime References")]
        [SerializeField] private Transform _turretPivot;
        [SerializeField] private Transform _firePoint;

        private TargetingPriority _currentPriority;
        private float _attackTimer;
        private Transform _currentTarget;
        private Vector2Int _gridPosition;

        public TowerData Data => _towerData;
        public Vector2Int GridPosition => _gridPosition;
        public TargetingPriority CurrentPriority
        {
            get => _currentPriority;
            set => _currentPriority = value;
        }

        private void Awake()
        {
            if (_towerData != null)
                _currentPriority = _towerData.DefaultPriority;
        }

        public void Initialize(TowerData data, Vector2Int gridPos)
        {
            _towerData = data;
            _gridPosition = gridPos;
            _currentPriority = data.DefaultPriority;
            _attackTimer = 0f;
        }

        private void Update()
        {
            if (_towerData == null) return;
            _attackTimer += Time.deltaTime;
            // Cap timer to prevent floating-point precision issues over long sessions
            if (_attackTimer > _towerData.AttackInterval)
                _attackTimer = _towerData.AttackInterval;
        }

        public void SetTarget(Transform target) => _currentTarget = target;
        public bool CanAttack() => _towerData != null && _attackTimer >= _towerData.AttackInterval;
        public void ResetAttackTimer() => _attackTimer = 0f;
        public Transform GetFirePoint() => _firePoint != null ? _firePoint : transform;
        public Transform GetTurretPivot() => _turretPivot != null ? _turretPivot : transform;

        private void OnDrawGizmosSelected()
        {
            if (_towerData == null) return;
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            DrawCircle(transform.position, _towerData.Range, 32);
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
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
