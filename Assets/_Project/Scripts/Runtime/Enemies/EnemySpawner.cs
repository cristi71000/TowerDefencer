using System;
using UnityEngine;
using TowerDefense.Core;
using TowerDefense.UI;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// Handles spawning enemies at the spawn point and tracking active enemies.
    /// Integrates with GameManager for currency rewards and life damage.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance { get; private set; }

        [Header("Currency Popup")]
        [SerializeField] private CurrencyPopup _currencyPopupPrefab;

        [Header("Debug")]
        [SerializeField] private bool _logSpawnEvents;

        private int _activeEnemyCount;

        /// <summary>
        /// Current number of active enemies in the level.
        /// </summary>
        public int ActiveEnemyCount => _activeEnemyCount;

        /// <summary>
        /// Fired when an enemy is spawned.
        /// </summary>
        public event Action<Enemy> OnEnemySpawned;

        /// <summary>
        /// Fired when an enemy is killed (not reached end).
        /// </summary>
        public event Action<Enemy> OnEnemyKilled;

        /// <summary>
        /// Fired when an enemy reaches the exit.
        /// </summary>
        public event Action<Enemy> OnEnemyReachedEnd;

        /// <summary>
        /// Fired when all active enemies have been defeated or reached the end.
        /// </summary>
        public event Action OnAllEnemiesDefeated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                UnityEngine.Debug.LogWarning($"Multiple EnemySpawner instances detected. Destroying duplicate on {gameObject.name}.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to ExitPoint events in Start to ensure ExitPoint has completed Awake
            if (ExitPoint.Instance != null)
            {
                ExitPoint.Instance.OnEnemyReachedExit += HandleEnemyReachedExit;
            }
            else
            {
                UnityEngine.Debug.LogError("EnemySpawner: ExitPoint.Instance is null in Start(). Enemy exit detection will not work.");
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from ExitPoint events
            if (ExitPoint.Instance != null)
            {
                ExitPoint.Instance.OnEnemyReachedExit -= HandleEnemyReachedExit;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Spawns an enemy at the spawn point and sends it to the exit.
        /// </summary>
        /// <param name="enemyData">The enemy type to spawn</param>
        /// <returns>The spawned enemy, or null if spawn failed</returns>
        public Enemy SpawnEnemy(EnemyData enemyData)
        {
            if (enemyData == null)
            {
                UnityEngine.Debug.LogError("EnemySpawner.SpawnEnemy called with null enemyData.");
                return null;
            }

            if (SpawnPoint.Instance == null)
            {
                UnityEngine.Debug.LogError("Cannot spawn enemy: SpawnPoint.Instance is null.");
                return null;
            }

            if (EnemyPoolManager.Instance == null)
            {
                UnityEngine.Debug.LogError("Cannot spawn enemy: EnemyPoolManager.Instance is null.");
                return null;
            }

            // Get enemy from pool at spawn position
            Vector3 spawnPosition = SpawnPoint.Instance.Position;
            Quaternion spawnRotation = SpawnPoint.Instance.transform.rotation;
            
            Enemy enemy = EnemyPoolManager.Instance.GetEnemy(enemyData, spawnPosition, spawnRotation);
            if (enemy == null)
            {
                UnityEngine.Debug.LogError($"Failed to get enemy from pool for type '{enemyData.EnemyName}'.");
                return null;
            }

            // Subscribe to enemy events
            enemy.OnDeath += HandleEnemyDeath;
            enemy.OnReachedEnd += HandleEnemyReachEnd;

            // Set destination to exit point
            if (ExitPoint.Instance != null)
            {
                enemy.SetDestination(ExitPoint.Instance.Position);
            }
            else
            {
                UnityEngine.Debug.LogWarning("ExitPoint.Instance is null. Enemy will not move to destination.");
            }

            _activeEnemyCount++;

            if (_logSpawnEvents)
            {
                UnityEngine.Debug.Log($"Spawned enemy '{enemyData.EnemyName}' at {spawnPosition}. Active count: {_activeEnemyCount}");
            }

            OnEnemySpawned?.Invoke(enemy);

            return enemy;
        }

        /// <summary>
        /// Handles the ExitPoint trigger detection.
        /// </summary>
        private void HandleEnemyReachedExit(GameObject enemyObject)
        {
            if (enemyObject == null) return;

            Enemy enemy = enemyObject.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead && !enemy.HasReachedEnd)
            {
                enemy.ReachEnd();
            }
        }

        /// <summary>
        /// Handles enemy death - grants currency reward.
        /// </summary>
        private void HandleEnemyDeath(Enemy enemy)
        {
            if (enemy == null) return;

            // Unsubscribe from events
            enemy.OnDeath -= HandleEnemyDeath;
            enemy.OnReachedEnd -= HandleEnemyReachEnd;

            // Grant currency reward
            if (GameManager.Instance != null && enemy.Data != null)
            {
                GameManager.Instance.ModifyCurrency(enemy.Data.KillReward);

                // Spawn currency popup
                SpawnCurrencyPopup(enemy.transform.position, enemy.Data.KillReward);

                if (_logSpawnEvents)
                {
                    UnityEngine.Debug.Log($"Enemy '{enemy.Data.EnemyName}' killed. Awarded {enemy.Data.KillReward} currency.");
                }
            }

            OnEnemyKilled?.Invoke(enemy);

            // Return to pool
            ReturnEnemyToPool(enemy);
        }

        /// <summary>
        /// Spawns a currency popup at the specified position showing the reward amount.
        /// </summary>
        private void SpawnCurrencyPopup(Vector3 position, int amount)
        {
            if (_currencyPopupPrefab == null) return;

            Vector3 popupPosition = position + Vector3.up * 0.5f;
            CurrencyPopup popup = Instantiate(_currencyPopupPrefab, popupPosition, Quaternion.identity);
            popup.Initialize(amount);
        }

        /// <summary>
        /// Handles enemy reaching the end - damages player lives.
        /// </summary>
        private void HandleEnemyReachEnd(Enemy enemy)
        {
            if (enemy == null) return;

            // Unsubscribe from events
            enemy.OnDeath -= HandleEnemyDeath;
            enemy.OnReachedEnd -= HandleEnemyReachEnd;

            // Damage player lives
            if (GameManager.Instance != null && enemy.Data != null)
            {
                GameManager.Instance.ModifyLives(-enemy.Data.Damage);

                if (_logSpawnEvents)
                {
                    UnityEngine.Debug.Log($"Enemy '{enemy.Data.EnemyName}' reached exit. Lost {enemy.Data.Damage} lives.");
                }
            }

            OnEnemyReachedEnd?.Invoke(enemy);

            // Return to pool
            ReturnEnemyToPool(enemy);
        }

        private void ReturnEnemyToPool(Enemy enemy)
        {
            _activeEnemyCount--;

            if (_logSpawnEvents)
            {
                UnityEngine.Debug.Log($"Enemy returned to pool. Active count: {_activeEnemyCount}");
            }

            // Return enemy to pool
            if (EnemyPoolManager.Instance != null)
            {
                EnemyPoolManager.Instance.ReturnEnemy(enemy);
            }

            // Check if all enemies are defeated
            if (_activeEnemyCount <= 0)
            {
                _activeEnemyCount = 0;
                OnAllEnemiesDefeated?.Invoke();

                if (_logSpawnEvents)
                {
                    UnityEngine.Debug.Log("All enemies defeated!");
                }
            }
        }

        /// <summary>
        /// Resets the spawner state. Call this when starting a new game/level.
        /// </summary>
        public void ResetSpawner()
        {
            _activeEnemyCount = 0;
        }
    }
}
