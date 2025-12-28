## Context

With all enemy types created, wave data needs updating to incorporate enemy variety. This issue updates the wave configurations to use the new enemy types strategically.

**Builds upon:** Issues 25, 29, 35-38 (Waves, Enemy variety)

## Detailed Implementation Instructions

### Update Wave Data Files

Revise wave data to use all enemy types:

**WD_Wave01.asset - Tutorial**
```
SpawnGroups:
- Group 1: BasicEnemy x 5, interval 1.5s
```

**WD_Wave02.asset - Warm Up**
```
SpawnGroups:
- Group 1: BasicEnemy x 8, interval 1.0s
```

**WD_Wave03.asset - Speed Test**
```
WaveMessage: "Fast enemies incoming!"
SpawnGroups:
- Group 1: BasicEnemy x 4, interval 1.0s
- Group 2: FastEnemy x 3, interval 0.5s
```

**WD_Wave04.asset - Mixed Threats**
```
SpawnGroups:
- Group 1: BasicEnemy x 5
- Group 2: FastEnemy x 4
- Group 3: BasicEnemy x 5
```

**WD_Wave05.asset - Air Raid**
```
WaveMessage: "Enemies from the sky!"
SpawnGroups:
- Group 1: BasicEnemy x 6
- Group 2: FlyingEnemy x 4
```

**WD_Wave06.asset - Heavy Armor**
```
WaveMessage: "Armored units approaching!"
SpawnGroups:
- Group 1: TankEnemy x 2, interval 3s
- Group 2: BasicEnemy x 8
```

**WD_Wave07.asset - The Swarm**
```
SpawnGroups:
- Group 1: FastEnemy x 15, interval 0.3s
- Group 2: BasicEnemy x 10, interval 0.5s
```

**WD_Wave08.asset - Combined Arms**
```
SpawnGroups:
- Group 1: TankEnemy x 1
- Group 2: BasicEnemy x 5
- Group 3: FlyingEnemy x 3
- Group 4: FastEnemy x 5
- Group 5: TankEnemy x 1
```

**WD_Wave09.asset - The Gauntlet**
```
WaveMessage: "Final stretch!"
SpawnGroups:
- Group 1: FlyingEnemy x 5
- Group 2: TankEnemy x 2
- Group 3: FastEnemy x 10
- Group 4: BasicEnemy x 15
```

**WD_Wave10.asset - Boss Wave**
```
WaveMessage: "THE GOLEM APPROACHES!"
IsBossWave: true
SpawnGroups:
- Group 1: BasicEnemy x 4 (guards)
- Group 2: BossEnemy x 1
- Group 3: FastEnemy x 6 (reinforcements, delayed)
- Group 4: FlyingEnemy x 4 (more reinforcements)
```

### Wave Balance Considerations

| Wave | Basic | Fast | Tank | Flying | Boss | Total Enemies |
|------|-------|------|------|--------|------|---------------|
| 1    | 5     | 0    | 0    | 0      | 0    | 5             |
| 2    | 8     | 0    | 0    | 0      | 0    | 8             |
| 3    | 4     | 3    | 0    | 0      | 0    | 7             |
| 4    | 10    | 4    | 0    | 0      | 0    | 14            |
| 5    | 6     | 0    | 0    | 4      | 0    | 10            |
| 6    | 8     | 0    | 2    | 0      | 0    | 10            |
| 7    | 10    | 15   | 0    | 0      | 0    | 25            |
| 8    | 5     | 5    | 2    | 3      | 0    | 15            |
| 9    | 15    | 10   | 2    | 5      | 0    | 32            |
| 10   | 4     | 6    | 0    | 4      | 1    | 15            |

### Prewarm Enemy Pools

Create `WavePoolPrewarmer.cs`:

```csharp
using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Waves
{
    public class WavePoolPrewarmer : MonoBehaviour
    {
        [SerializeField] private LevelWaveConfig _levelConfig;

        private void Start()
        {
            PrewarmPools();
        }

        private void PrewarmPools()
        {
            if (_levelConfig == null) return;

            // Analyze all waves to determine pool sizes
            var enemyCounts = new System.Collections.Generic.Dictionary<EnemyData, int>();

            foreach (var wave in _levelConfig.Waves)
            {
                foreach (var group in wave.SpawnGroups)
                {
                    foreach (var spawn in group.Enemies)
                    {
                        if (spawn.EnemyType == null) continue;

                        if (!enemyCounts.ContainsKey(spawn.EnemyType))
                            enemyCounts[spawn.EnemyType] = 0;

                        enemyCounts[spawn.EnemyType] = Mathf.Max(
                            enemyCounts[spawn.EnemyType],
                            spawn.Count
                        );
                    }
                }
            }

            // Prewarm each pool
            foreach (var kvp in enemyCounts)
            {
                EnemyPoolManager.Instance?.PrewarmPool(kvp.Key, kvp.Value + 5);
            }
        }
    }
}
```

## Testing and Acceptance Criteria

### Manual Test Steps

1. Play through all 10 waves
2. Verify each wave spawns correct enemy types
3. Verify wave progression feels challenging but fair
4. Verify boss wave is appropriately difficult
5. Check that pools handle spawn volumes

### Done When

- [ ] All wave data updated with enemy variety
- [ ] Fast enemies appear in waves 3+
- [ ] Flying enemies appear in waves 5+
- [ ] Tank enemies appear in waves 6+
- [ ] Boss appears in wave 10
- [ ] Wave messages display for new enemy types
- [ ] Pool prewarming prevents spawn hitches
- [ ] Difficulty curve feels appropriate

## Dependencies

- Issues 25, 29: Wave system
- Issues 35-38: All enemy types
