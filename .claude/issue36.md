## Context

Tank enemies are slow but have high health and armor. They test the player's damage output capacity and reward sniper/high-damage towers.

**Builds upon:** Issue 9 (Enemy systems)

## Detailed Implementation Instructions

### Tank Enemy Data

Create `ED_TankEnemy.asset`:
```
EnemyName: "Tank"
Type: Tank
MaxHealth: 400 (4x basic)
MoveSpeed: 1.5 (half basic speed)
Armor: 5 (reduces each hit by 5)
CurrencyReward: 25
LivesDamage: 3
IsFlying: false
IsImmuneToSlow: true (cannot be slowed)
SlowResistance: 1.0
ModelScale: 1.5 (larger)
```

### Tank Enemy Prefab

Create `TankEnemy` prefab:

```
TankEnemy (Enemy.cs, NavMeshAgent, CapsuleCollider)
|-- Model
|   |-- Body (Cube, wide and sturdy)
|   +-- ArmorPlates (additional cubes for detail)
+-- HealthBarAnchor
```

Visual design:
- Larger than basic enemy
- Boxy/armored appearance
- Dark metallic color
- Slower, heavier movement

### Armor Visual Feedback

Show armor with visual indicator:

```csharp
// In Enemy.cs, add armor indicator
[SerializeField] private GameObject _armorIndicator;

public void TakeDamage(DamageInfo info)
{
    // ... existing damage code

    // Flash armor indicator when armor absorbs damage
    if (_enemyData.Armor > 0 && _armorIndicator != null)
    {
        StartCoroutine(FlashArmor());
    }
}

private System.Collections.IEnumerator FlashArmor()
{
    _armorIndicator.SetActive(true);
    yield return new WaitForSeconds(0.1f);
    _armorIndicator.SetActive(false);
}
```

### Tank Enemy Material

Create `M_Enemy_Tank.mat`:
- Color: Dark gray RGB(80, 80, 80)
- Metallic: 0.8
- Smoothness: 0.6

### Armor Plate Material

Create `M_Enemy_Armor.mat`:
- Color: Dark bronze RGB(100, 70, 50)
- Metallic: 1.0

## Testing and Acceptance Criteria

### Done When

- [ ] TankEnemy data asset created
- [ ] TankEnemy prefab with armored visual
- [ ] Moves at half speed
- [ ] Has 4x health
- [ ] Armor reduces damage correctly
- [ ] Immune to slow effects
- [ ] Armor flash on hit
- [ ] Larger scale than basic

## Dependencies

- Issue 9: Enemy systems
