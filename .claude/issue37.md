## Context

Flying enemies bypass ground obstacles and potentially take different paths. They require towers that can target air units and add a layer of strategic complexity.

**Builds upon:** Issue 9 (Enemy systems)

## Detailed Implementation Instructions

### Flying Enemy Data

Create `ED_FlyingEnemy.asset`:
```
EnemyName: "Flyer"
Type: Flying
MaxHealth: 60
MoveSpeed: 4
Armor: 0
CurrencyReward: 15
LivesDamage: 2
IsFlying: true
IsImmuneToSlow: true
SlowResistance: 1.0
ModelScale: 0.9
```

### Flying Enemy Movement

Flying enemies can either:
1. Follow same path but elevated
2. Take direct route (more challenging)

Create `FlyingEnemy.cs`:

```csharp
using UnityEngine;
using UnityEngine.AI;
using TowerDefense.Core;

namespace TowerDefense.Enemies
{
    public class FlyingEnemy : MonoBehaviour
    {
        [SerializeField] private float _flyHeight = 2f;
        [SerializeField] private bool _directPath = false;

        private Enemy _enemy;
        private NavMeshAgent _agent;
        private Transform _target;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            if (_directPath)
            {
                // Disable NavMesh, fly directly
                _agent.enabled = false;
                _target = ExitPoint.Instance?.transform;
            }
            else
            {
                // Offset NavMesh position for visual height
                _agent.baseOffset = _flyHeight;
            }
        }

        private void Update()
        {
            if (_directPath && _target != null && !_enemy.IsDead)
            {
                // Move directly toward exit
                Vector3 direction = (_target.position - transform.position).normalized;
                transform.position += direction * _enemy.Data.MoveSpeed * Time.deltaTime;

                // Check arrival
                if (Vector3.Distance(transform.position, _target.position) < 1f)
                {
                    // Reached end - trigger via Enemy component
                }
            }
        }
    }
}
```

### Flying Enemy Prefab

Create `FlyingEnemy` prefab:

```
FlyingEnemy (Enemy.cs, FlyingEnemy.cs, NavMeshAgent)
|-- Model
|   |-- Body (Sphere or diamond shape)
|   +-- Wings (Two angled planes)
|       +-- WingAnimator
+-- HealthBarAnchor
```

### Wing Animation

Simple bobbing/flapping animation:

```csharp
public class WingAnimation : MonoBehaviour
{
    [SerializeField] private float _flapSpeed = 10f;
    [SerializeField] private float _flapAngle = 20f;

    private void Update()
    {
        float angle = Mathf.Sin(Time.time * _flapSpeed) * _flapAngle;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
```

### Tower Anti-Air Configuration

Update TowerData to support anti-air:

```csharp
// Add to TowerData.cs
[Header("Targeting Filters")]
public bool CanTargetGround = true;
public bool CanTargetAir = true;
```

Update TowerTargeting to filter by air/ground:

```csharp
// In TowerTargeting.cs UpdateTargets()
if (enemy.IsFlying && !_tower.Data.CanTargetAir) continue;
if (!enemy.IsFlying && !_tower.Data.CanTargetGround) continue;
```

### Flying Enemy Material

Create `M_Enemy_Flying.mat`:
- Color: Purple/Magenta RGB(180, 50, 180)
- Slight transparency for ethereal look

## Testing and Acceptance Criteria

### Done When

- [ ] FlyingEnemy data asset created
- [ ] FlyingEnemy prefab with wings
- [ ] Flies above ground level
- [ ] Wing flapping animation
- [ ] Towers can filter air/ground targets
- [ ] Basic tower can target flyers
- [ ] Immune to slow
- [ ] Reaches exit correctly

## Dependencies

- Issue 9: Enemy systems
- Issue 14: Targeting (for air filter)
