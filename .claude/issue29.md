## Context

Wave difficulty should escalate appropriately. This issue creates additional wave data for a complete level experience (10 waves) with proper difficulty curves and introduces enemy variety through wave composition.

**Builds upon:** Issue 25-26 (Wave Data, Wave Manager)

## Detailed Implementation Instructions

### Difficulty Scaling Principles

**Early Waves (1-3):**
- Basic enemies only
- Low count (5-10)
- Slow spawn rate
- Learn mechanics

**Mid Waves (4-6):**
- Introduce fast enemies
- Medium count (10-20)
- Faster spawn rate
- Mixed compositions

**Late Waves (7-9):**
- All enemy types
- High count (20-30)
- Multiple spawn groups
- Strategic compositions

**Final Wave (10):**
- Boss or large swarm
- Everything at once
- Maximum challenge

### Wave Data Configurations

Create wave assets in `_Project/ScriptableObjects/WaveData/`:

**WD_Wave01.asset - Tutorial**
```
WaveName: "First Steps"
SpawnGroups:
- Group 1: BasicEnemy x 5, interval 1.5s
CompletionBonus: 50
```

**WD_Wave02.asset - Warm Up**
```
WaveName: "Warming Up"
SpawnGroups:
- Group 1: BasicEnemy x 8, interval 1.0s
CompletionBonus: 60
```

**WD_Wave03.asset - Getting Started**
```
WaveName: "Getting Started"
SpawnGroups:
- Group 1: BasicEnemy x 6, interval 0.8s
- Group 2: BasicEnemy x 6, interval 0.8s (2s delay)
CompletionBonus: 75
```

**WD_Wave04.asset - Quick Ones**
```
WaveName: "Quick Ones"
SpawnGroups:
- Group 1: BasicEnemy x 5
- Group 2: FastEnemy x 3, interval 0.5s (when implemented)
CompletionBonus: 90
WaveMessage: "Fast enemies incoming!"
```

**WD_Wave05.asset - Mix It Up**
```
WaveName: "Mix It Up"
SpawnGroups:
- Group 1: BasicEnemy x 8
- Group 2: FastEnemy x 5
- Group 3: BasicEnemy x 5
CompletionBonus: 100
```

**WD_Wave06.asset - Heavy Hitters**
```
WaveName: "Heavy Hitters"
SpawnGroups:
- Group 1: TankEnemy x 3, interval 2s (when implemented)
- Group 2: BasicEnemy x 10
CompletionBonus: 120
WaveMessage: "Armored enemies approaching!"
```

**WD_Wave07.asset - The Swarm**
```
WaveName: "The Swarm"
SpawnGroups:
- Group 1: BasicEnemy x 15, interval 0.3s
- Group 2: FastEnemy x 10, interval 0.3s
CompletionBonus: 140
```

**WD_Wave08.asset - Combined Arms**
```
WaveName: "Combined Arms"
SpawnGroups:
- Group 1: TankEnemy x 2
- Group 2: BasicEnemy x 10
- Group 3: FastEnemy x 8
- Group 4: TankEnemy x 2
CompletionBonus: 160
```

**WD_Wave09.asset - Final Push**
```
WaveName: "Final Push"
SpawnGroups:
- Group 1: All enemy types mixed x 20
- Group 2: FastEnemy x 15
CompletionBonus: 180
WaveMessage: "Prepare for the final wave!"
```

**WD_Wave10.asset - The Boss**
```
WaveName: "The Boss"
IsBossWave: true
SpawnGroups:
- Group 1: BasicEnemy x 5 (guards)
- Group 2: BossEnemy x 1 (when implemented)
- Group 3: BasicEnemy x 10 (reinforcements)
CompletionBonus: 300
WaveMessage: "BOSS INCOMING!"
```

### Update Level Config

Update `LC_TestLevel.asset`:
```
LevelName: "Forest Path"
Waves: [WD_Wave01 through WD_Wave10]
TimeBetweenWaves: 15
AutoStartFirstWave: false
AutoStartNextWave: true
```

### Wave Preview Component (Optional)

Create `WavePreview.cs`:

```csharp
using UnityEngine;
using TMPro;
using TowerDefense.Waves;

namespace TowerDefense.UI
{
    public class WavePreview : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _previewText;

        public void ShowWavePreview(WaveData wave)
        {
            if (wave == null) return;

            string preview = $"<b>{wave.WaveName}</b>\n";
            preview += $"Enemies: {wave.TotalEnemyCount}\n";

            if (!string.IsNullOrEmpty(wave.WaveMessage))
            {
                preview += $"\n<color=yellow>{wave.WaveMessage}</color>";
            }

            _previewText.text = preview;
        }
    }
}
```

## Testing and Acceptance Criteria

### Done When

- [ ] 10 wave data assets created
- [ ] Difficulty escalates appropriately
- [ ] Wave bonuses increase with difficulty
- [ ] Boss wave configured
- [ ] Level config includes all waves
- [ ] Early waves completable with basic tower
- [ ] Later waves require strategy
- [ ] Wave messages display correctly

## Dependencies

- Issues 25-26: Wave system
- Note: Fast/Tank/Boss enemies created in M8

## Notes

- Wave data references enemy types not yet created
- Use BasicEnemy as placeholder until M8
- Adjust counts after playtesting
- Boss wave may need rebalancing
