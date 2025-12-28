## Context

Slow towers provide crowd control by reducing enemy movement speed. This enables other towers more time to deal damage and is essential for strategic depth.

**Builds upon:** Issues 6, 18 (Tower systems, Status Effects)

## Detailed Implementation Instructions

### Slow Tower Data

Create `TD_FreezeTower.asset`:
```
TowerName: "Freeze Tower"
Description: "Slows enemies within range, making them easier targets."
Type: Slow
PurchaseCost: 100
SellValue: 50
Range: 4
AttackSpeed: 2 (fast application)
Damage: 5 (minimal)
SlowAmount: 0.4 (40% slow)
SlowDuration: 2.0
DefaultPriority: First
ProjectileSpeed: 20 (or 0 for instant)
```

### Slow Tower Behavior

The freeze tower can work two ways:
1. **Projectile-based**: Fires slow projectiles
2. **Aura-based**: Continuously slows enemies in range

Implement aura-based for distinctiveness:

Create `SlowAuraTower.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Enemies;
using TowerDefense.Core;

namespace TowerDefense.Towers
{
    public class SlowAuraTower : MonoBehaviour
    {
        [SerializeField] private float _slowAmount = 0.4f;
        [SerializeField] private float _slowDuration = 0.5f;
        [SerializeField] private float _tickRate = 0.25f;

        private Tower _tower;
        private TowerTargeting _targeting;
        private float _tickTimer;

        private void Awake()
        {
            _tower = GetComponent<Tower>();
            _targeting = GetComponent<TowerTargeting>();
        }

        private void Start()
        {
            if (_tower.Data != null)
            {
                _slowAmount = _tower.Data.SlowAmount;
                _slowDuration = _tower.Data.SlowDuration;
            }
        }

        private void Update()
        {
            _tickTimer += Time.deltaTime;
            if (_tickTimer >= _tickRate)
            {
                _tickTimer = 0f;
                ApplySlowToEnemiesInRange();
            }
        }

        private void ApplySlowToEnemiesInRange()
        {
            if (_targeting == null) return;

            foreach (var enemy in _targeting.EnemiesInRange)
            {
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.ApplySlow(_slowAmount, _slowDuration);
                }
            }
        }
    }
}
```

### Freeze Tower Prefab

Create `FreezeTower` prefab:

```
FreezeTower (Tower.cs, TowerTargeting, SlowAuraTower)
|-- Base (Hexagon or cylinder - unique shape)
|-- Crystal (Diamond/pyramid shape - visual centerpiece)
|   +-- FreezeAuraVFX (particle system)
+-- RangeIndicator
```

### Freeze Aura VFX

Create `VFX_FreezeAura.prefab`:
- Particle System:
  - Shape: Circle at base
  - Emission: Constant, low rate (10/s)
  - Particles float upward slowly
  - Color: Light blue/cyan
  - Size: Small snowflake-like
  - Noise module for organic movement

### Slow Visual on Enemies

Add visual feedback when enemy is slowed:

```csharp
// In Enemy.cs
[SerializeField] private GameObject _slowEffectPrefab;
private GameObject _slowEffectInstance;

public void ApplySlow(float slowAmount, float duration)
{
    // ... existing slow logic

    // Show slow effect
    if (_slowEffectPrefab != null && _slowEffectInstance == null)
    {
        _slowEffectInstance = Instantiate(_slowEffectPrefab, transform);
    }
}

private void UpdateSlowEffect()
{
    // ... existing timer logic

    if (_slowTimer <= 0 && _slowEffectInstance != null)
    {
        Destroy(_slowEffectInstance);
        _slowEffectInstance = null;
    }
}
```

### Enemy Slow VFX

Create `VFX_EnemySlow.prefab`:
- Simple particle or tint overlay
- Blue color tint
- Follows enemy

## Testing and Acceptance Criteria

### Done When

- [ ] FreezeTower data asset created
- [ ] FreezeTower prefab with crystal visual
- [ ] Aura continuously applies slow
- [ ] All enemies in range affected
- [ ] Slow effect visible on tower (aura)
- [ ] Slow effect visible on enemies
- [ ] Slow stacks/refreshes correctly
- [ ] Balanced range and slow amount

## Dependencies

- Issue 6: Tower systems
- Issue 18: Status Effects
