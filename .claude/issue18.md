## Context

Slow towers need to apply debuffs to enemies. This issue implements a status effect system that allows towers to apply effects like slow, damage over time, or other debuffs to enemies.

**Builds upon:** Issues 9, 14-16 (Enemy, Combat systems)

## Detailed Implementation Instructions

### Status Effect Base Class

Create `StatusEffect.cs` in `_Project/Scripts/Runtime/Core/`:

```csharp
using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Core
{
    public abstract class StatusEffect
    {
        public float Duration { get; protected set; }
        public float RemainingTime { get; protected set; }
        public bool IsExpired => RemainingTime <= 0;
        public GameObject Source { get; protected set; }

        public StatusEffect(float duration, GameObject source = null)
        {
            Duration = duration;
            RemainingTime = duration;
            Source = source;
        }

        public virtual void Apply(Enemy enemy) { }
        public virtual void Update(Enemy enemy, float deltaTime)
        {
            RemainingTime -= deltaTime;
        }
        public virtual void Remove(Enemy enemy) { }
        public virtual bool CanStack => false;
        public virtual void Refresh() => RemainingTime = Duration;
    }
}
```

### Slow Effect

Create `SlowEffect.cs`:

```csharp
using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Core
{
    public class SlowEffect : StatusEffect
    {
        public float SlowAmount { get; private set; }

        public SlowEffect(float slowAmount, float duration, GameObject source = null)
            : base(duration, source)
        {
            SlowAmount = Mathf.Clamp01(slowAmount);
        }

        public override void Apply(Enemy enemy)
        {
            enemy.ApplySlow(SlowAmount, Duration);
        }

        public override void Remove(Enemy enemy)
        {
            // Enemy handles slow removal internally
        }
    }
}
```

### Damage Over Time Effect

Create `DamageOverTimeEffect.cs`:

```csharp
using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Core
{
    public class DamageOverTimeEffect : StatusEffect
    {
        public int DamagePerTick { get; private set; }
        public float TickInterval { get; private set; }
        private float _tickTimer;

        public DamageOverTimeEffect(int damagePerTick, float tickInterval, float duration, GameObject source = null)
            : base(duration, source)
        {
            DamagePerTick = damagePerTick;
            TickInterval = tickInterval;
            _tickTimer = 0f;
        }

        public override void Update(Enemy enemy, float deltaTime)
        {
            base.Update(enemy, deltaTime);

            _tickTimer += deltaTime;
            if (_tickTimer >= TickInterval)
            {
                _tickTimer -= TickInterval;
                enemy.TakeDamage(DamagePerTick);
            }
        }

        public override bool CanStack => true;
    }
}
```

### Status Effect Manager Component

Create `StatusEffectManager.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Core
{
    public class StatusEffectManager : MonoBehaviour
    {
        private Enemy _enemy;
        private List<StatusEffect> _activeEffects = new List<StatusEffect>();
        private List<StatusEffect> _effectsToRemove = new List<StatusEffect>();

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        private void Update()
        {
            if (_enemy.IsDead)
            {
                ClearAllEffects();
                return;
            }

            foreach (var effect in _activeEffects)
            {
                effect.Update(_enemy, Time.deltaTime);

                if (effect.IsExpired)
                {
                    _effectsToRemove.Add(effect);
                }
            }

            foreach (var effect in _effectsToRemove)
            {
                effect.Remove(_enemy);
                _activeEffects.Remove(effect);
            }
            _effectsToRemove.Clear();
        }

        public void AddEffect(StatusEffect effect)
        {
            // Check for existing effect of same type
            var existing = _activeEffects.Find(e => e.GetType() == effect.GetType());

            if (existing != null)
            {
                if (effect.CanStack)
                {
                    _activeEffects.Add(effect);
                    effect.Apply(_enemy);
                }
                else
                {
                    existing.Refresh();
                }
            }
            else
            {
                _activeEffects.Add(effect);
                effect.Apply(_enemy);
            }
        }

        public void RemoveEffect<T>() where T : StatusEffect
        {
            var effect = _activeEffects.Find(e => e is T);
            if (effect != null)
            {
                effect.Remove(_enemy);
                _activeEffects.Remove(effect);
            }
        }

        public bool HasEffect<T>() where T : StatusEffect
        {
            return _activeEffects.Exists(e => e is T);
        }

        public void ClearAllEffects()
        {
            foreach (var effect in _activeEffects)
            {
                effect.Remove(_enemy);
            }
            _activeEffects.Clear();
        }
    }
}
```

### Update Enemy Prefab

Add StatusEffectManager component to BasicEnemy prefab.

### Update Projectile for Status Effects

```csharp
// Add to Projectile.cs
private float _slowAmount;
private float _slowDuration;

public void Initialize(Enemy target, int damage, float speed, float aoeRadius = 0f, float slowAmount = 0f, float slowDuration = 0f)
{
    // ... existing initialization
    _slowAmount = slowAmount;
    _slowDuration = slowDuration;
}

private void ApplyEffects(Enemy enemy)
{
    if (_slowAmount > 0 && _slowDuration > 0)
    {
        var slowEffect = new SlowEffect(_slowAmount, _slowDuration, gameObject);
        enemy.GetComponent<StatusEffectManager>()?.AddEffect(slowEffect);
    }
}
```

## Testing and Acceptance Criteria

### Done When

- [ ] StatusEffect base class implemented
- [ ] SlowEffect reduces enemy speed
- [ ] DamageOverTimeEffect deals periodic damage
- [ ] Effects expire after duration
- [ ] Non-stackable effects refresh instead of duplicating
- [ ] Stackable effects accumulate
- [ ] Effects cleared on enemy death

## Dependencies

- Issue 9: Enemy systems
- Issues 14-16: Combat systems
