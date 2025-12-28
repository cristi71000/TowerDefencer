## Context

The wave spawner uses WaveData to spawn enemies in the correct sequence. This issue implements the WaveManager that controls wave flow, spawning, and progression through the level.

**Builds upon:** Issues 11, 25 (Enemy Spawner, WaveData)

## Detailed Implementation Instructions

### Wave Manager

Create `WaveManager.cs` in `_Project/Scripts/Runtime/Waves/`:

```csharp
using System.Collections;
using UnityEngine;
using TowerDefense.Core;
using TowerDefense.Enemies;
using TowerDefense.Core.Events;

namespace TowerDefense.Waves
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private LevelWaveConfig _levelConfig;

        [Header("Events")]
        [SerializeField] private IntEventChannel _onWaveStarted;
        [SerializeField] private IntEventChannel _onWaveCompleted;
        [SerializeField] private GameEventChannel _onAllWavesCompleted;

        private int _currentWaveIndex = -1;
        private WaveData _currentWave;
        private bool _isSpawning;
        private int _enemiesRemainingToSpawn;
        private Coroutine _spawnCoroutine;

        public int CurrentWaveNumber => _currentWaveIndex + 1;
        public int TotalWaves => _levelConfig?.TotalWaves ?? 0;
        public bool IsSpawning => _isSpawning;
        public WaveData CurrentWave => _currentWave;
        public bool AllWavesCompleted => _currentWaveIndex >= TotalWaves - 1 && !_isSpawning;

        public event System.Action<int> OnWaveStarted;
        public event System.Action<int> OnWaveCompleted;
        public event System.Action OnAllWavesCompleted;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (_levelConfig != null && _levelConfig.AutoStartFirstWave)
            {
                StartNextWave();
            }
        }

        public void StartNextWave()
        {
            if (_isSpawning)
            {
                Debug.LogWarning("Already spawning a wave!");
                return;
            }

            _currentWaveIndex++;

            if (_currentWaveIndex >= TotalWaves)
            {
                Debug.Log("All waves completed!");
                TriggerAllWavesCompleted();
                return;
            }

            _currentWave = _levelConfig.Waves[_currentWaveIndex];
            _spawnCoroutine = StartCoroutine(SpawnWaveCoroutine());
        }

        private IEnumerator SpawnWaveCoroutine()
        {
            _isSpawning = true;
            _enemiesRemainingToSpawn = _currentWave.TotalEnemyCount;

            // Pre-wave delay
            if (_currentWave.PreWaveDelay > 0)
            {
                yield return new WaitForSeconds(_currentWave.PreWaveDelay);
            }

            // Notify wave started
            OnWaveStarted?.Invoke(CurrentWaveNumber);
            _onWaveStarted?.RaiseEvent(CurrentWaveNumber);
            GameManager.Instance?.SetGameState(GameState.WaveActive);

            Debug.Log($"Wave {CurrentWaveNumber} started: {_currentWave.WaveName}");

            // Spawn each group
            foreach (var group in _currentWave.SpawnGroups)
            {
                // Delay before group
                if (group.Enemies.Length > 0 && group.Enemies[0].DelayBeforeGroup > 0)
                {
                    yield return new WaitForSeconds(group.Enemies[0].DelayBeforeGroup);
                }

                // Spawn enemies in group
                foreach (var spawnInfo in group.Enemies)
                {
                    for (int i = 0; i < spawnInfo.Count; i++)
                    {
                        SpawnEnemy(spawnInfo.EnemyType);
                        _enemiesRemainingToSpawn--;

                        if (spawnInfo.SpawnInterval > 0 && i < spawnInfo.Count - 1)
                        {
                            yield return new WaitForSeconds(spawnInfo.SpawnInterval);
                        }
                    }
                }

                // Delay after group
                if (group.DelayAfterGroup > 0)
                {
                    yield return new WaitForSeconds(group.DelayAfterGroup);
                }
            }

            _isSpawning = false;

            // Subscribe to check for wave completion
            EnemySpawner.Instance.OnAllEnemiesDefeated += HandleWaveCleared;
            CheckWaveCompletion();
        }

        private void SpawnEnemy(EnemyData data)
        {
            EnemySpawner.Instance?.SpawnEnemy(data);
        }

        private void HandleWaveCleared()
        {
            EnemySpawner.Instance.OnAllEnemiesDefeated -= HandleWaveCleared;
            CompleteWave();
        }

        private void CheckWaveCompletion()
        {
            if (!_isSpawning && EnemySpawner.Instance.ActiveEnemyCount == 0)
            {
                EnemySpawner.Instance.OnAllEnemiesDefeated -= HandleWaveCleared;
                CompleteWave();
            }
        }

        private void CompleteWave()
        {
            Debug.Log($"Wave {CurrentWaveNumber} completed!");

            // Grant completion bonus
            Economy.EconomyManager.Instance?.AddCurrency(_currentWave.CompletionBonus);

            OnWaveCompleted?.Invoke(CurrentWaveNumber);
            _onWaveCompleted?.RaiseEvent(CurrentWaveNumber);

            GameManager.Instance?.SetGameState(GameState.PreWave);

            // Check if all waves done
            if (_currentWaveIndex >= TotalWaves - 1)
            {
                TriggerAllWavesCompleted();
            }
            else if (_levelConfig.AutoStartNextWave)
            {
                StartCoroutine(AutoStartNextWaveCoroutine());
            }
        }

        private IEnumerator AutoStartNextWaveCoroutine()
        {
            yield return new WaitForSeconds(_levelConfig.TimeBetweenWaves);
            StartNextWave();
        }

        private void TriggerAllWavesCompleted()
        {
            Debug.Log("All waves completed - VICTORY!");
            OnAllWavesCompleted?.Invoke();
            _onAllWavesCompleted?.RaiseEvent();
            GameManager.Instance?.SetGameState(GameState.Victory);
        }

        public void SkipToWave(int waveIndex)
        {
            if (_isSpawning)
            {
                StopCoroutine(_spawnCoroutine);
                EnemySpawner.Instance?.ClearAllEnemies();
            }

            _currentWaveIndex = waveIndex - 1;
            _isSpawning = false;
        }
    }
}
```

### Scene Setup

1. Create WaveManager GameObject under --- MANAGEMENT ---
2. Assign LevelWaveConfig asset
3. Configure event channels

## Testing and Acceptance Criteria

### Done When

- [ ] WaveManager spawns enemies per WaveData
- [ ] Spawn groups execute in sequence
- [ ] Spawn intervals respected
- [ ] Wave start event fires
- [ ] Wave completion detected when all enemies cleared
- [ ] Wave bonus granted on completion
- [ ] Auto-start next wave works
- [ ] All waves complete triggers victory

## Dependencies

- Issue 11: Enemy Spawner
- Issue 25: WaveData
