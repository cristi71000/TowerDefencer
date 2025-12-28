## Context

The combat loop needs integration testing to ensure all systems work together: towers detect enemies, aim, fire projectiles, projectiles hit and damage enemies, enemies die and reward currency. This issue focuses on end-to-end testing and fixes.

**Builds upon:** Issues 14-18 (All combat systems)

## Detailed Implementation Instructions

### Combat Test Scene Setup

Create a dedicated test setup in Main.unity:

1. Ensure all managers are present:
   - GameManager
   - GridManager
   - TowerPlacementManager
   - TowerSelectionManager
   - EnemySpawner
   - EnemyPoolManager
   - ProjectilePoolManager
   - DamageNumberSpawner (optional)

2. Add debug controls for testing:

### Debug Combat Controller

Create `DebugCombatController.cs`:

```csharp
using UnityEngine;
using TowerDefense.Enemies;
using TowerDefense.Towers;
using TowerDefense.Core;

namespace TowerDefense.Debug
{
    public class DebugCombatController : MonoBehaviour
    {
        [Header("Test Data")]
        [SerializeField] private TowerData _testTowerData;
        [SerializeField] private EnemyData _testEnemyData;

        [Header("Spawn Settings")]
        [SerializeField] private Vector2Int _towerGridPosition = new Vector2Int(10, 10);
        [SerializeField] private int _enemyCount = 5;
        [SerializeField] private float _spawnInterval = 1f;

        [Header("Keys")]
        [SerializeField] private KeyCode _spawnTowerKey = KeyCode.T;
        [SerializeField] private KeyCode _spawnEnemyKey = KeyCode.E;
        [SerializeField] private KeyCode _spawnWaveKey = KeyCode.W;
        [SerializeField] private KeyCode _killAllKey = KeyCode.K;

        private void Update()
        {
            if (Input.GetKeyDown(_spawnTowerKey))
            {
                SpawnTestTower();
            }

            if (Input.GetKeyDown(_spawnEnemyKey))
            {
                SpawnTestEnemy();
            }

            if (Input.GetKeyDown(_spawnWaveKey))
            {
                StartCoroutine(SpawnTestWave());
            }

            if (Input.GetKeyDown(_killAllKey))
            {
                KillAllEnemies();
            }
        }

        private void SpawnTestTower()
        {
            if (_testTowerData == null || GridManager.Instance == null) return;

            Vector3 worldPos = GridManager.Instance.GridToWorldPosition(_towerGridPosition);

            if (!GridManager.Instance.CanPlaceAt(_towerGridPosition))
            {
                UnityEngine.Debug.Log("Cannot place tower at this position");
                return;
            }

            GameObject tower = Instantiate(_testTowerData.Prefab, worldPos, Quaternion.identity);
            tower.GetComponent<Tower>()?.Initialize(_testTowerData, _towerGridPosition);
            GridManager.Instance.TryOccupyCell(_towerGridPosition, tower);

            UnityEngine.Debug.Log($"Spawned test tower at {_towerGridPosition}");
        }

        private void SpawnTestEnemy()
        {
            EnemySpawner.Instance?.SpawnEnemy(_testEnemyData);
        }

        private System.Collections.IEnumerator SpawnTestWave()
        {
            for (int i = 0; i < _enemyCount; i++)
            {
                EnemySpawner.Instance?.SpawnEnemy(_testEnemyData);
                yield return new WaitForSeconds(_spawnInterval);
            }
        }

        private void KillAllEnemies()
        {
            var enemies = FindObjectsOfType<Enemy>();
            foreach (var enemy in enemies)
            {
                if (!enemy.IsDead)
                {
                    enemy.TakeDamage(9999);
                }
            }
        }
    }
}
```

### Combat Integration Checklist

Verify the following flow works:

1. **Tower Placement**
   - [ ] Tower placed on valid grid cell
   - [ ] Tower initializes with correct data

2. **Enemy Spawn**
   - [ ] Enemy spawns at spawn point
   - [ ] Enemy navigates toward exit
   - [ ] Enemy health bar displays

3. **Targeting**
   - [ ] Tower detects enemy in range
   - [ ] Tower selects target based on priority
   - [ ] Turret rotates toward target

4. **Attack**
   - [ ] Tower fires at attack speed interval
   - [ ] Projectile spawns at fire point
   - [ ] Projectile moves toward target

5. **Damage**
   - [ ] Projectile hits enemy
   - [ ] Enemy takes damage (armor applied)
   - [ ] Health bar updates
   - [ ] Damage number displays

6. **Death**
   - [ ] Enemy dies at 0 health
   - [ ] Currency reward granted
   - [ ] Death VFX plays
   - [ ] Enemy returns to pool
   - [ ] Tower retargets

7. **Exit**
   - [ ] Enemy reaching exit damages lives
   - [ ] Enemy returns to pool

### Performance Validation

Create `CombatPerformanceTest.cs`:

```csharp
using UnityEngine;

namespace TowerDefense.Debug
{
    public class CombatPerformanceTest : MonoBehaviour
    {
        [SerializeField] private int _targetFPS = 60;

        private float _deltaTime;
        private int _frameCount;
        private float _fpsUpdateInterval = 0.5f;
        private float _fpsTimer;
        private float _currentFPS;

        private void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            _frameCount++;
            _fpsTimer += Time.unscaledDeltaTime;

            if (_fpsTimer >= _fpsUpdateInterval)
            {
                _currentFPS = _frameCount / _fpsTimer;
                _frameCount = 0;
                _fpsTimer = 0;

                if (_currentFPS < _targetFPS * 0.8f)
                {
                    UnityEngine.Debug.LogWarning($"FPS drop: {_currentFPS:F1} FPS");
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.Label($"FPS: {_currentFPS:F1}");
            GUILayout.Label($"Active Enemies: {EnemySpawner.Instance?.ActiveEnemyCount ?? 0}");
        }
    }
}
```

## Testing and Acceptance Criteria

### Manual Test Steps

1. Enter Play mode
2. Press T to spawn test tower
3. Press E to spawn single enemy
4. Observe full combat loop
5. Press W to spawn wave of enemies
6. Verify multiple targets handled
7. Press K to kill all enemies
8. Verify cleanup and currency

### Performance Targets

- 60 FPS with 20+ enemies
- No memory leaks after clearing enemies
- Smooth projectile movement

### Done When

- [ ] Full combat loop works end-to-end
- [ ] All systems integrate correctly
- [ ] Debug tools functional for testing
- [ ] Performance acceptable with many enemies
- [ ] No errors or warnings in console
- [ ] Memory properly managed (pools working)

## Dependencies

- Issues 14-18: All combat systems

## Notes

- This is primarily a testing/integration issue
- Fix any bugs discovered during testing
- Document any workarounds or known issues
