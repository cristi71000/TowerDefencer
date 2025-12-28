## Context

Waves define when and what enemies spawn. This issue creates the WaveData ScriptableObject that defines wave composition, spawn timing, and difficulty progression. This is the data foundation for the wave spawning system.

**Builds upon:** Issue 9 (EnemyData)

## Detailed Implementation Instructions

### Wave Data ScriptableObject

Create `WaveData.cs` in `_Project/Scripts/Runtime/Waves/`:

```csharp
using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Waves
{
    [System.Serializable]
    public class EnemySpawnInfo
    {
        public EnemyData EnemyType;
        public int Count;
        public float SpawnInterval = 0.5f;
        public float DelayBeforeGroup = 0f;
    }

    [System.Serializable]
    public class SpawnGroup
    {
        public string GroupName;
        public EnemySpawnInfo[] Enemies;
        public float DelayAfterGroup = 1f;
    }

    [CreateAssetMenu(fileName = "NewWaveData", menuName = "TD/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [Header("Wave Info")]
        public string WaveName;
        public int WaveNumber;

        [Header("Spawn Configuration")]
        public SpawnGroup[] SpawnGroups;

        [Header("Timing")]
        public float PreWaveDelay = 3f;
        public float PostWaveDelay = 5f;

        [Header("Rewards")]
        public int CompletionBonus = 50;

        [Header("Special")]
        public bool IsBossWave = false;
        public string WaveMessage;

        public int TotalEnemyCount
        {
            get
            {
                int total = 0;
                foreach (var group in SpawnGroups)
                {
                    foreach (var enemy in group.Enemies)
                    {
                        total += enemy.Count;
                    }
                }
                return total;
            }
        }
    }
}
```

### Level Wave Configuration

Create `LevelWaveConfig.cs`:

```csharp
using UnityEngine;

namespace TowerDefense.Waves
{
    [CreateAssetMenu(fileName = "NewLevelWaves", menuName = "TD/Level Wave Config")]
    public class LevelWaveConfig : ScriptableObject
    {
        public string LevelName;
        public WaveData[] Waves;

        [Header("Settings")]
        public float TimeBetweenWaves = 10f;
        public bool AutoStartFirstWave = false;
        public bool AutoStartNextWave = true;

        public int TotalWaves => Waves?.Length ?? 0;
    }
}
```

### Create Sample Wave Data

Create wave data assets in `_Project/ScriptableObjects/WaveData/`:

**Wave 1 - Tutorial:**
```
WD_Wave01.asset
- WaveName: "First Wave"
- WaveNumber: 1
- PreWaveDelay: 5
- SpawnGroups:
  - Group 1:
    - BasicEnemy x 5, interval 1.0s
- CompletionBonus: 50
```

**Wave 2 - Building Up:**
```
WD_Wave02.asset
- WaveName: "Building Up"
- WaveNumber: 2
- SpawnGroups:
  - Group 1: BasicEnemy x 8, interval 0.8s
- CompletionBonus: 60
```

**Wave 3 - First Challenge:**
```
WD_Wave03.asset
- WaveName: "First Challenge"
- WaveNumber: 3
- SpawnGroups:
  - Group 1: BasicEnemy x 5, interval 0.8s
  - Group 2 (after 2s delay): BasicEnemy x 5, interval 0.5s
- CompletionBonus: 75
```

### Create Level Configuration

Create `LC_TestLevel.asset`:
```
- LevelName: "Test Level"
- Waves: [WD_Wave01, WD_Wave02, WD_Wave03]
- TimeBetweenWaves: 10
- AutoStartFirstWave: false
- AutoStartNextWave: true
```

## Testing and Acceptance Criteria

### Done When

- [ ] WaveData ScriptableObject created
- [ ] EnemySpawnInfo struct with type, count, interval
- [ ] SpawnGroup supports multiple enemy types
- [ ] LevelWaveConfig holds array of waves
- [ ] TotalEnemyCount calculated correctly
- [ ] Sample wave data assets created
- [ ] Level config asset created

## Dependencies

- Issue 9: EnemyData
