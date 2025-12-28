## Context

Tower upgrades allow players to invest in existing towers rather than only placing new ones. This adds strategic depth: upgrade a well-placed tower vs. add more coverage. This issue implements the upgrade data structure and system.

**Builds upon:** Issue 6 (TowerData)

## Detailed Implementation Instructions

### Upgrade Data Structure

Update `TowerData.cs` for upgrade paths:

```csharp
[Header("Upgrade Configuration")]
public TowerData[] UpgradesTo;       // Available upgrade options
public TowerData UpgradesFrom;       // Previous tier (null if base)
public int UpgradeTier = 0;          // 0 = base, 1 = tier 1, etc.
```

### Tower Upgrade Manager

Create `TowerUpgradeManager.cs`:

```csharp
using UnityEngine;
using TowerDefense.Economy;
using TowerDefense.Core;

namespace TowerDefense.Towers
{
    public class TowerUpgradeManager : MonoBehaviour
    {
        public static TowerUpgradeManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool CanUpgrade(Tower tower, TowerData upgradeTo)
        {
            if (tower == null || upgradeTo == null) return false;
            if (!IsValidUpgrade(tower.Data, upgradeTo)) return false;

            return EconomyManager.Instance.CanAfford(upgradeTo.PurchaseCost);
        }

        public bool TryUpgrade(Tower tower, TowerData upgradeTo)
        {
            if (!CanUpgrade(tower, upgradeTo)) return false;

            // Spend currency (upgrade cost minus sell value of current)
            int upgradeCost = GetUpgradeCost(tower, upgradeTo);
            if (!EconomyManager.Instance.TrySpend(upgradeCost))
                return false;

            // Perform upgrade
            PerformUpgrade(tower, upgradeTo);
            return true;
        }

        public int GetUpgradeCost(Tower tower, TowerData upgradeTo)
        {
            // Full cost of new tower, or difference from current
            // Option 1: Full cost
            // return upgradeTo.PurchaseCost;

            // Option 2: Difference (more forgiving)
            return upgradeTo.PurchaseCost - tower.Data.SellValue;
        }

        private bool IsValidUpgrade(TowerData current, TowerData upgradeTo)
        {
            if (current.UpgradesTo == null) return false;

            foreach (var option in current.UpgradesTo)
            {
                if (option == upgradeTo) return true;
            }
            return false;
        }

        private void PerformUpgrade(Tower tower, TowerData newData)
        {
            Vector2Int gridPos = tower.GridPosition;
            Vector3 worldPos = tower.transform.position;
            Quaternion rotation = tower.transform.rotation;

            // Store targeting priority
            TargetingPriority priority = tower.CurrentPriority;

            // Destroy old tower
            GridManager.Instance?.FreeCell(gridPos);
            Destroy(tower.gameObject);

            // Create new tower
            GameObject newTowerObj = Instantiate(newData.Prefab, worldPos, rotation);
            Tower newTower = newTowerObj.GetComponent<Tower>();
            newTower.Initialize(newData, gridPos);
            newTower.CurrentPriority = priority;

            GridManager.Instance?.TryOccupyCell(gridPos, newTowerObj);

            Debug.Log($"Upgraded to {newData.TowerName}");
        }

        public TowerData[] GetAvailableUpgrades(Tower tower)
        {
            return tower?.Data?.UpgradesTo ?? new TowerData[0];
        }
    }
}
```

### Create Upgrade Tower Data

For each base tower, create upgraded versions:

**Basic Tower Upgrades:**
- `TD_BasicTower_T2.asset`:
  - TowerName: "Improved Arrow Tower"
  - Damage: 25 (vs 15)
  - AttackSpeed: 1.25
  - Range: 7
  - PurchaseCost: 150
  - UpgradeTier: 1
  - UpgradesFrom: TD_BasicTower

- `TD_BasicTower_T3.asset`:
  - TowerName: "Master Archer"
  - Damage: 40
  - AttackSpeed: 1.5
  - Range: 8
  - PurchaseCost: 250
  - UpgradeTier: 2
  - UpgradesFrom: TD_BasicTower_T2

**Update base TD_BasicTower.asset:**
```
UpgradesTo: [TD_BasicTower_T2]
UpgradeTier: 0
```

**Update TD_BasicTower_T2.asset:**
```
UpgradesTo: [TD_BasicTower_T3]
```

### Upgrade Prefab Variants

Create upgraded prefabs with enhanced visuals:
- Larger/more detailed model
- Better materials (shinier, more colorful)
- Additional decorative elements

## Testing and Acceptance Criteria

### Done When

- [ ] TowerData supports upgrade references
- [ ] TowerUpgradeManager handles upgrade logic
- [ ] CanUpgrade validates correctly
- [ ] Currency spent on upgrade
- [ ] Old tower replaced with new
- [ ] Grid position maintained
- [ ] Targeting priority preserved
- [ ] At least 3 tiers for basic tower

## Dependencies

- Issue 6: TowerData
