## Context

The damage system needs proper integration to ensure enemies take damage correctly, armor is applied, and the health system works consistently. This issue formalizes the damage calculation and ensures all components work together.

**Builds upon:** Issues 9, 14-16 (Enemy, Combat systems)

## Detailed Implementation Instructions

### Damage Calculator Utility

Create `DamageCalculator.cs` in `_Project/Scripts/Runtime/Core/`:

```csharp
namespace TowerDefense.Core
{
    public static class DamageCalculator
    {
        public static int CalculateDamage(int baseDamage, int armor, float damageMultiplier = 1f)
        {
            // Apply multiplier first
            int modifiedDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);

            // Apply armor (flat reduction)
            int finalDamage = Mathf.Max(1, modifiedDamage - armor);

            return finalDamage;
        }

        public static int CalculateCriticalDamage(int baseDamage, float critMultiplier = 2f)
        {
            return Mathf.RoundToInt(baseDamage * critMultiplier);
        }

        public static bool RollCritical(float critChance)
        {
            return Random.value < critChance;
        }
    }
}
```

### Damage Info Struct

Create `DamageInfo.cs` in `_Project/Scripts/Runtime/Core/`:

```csharp
using UnityEngine;

namespace TowerDefense.Core
{
    public struct DamageInfo
    {
        public int Amount;
        public GameObject Source;
        public Vector3 HitPoint;
        public bool IsCritical;
        public DamageType Type;

        public DamageInfo(int amount, GameObject source = null, Vector3 hitPoint = default, bool isCritical = false, DamageType type = DamageType.Physical)
        {
            Amount = amount;
            Source = source;
            HitPoint = hitPoint;
            IsCritical = isCritical;
            Type = type;
        }
    }

    public enum DamageType
    {
        Physical,
        Magic,
        True // Ignores armor
    }
}
```

### Update Enemy TakeDamage

Update `Enemy.cs` to use damage info:

```csharp
public void TakeDamage(int damage)
{
    TakeDamage(new DamageInfo(damage));
}

public void TakeDamage(DamageInfo damageInfo)
{
    if (_isDead) return;

    int finalDamage;
    if (damageInfo.Type == DamageType.True)
    {
        finalDamage = damageInfo.Amount;
    }
    else
    {
        finalDamage = DamageCalculator.CalculateDamage(damageInfo.Amount, _enemyData.Armor);
    }

    _currentHealth -= finalDamage;
    OnHealthChanged?.Invoke(this, _currentHealth, _enemyData.MaxHealth);

    // Damage number event (for UI)
    OnDamageTaken?.Invoke(this, finalDamage, damageInfo.IsCritical);

    if (_currentHealth <= 0)
    {
        Die();
    }
}

public event System.Action<Enemy, int, bool> OnDamageTaken; // enemy, damage, isCritical
```

### Update Projectile to Pass DamageInfo

```csharp
private void DealSingleTargetDamage()
{
    if (_target != null && !_target.IsDead)
    {
        var info = new DamageInfo(_damage, gameObject, transform.position);
        _target.TakeDamage(info);
    }
}

private void DealAOEDamage()
{
    Collider[] hits = Physics.OverlapSphere(transform.position, _aoeRadius, LayerMask.GetMask("Enemy"));
    foreach (var hit in hits)
    {
        Enemy enemy = hit.GetComponentInParent<Enemy>();
        if (enemy != null && !enemy.IsDead)
        {
            var info = new DamageInfo(_damage, gameObject, transform.position);
            enemy.TakeDamage(info);
        }
    }
}
```

### Floating Damage Numbers

Create `FloatingDamageNumber.cs` in `_Project/Scripts/Runtime/UI/`:

```csharp
using UnityEngine;
using TMPro;

namespace TowerDefense.UI
{
    public class FloatingDamageNumber : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private float _floatSpeed = 1.5f;
        [SerializeField] private float _lifetime = 0.8f;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _criticalColor = Color.yellow;
        [SerializeField] private float _criticalScale = 1.5f;

        private float _timer;

        public void Initialize(int damage, bool isCritical)
        {
            _text.text = damage.ToString();
            _text.color = isCritical ? _criticalColor : _normalColor;
            transform.localScale = Vector3.one * (isCritical ? _criticalScale : 1f);
            _timer = 0f;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            transform.position += Vector3.up * _floatSpeed * Time.deltaTime;

            Color c = _text.color;
            c.a = 1f - (_timer / _lifetime);
            _text.color = c;

            if (_timer >= _lifetime)
                Destroy(gameObject);
        }
    }
}
```

### Damage Number Spawner

Create `DamageNumberSpawner.cs`:

```csharp
using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.UI
{
    public class DamageNumberSpawner : MonoBehaviour
    {
        public static DamageNumberSpawner Instance { get; private set; }

        [SerializeField] private FloatingDamageNumber _prefab;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void SpawnDamageNumber(Vector3 position, int damage, bool isCritical)
        {
            if (_prefab == null) return;

            var instance = Instantiate(_prefab, position + Vector3.up * 0.5f + Random.insideUnitSphere * 0.3f, Quaternion.identity);
            instance.Initialize(damage, isCritical);
        }
    }
}
```

## Testing and Acceptance Criteria

### Done When

- [ ] DamageCalculator correctly applies armor
- [ ] True damage bypasses armor
- [ ] Minimum damage is 1
- [ ] OnDamageTaken event fires with correct values
- [ ] Floating damage numbers appear on hit
- [ ] Critical hits show larger/colored numbers
- [ ] Damage numbers float up and fade

## Dependencies

- Issue 9: Enemy health system
- Issues 14-16: Combat systems
