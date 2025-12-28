## Context

The basic tower provides single-target damage. Now we need an Area of Effect (AOE) tower that deals splash damage to groups of enemies. This is essential for dealing with swarm waves.

**Builds upon:** Issues 6, 14-16 (Tower systems, Combat)

## Detailed Implementation Instructions

### AOE Tower Data

Create `TD_CannonTower.asset`:
```
TowerName: "Cannon Tower"
Description: "Fires explosive shells that damage all enemies in the blast radius."
Type: AOE
PurchaseCost: 150
SellValue: 75
Range: 5
AttackSpeed: 0.5 (slow)
Damage: 25
AOERadius: 2
DefaultPriority: First
ProjectileSpeed: 12
```

### AOE Projectile

Create `CannonProjectile` prefab:

```
CannonProjectile (Projectile.cs)
|-- Model (Sphere scaled 0.3)
+-- TrailRenderer (optional)
```

Configure:
- Larger than basic projectile
- Dark gray/metallic material
- Add TrailRenderer for visual effect

### AOE Visual Effect

The AOE damage needs visual feedback:

```csharp
// Add to Projectile.cs
[SerializeField] private GameObject _aoeExplosionPrefab;

private void DealAOEDamage()
{
    // Spawn explosion VFX
    if (_aoeExplosionPrefab != null)
    {
        GameObject explosion = Instantiate(_aoeExplosionPrefab, transform.position, Quaternion.identity);
        Destroy(explosion, 2f);
    }

    // Existing AOE damage code
    Collider[] hits = Physics.OverlapSphere(transform.position, _aoeRadius, LayerMask.GetMask("Enemy"));
    foreach (var hit in hits)
    {
        Enemy enemy = hit.GetComponentInParent<Enemy>();
        if (enemy != null && !enemy.IsDead)
        {
            enemy.TakeDamage(_damage);
        }
    }
}
```

### AOE Explosion VFX Prefab

Create `VFX_Explosion.prefab`:
- Particle System with:
  - Burst emission (20-30 particles)
  - Sphere shape
  - Size over lifetime (expand then shrink)
  - Color: Orange to dark gray
  - Lifetime: 0.5s
  - No looping

### Cannon Tower Prefab

Create `CannonTower` prefab:

```
CannonTower (Tower.cs, TowerTargeting, TowerAiming, TowerAttack)
|-- Base (Cube 2x0.5x2 - larger base)
|-- TurretPivot
|   +-- Cannon (Cylinder rotated, scaled 0.6x1.2x0.6)
|       +-- FirePoint
+-- RangeIndicator
```

Material: M_Tower_Cannon - dark bronze/brown color

### Update Tower Selection

Add CannonTower to TowerSelectionPanel available towers array.

## Testing and Acceptance Criteria

### Done When

- [ ] CannonTower data asset created
- [ ] CannonTower prefab with distinct visual
- [ ] Cannon fires slower than basic tower
- [ ] Projectile deals splash damage
- [ ] AOE radius damages all enemies in range
- [ ] Explosion VFX plays on impact
- [ ] Tower available in selection panel
- [ ] Balanced cost vs effectiveness

## Dependencies

- Issues 6, 14-16: Tower and combat systems
