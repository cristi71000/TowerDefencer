## Context

Fast enemies test the player's ability to cover the entire path. They have low health but high speed, requiring precise placement and fast-attacking towers.

**Builds upon:** Issue 9 (Enemy systems)

## Detailed Implementation Instructions

### Fast Enemy Data

Create `ED_FastEnemy.asset`:
```
EnemyName: "Runner"
Type: Fast
MaxHealth: 50 (low)
MoveSpeed: 6 (double basic)
Armor: 0
CurrencyReward: 8
LivesDamage: 1
IsFlying: false
IsImmuneToSlow: false
SlowResistance: 0.3 (30% slow resistance)
ModelScale: 0.8 (smaller)
```

### Fast Enemy Prefab

Create `FastEnemy` prefab:

```
FastEnemy (Enemy.cs, NavMeshAgent, CapsuleCollider)
|-- Model
|   +-- Body (Capsule, thin and elongated)
+-- HealthBarAnchor
```

Visual design:
- Smaller than basic enemy
- Elongated shape suggests speed
- Bright color (cyan/light blue)
- Consider adding motion trail

### Speed Trail Effect

Create `SpeedTrail.cs`:

```csharp
using UnityEngine;

namespace TowerDefense.Enemies
{
    public class SpeedTrail : MonoBehaviour
    {
        [SerializeField] private TrailRenderer _trail;
        [SerializeField] private float _minSpeedForTrail = 3f;

        private Enemy _enemy;

        private void Awake()
        {
            _enemy = GetComponentInParent<Enemy>();
        }

        private void Update()
        {
            if (_trail != null && _enemy != null)
            {
                _trail.emitting = _enemy.CurrentSpeed >= _minSpeedForTrail;
            }
        }
    }
}
```

### Update Enemy Pool

Update EnemyPoolManager to prewarm fast enemy pool based on wave data.

### Fast Enemy Material

Create `M_Enemy_Fast.mat`:
- Color: Cyan RGB(50, 200, 200)
- Slight emissive glow

## Testing and Acceptance Criteria

### Done When

- [ ] FastEnemy data asset created
- [ ] FastEnemy prefab with distinct visual
- [ ] Moves at 2x basic enemy speed
- [ ] Has 50% health of basic enemy
- [ ] 30% slow resistance works
- [ ] Speed trail visible when moving
- [ ] Properly pools and despawns

## Dependencies

- Issue 9: Enemy systems
