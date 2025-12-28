## Context

Each tower type should have multiple upgrade tiers. This issue creates the upgrade data for all tower types, establishing a complete progression system.

**Builds upon:** Issues 30-33, 40 (Tower variety, Upgrade system)

## Detailed Implementation Instructions

### Upgrade Philosophy

- **Tier 0 (Base):** Starting tower, affordable, basic stats
- **Tier 1:** Moderate improvement, ~1.5x cost
- **Tier 2:** Significant improvement, ~2.5x cost

### Basic Tower Upgrade Path

**TD_BasicTower.asset (Tier 0):**
```
TowerName: "Arrow Tower"
PurchaseCost: 100
Damage: 15
AttackSpeed: 1.0
Range: 6
UpgradesTo: [TD_BasicTower_T1]
```

**TD_BasicTower_T1.asset (Tier 1):**
```
TowerName: "Improved Arrow Tower"
PurchaseCost: 175
SellValue: 90
Damage: 25
AttackSpeed: 1.25
Range: 7
UpgradesFrom: TD_BasicTower
UpgradesTo: [TD_BasicTower_T2]
```

**TD_BasicTower_T2.asset (Tier 2):**
```
TowerName: "Master Archer"
PurchaseCost: 300
SellValue: 150
Damage: 45
AttackSpeed: 1.5
Range: 8
UpgradesFrom: TD_BasicTower_T1
```

### Cannon Tower Upgrade Path

**TD_CannonTower_T1.asset:**
```
TowerName: "Heavy Cannon"
PurchaseCost: 250
Damage: 40
AOERadius: 2.5
```

**TD_CannonTower_T2.asset:**
```
TowerName: "Artillery"
PurchaseCost: 400
Damage: 60
AOERadius: 3.5
```

### Freeze Tower Upgrade Path

**TD_FreezeTower_T1.asset:**
```
TowerName: "Frost Tower"
PurchaseCost: 175
SlowAmount: 0.5
SlowDuration: 2.5
Range: 4.5
```

**TD_FreezeTower_T2.asset:**
```
TowerName: "Blizzard Tower"
PurchaseCost: 300
SlowAmount: 0.6
SlowDuration: 3
Range: 5.5
```

### Sniper Tower Upgrade Path

**TD_SniperTower_T1.asset:**
```
TowerName: "Marksman Tower"
PurchaseCost: 375
Damage: 150
Range: 14
AttackSpeed: 0.4
```

**TD_SniperTower_T2.asset:**
```
TowerName: "Assassin Tower"
PurchaseCost: 550
Damage: 250
Range: 16
AttackSpeed: 0.5
```

### Support Tower Upgrade Path

**TD_SupportTower_T1.asset:**
```
TowerName: "Battle Standard"
PurchaseCost: 325
BuffAmount: 0.35
BuffRadius: 4.5
```

**TD_SupportTower_T2.asset:**
```
TowerName: "War Shrine"
PurchaseCost: 500
BuffAmount: 0.5
BuffRadius: 5.5
```

### Upgrade Prefab Variants

Create visual variants for upgraded towers:

**Tier 1 Visual Enhancements:**
- Slightly larger scale (1.1x)
- Added decorative elements
- Improved material (more metallic/polished)
- Color accent changes

**Tier 2 Visual Enhancements:**
- Larger scale (1.2x)
- Significant detail additions
- Glowing elements or particles
- Premium material appearance

### Balance Considerations

| Tower | T0 Cost | T1 Cost | T2 Cost | DPS @ T2 |
|-------|---------|---------|---------|----------|
| Basic | 100 | 175 | 300 | 67.5 |
| Cannon | 150 | 250 | 400 | 30 (AOE) |
| Freeze | 100 | 175 | 300 | N/A |
| Sniper | 250 | 375 | 550 | 125 |
| Support | 200 | 325 | 500 | N/A |

## Testing and Acceptance Criteria

### Done When

- [ ] All base towers have UpgradesTo configured
- [ ] Tier 1 data assets created for all towers
- [ ] Tier 2 data assets created for all towers
- [ ] Upgrade prefabs have enhanced visuals
- [ ] Upgrade costs balanced
- [ ] Stats increase meaningfully per tier
- [ ] All upgrade chains work in-game

## Dependencies

- Issues 30-33: Base tower types
- Issue 40: Upgrade system
