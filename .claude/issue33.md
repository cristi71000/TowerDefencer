## Context

Support towers buff nearby towers instead of attacking enemies directly. This adds strategic depth by rewarding optimal tower placement and synergies.

**Builds upon:** Issues 6, 14 (Tower systems)

## Detailed Implementation Instructions

### Support Tower Data

Create `TD_SupportTower.asset`:
```
TowerName: "Support Tower"
Description: "Boosts the damage of nearby towers. Does not attack enemies."
Type: Support
PurchaseCost: 200
SellValue: 100
Range: 0 (no enemy targeting)
AttackSpeed: 0
Damage: 0
BuffRadius: 4
BuffAmount: 0.25 (25% damage boost)
DefaultPriority: First (unused)
```

### Tower Buff System

Create `TowerBuff.cs`:

```csharp
namespace TowerDefense.Towers
{
    [System.Serializable]
    public class TowerBuff
    {
        public BuffType Type;
        public float Amount;
        public Tower Source;

        public TowerBuff(BuffType type, float amount, Tower source)
        {
            Type = type;
            Amount = amount;
            Source = source;
        }
    }

    public enum BuffType
    {
        DamageMultiplier,
        AttackSpeedMultiplier,
        RangeMultiplier
    }
}
```

### Buff Receiver Component

Create `TowerBuffReceiver.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Towers
{
    public class TowerBuffReceiver : MonoBehaviour
    {
        private List<TowerBuff> _activeBuffs = new List<TowerBuff>();
        private Tower _tower;

        public float DamageMultiplier { get; private set; } = 1f;
        public float AttackSpeedMultiplier { get; private set; } = 1f;
        public float RangeMultiplier { get; private set; } = 1f;

        public event System.Action OnBuffsChanged;

        private void Awake()
        {
            _tower = GetComponent<Tower>();
        }

        public void AddBuff(TowerBuff buff)
        {
            // Check for duplicate source
            _activeBuffs.RemoveAll(b => b.Source == buff.Source && b.Type == buff.Type);
            _activeBuffs.Add(buff);
            RecalculateMultipliers();
        }

        public void RemoveBuff(Tower source)
        {
            _activeBuffs.RemoveAll(b => b.Source == source);
            RecalculateMultipliers();
        }

        public void ClearAllBuffs()
        {
            _activeBuffs.Clear();
            RecalculateMultipliers();
        }

        private void RecalculateMultipliers()
        {
            DamageMultiplier = 1f;
            AttackSpeedMultiplier = 1f;
            RangeMultiplier = 1f;

            foreach (var buff in _activeBuffs)
            {
                switch (buff.Type)
                {
                    case BuffType.DamageMultiplier:
                        DamageMultiplier += buff.Amount;
                        break;
                    case BuffType.AttackSpeedMultiplier:
                        AttackSpeedMultiplier += buff.Amount;
                        break;
                    case BuffType.RangeMultiplier:
                        RangeMultiplier += buff.Amount;
                        break;
                }
            }

            OnBuffsChanged?.Invoke();
        }

        public bool HasBuffFrom(Tower source)
        {
            return _activeBuffs.Exists(b => b.Source == source);
        }
    }
}
```

### Support Tower Aura

Create `SupportTowerAura.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Towers
{
    public class SupportTowerAura : MonoBehaviour
    {
        [SerializeField] private float _updateInterval = 0.5f;
        [SerializeField] private LayerMask _towerLayer;

        private Tower _tower;
        private float _buffRadius;
        private float _buffAmount;
        private float _updateTimer;
        private List<TowerBuffReceiver> _buffedTowers = new List<TowerBuffReceiver>();

        private void Awake()
        {
            _tower = GetComponent<Tower>();
        }

        private void Start()
        {
            if (_tower.Data != null)
            {
                _buffRadius = _tower.Data.BuffRadius;
                _buffAmount = _tower.Data.BuffAmount;
            }
        }

        private void Update()
        {
            _updateTimer += Time.deltaTime;
            if (_updateTimer >= _updateInterval)
            {
                _updateTimer = 0f;
                UpdateBuffedTowers();
            }
        }

        private void OnDisable()
        {
            RemoveAllBuffs();
        }

        private void OnDestroy()
        {
            RemoveAllBuffs();
        }

        private void UpdateBuffedTowers()
        {
            // Find towers in range
            Collider[] hits = Physics.OverlapSphere(transform.position, _buffRadius, _towerLayer);
            HashSet<TowerBuffReceiver> currentInRange = new HashSet<TowerBuffReceiver>();

            foreach (var hit in hits)
            {
                Tower tower = hit.GetComponentInParent<Tower>();
                if (tower != null && tower != _tower)
                {
                    TowerBuffReceiver receiver = tower.GetComponent<TowerBuffReceiver>();
                    if (receiver != null)
                    {
                        currentInRange.Add(receiver);

                        // Apply buff if not already applied
                        if (!_buffedTowers.Contains(receiver))
                        {
                            var buff = new TowerBuff(BuffType.DamageMultiplier, _buffAmount, _tower);
                            receiver.AddBuff(buff);
                            _buffedTowers.Add(receiver);
                        }
                    }
                }
            }

            // Remove buffs from towers no longer in range
            for (int i = _buffedTowers.Count - 1; i >= 0; i--)
            {
                if (!currentInRange.Contains(_buffedTowers[i]))
                {
                    _buffedTowers[i].RemoveBuff(_tower);
                    _buffedTowers.RemoveAt(i);
                }
            }
        }

        private void RemoveAllBuffs()
        {
            foreach (var receiver in _buffedTowers)
            {
                if (receiver != null)
                    receiver.RemoveBuff(_tower);
            }
            _buffedTowers.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, _buffRadius > 0 ? _buffRadius : 4f);
        }
    }
}
```

### Update TowerAttack to Use Buffs

```csharp
// In TowerAttack.cs, when calculating damage:
private int GetBuffedDamage()
{
    int baseDamage = _tower.Data.Damage;
    TowerBuffReceiver receiver = GetComponent<TowerBuffReceiver>();

    if (receiver != null)
    {
        return Mathf.RoundToInt(baseDamage * receiver.DamageMultiplier);
    }
    return baseDamage;
}
```

### Support Tower Prefab

Create `SupportTower` prefab:

```
SupportTower (Tower.cs, SupportTowerAura)
|-- Base (Circular platform)
|-- Beacon (Glowing pillar/crystal)
|   +-- AuraVFX (expanding ring particles)
+-- BuffRangeIndicator (shows buff radius)
```

### Buff Visual on Affected Towers

Add visual indicator when tower is buffed:
- Glowing outline
- Particle effect
- Color tint

## Testing and Acceptance Criteria

### Done When

- [ ] SupportTower data asset created
- [ ] SupportTower prefab with beacon visual
- [ ] Buff system applies to nearby towers
- [ ] Damage multiplier calculated correctly
- [ ] Buffs removed when support tower destroyed
- [ ] Buffs removed when tower leaves range
- [ ] Visual feedback on buffed towers
- [ ] Multiple support towers stack

## Dependencies

- Issues 6, 14: Tower systems
