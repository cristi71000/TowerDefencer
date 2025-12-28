## Context

With the grid system in place, we need the TowerData ScriptableObject to define tower properties and a basic tower prefab that can be instantiated when placed. This issue establishes the data-driven foundation for all tower types and creates the first placeable tower using Unity primitives.

**Builds upon:** Issue 5 (Grid System)

## Detailed Implementation Instructions

### TowerData ScriptableObject

Create `TowerData.cs` in `_Project/Scripts/Runtime/Towers/`:

```csharp
using UnityEngine;

namespace TowerDefense.Towers
{
    public enum TargetingPriority
    {
        First,      // Closest to exit
        Nearest,    // Closest to tower
        Strongest,  // Highest current health
        Weakest,    // Lowest current health
        Fastest     // Highest speed
    }

    public enum TowerType
    {
        Basic,      // Single target, balanced
        AOE,        // Area damage
        Slow,       // Applies slow effect
        Sniper,     // High damage, slow fire rate
        Support     // Buffs nearby towers
    }

    [CreateAssetMenu(fileName = "NewTowerData", menuName = "TD/Tower Data")]
    public class TowerData : ScriptableObject
    {
        [Header("Identity")]
        public string TowerName;
        public string Description;
        public Sprite Icon;
        public TowerType Type;

        [Header("Prefab")]
        public GameObject Prefab;

        [Header("Economy")]
        public int PurchaseCost;
        public int SellValue;

        [Header("Combat Stats")]
        public float Range = 5f;
        public float AttackSpeed = 1f;
        public int Damage = 10;
        public TargetingPriority DefaultPriority = TargetingPriority.First;

        [Header("Projectile")]
        public GameObject ProjectilePrefab;
        public float ProjectileSpeed = 10f;

        [Header("Special Effects")]
        public float SlowAmount = 0f;
        public float SlowDuration = 0f;
        public float AOERadius = 0f;
        public float BuffRadius = 0f;
        public float BuffAmount = 0f;

        [Header("Upgrades")]
        public TowerData[] UpgradesTo;
        public TowerData UpgradesFrom;

        [Header("Audio/Visual")]
        public AudioClip AttackSound;
        public GameObject MuzzleFlashPrefab;

        public float AttackInterval => 1f / AttackSpeed;
        public bool IsAOE => AOERadius > 0;
        public bool HasSlowEffect => SlowAmount > 0 && SlowDuration > 0;
        public bool IsSupportTower => Type == TowerType.Support;
    }
}
```

### Tower Component

Create `Tower.cs` in `_Project/Scripts/Runtime/Towers/`:

```csharp
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
        }

        public void SetTarget(Transform target) => _currentTarget = target;
        public bool CanAttack() => _attackTimer >= _towerData.AttackInterval;
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
```

### Range Indicator Component

Create `RangeIndicator.cs`:

```csharp
using UnityEngine;

namespace TowerDefense.Towers
{
    [RequireComponent(typeof(LineRenderer))]
    public class RangeIndicator : MonoBehaviour
    {
        [SerializeField] private int _segments = 64;
        [SerializeField] private float _lineWidth = 0.1f;
        [SerializeField] private Material _rangeMaterial;

        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.loop = true;
            _lineRenderer.positionCount = _segments;
            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _lineWidth;
            if (_rangeMaterial != null) _lineRenderer.material = _rangeMaterial;
            gameObject.SetActive(false);
        }

        public void SetRadius(float radius)
        {
            float angleStep = 360f / _segments;
            for (int i = 0; i < _segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                _lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0.1f, Mathf.Sin(angle) * radius));
            }
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
```

### Basic Tower Prefab Structure

```
BasicTower (Tower.cs, BoxCollider)
|-- Base (Cube scaled 1.5, 0.5, 1.5)
|-- TurretPivot (empty, Y = 0.5)
|   +-- Turret (Cube scaled 0.5, 0.5, 1.2)
|       +-- FirePoint (empty, at end of turret barrel)
+-- RangeIndicator (RangeIndicator.cs, LineRenderer)
```

Save as `_Project/Prefabs/Towers/BasicTower.prefab`

### Create Basic Tower Data Asset

Create at `_Project/ScriptableObjects/TowerData/TD_BasicTower.asset`:

- Tower Name: "Basic Tower"
- Type: Basic
- Purchase Cost: 100
- Sell Value: 50
- Range: 6
- Attack Speed: 1
- Damage: 15
- Default Priority: First
- Projectile Speed: 15

## Testing and Acceptance Criteria

### Done When

- [ ] TowerData ScriptableObject created with all fields
- [ ] Tower component created with initialization method
- [ ] RangeIndicator component draws circle at runtime
- [ ] BasicTower prefab created with correct hierarchy
- [ ] Tower prefab on layer 7 with "Tower" tag
- [ ] TD_BasicTower asset created with balanced stats
- [ ] Range gizmo visible when tower selected
- [ ] No errors in play mode

## Dependencies

- Issue 5: Grid System
