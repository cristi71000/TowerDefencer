## Context

Sniper towers provide high single-target damage against tough enemies like tanks and bosses. They have long range but slow fire rate, requiring strategic placement.

**Builds upon:** Issues 6, 14-16 (Tower systems, Combat)

## Detailed Implementation Instructions

### Sniper Tower Data

Create `TD_SniperTower.asset`:
```
TowerName: "Sniper Tower"
Description: "Long-range tower with devastating single-target damage. Excellent against armored enemies."
Type: Sniper
PurchaseCost: 250
SellValue: 125
Range: 12 (longest range)
AttackSpeed: 0.33 (very slow - 3 second cooldown)
Damage: 100 (high damage)
AOERadius: 0 (single target)
DefaultPriority: Strongest
ProjectileSpeed: 50 (very fast/instant)
```

### Sniper Projectile (Instant Hit)

For snipers, use hitscan instead of projectile:

Create `HitscanAttack.cs`:

```csharp
using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Towers
{
    public class HitscanAttack : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private LineRenderer _bulletTrail;
        [SerializeField] private float _trailDuration = 0.1f;
        [SerializeField] private GameObject _impactVFX;

        private Tower _tower;
        private TowerTargeting _targeting;

        private void Awake()
        {
            _tower = GetComponent<Tower>();
            _targeting = GetComponent<TowerTargeting>();

            if (_bulletTrail != null)
                _bulletTrail.enabled = false;
        }

        public void FireHitscan()
        {
            if (!_targeting.HasTarget) return;

            Enemy target = _targeting.CurrentTarget;
            Transform firePoint = _tower.GetFirePoint();

            // Deal damage instantly
            target.TakeDamage(_tower.Data.Damage);

            // Visual feedback
            ShowBulletTrail(firePoint.position, target.TargetPoint.position);
            ShowImpact(target.TargetPoint.position);
        }

        private void ShowBulletTrail(Vector3 start, Vector3 end)
        {
            if (_bulletTrail == null) return;

            _bulletTrail.enabled = true;
            _bulletTrail.SetPosition(0, start);
            _bulletTrail.SetPosition(1, end);

            StartCoroutine(HideTrailAfterDelay());
        }

        private System.Collections.IEnumerator HideTrailAfterDelay()
        {
            yield return new WaitForSeconds(_trailDuration);
            _bulletTrail.enabled = false;
        }

        private void ShowImpact(Vector3 position)
        {
            if (_impactVFX == null) return;

            GameObject impact = Instantiate(_impactVFX, position, Quaternion.identity);
            Destroy(impact, 1f);
        }
    }
}
```

### Update TowerAttack for Hitscan

```csharp
// In TowerAttack.cs
[SerializeField] private bool _useHitscan = false;
private HitscanAttack _hitscanAttack;

private void Awake()
{
    // ... existing
    _hitscanAttack = GetComponent<HitscanAttack>();
}

private void FireProjectile(Enemy target)
{
    if (_useHitscan && _hitscanAttack != null)
    {
        _hitscanAttack.FireHitscan();
        return;
    }
    // ... existing projectile code
}
```

### Sniper Tower Prefab

Create `SniperTower` prefab:

```
SniperTower (Tower.cs, TowerTargeting, TowerAiming, TowerAttack, HitscanAttack)
|-- Base (Tall thin platform)
|-- TurretPivot
|   +-- SniperBarrel (Long thin cylinder)
|       |-- FirePoint
|       +-- BulletTrail (LineRenderer)
+-- RangeIndicator
```

### Sniper Visual Design

- Tall, elevated design
- Long barrel
- Scope detail (small cylinder on top)
- Dark/tactical color scheme

### Bullet Trail Setup

LineRenderer settings:
- Width: 0.05
- Material: Additive/Unlit yellow
- Positions: 2 (start, end)

### Impact VFX

Create `VFX_SniperImpact.prefab`:
- Quick spark burst
- Small radius
- Yellow/white color

## Testing and Acceptance Criteria

### Done When

- [ ] SniperTower data asset created
- [ ] SniperTower prefab with tall design
- [ ] Longest range of all towers
- [ ] Hitscan instant damage
- [ ] Bullet trail visual
- [ ] Impact VFX on hit
- [ ] Default targets strongest enemy
- [ ] Balanced damage vs fire rate

## Dependencies

- Issues 6, 14-16: Tower and combat systems
