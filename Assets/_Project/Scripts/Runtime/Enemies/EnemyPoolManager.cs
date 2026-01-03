using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// Manages object pools for different enemy types.
    /// Provides efficient enemy spawning and recycling.
    /// </summary>
    public class EnemyPoolManager : MonoBehaviour
    {
        public static EnemyPoolManager Instance { get; private set; }

        [Header("Pool Settings")]
        [SerializeField] private int _initialPoolSize = 10;
        [SerializeField] private Transform _poolContainer;

        private readonly Dictionary<EnemyData, ObjectPool<Enemy>> _pools = new Dictionary<EnemyData, ObjectPool<Enemy>>();

        /// <summary>
        /// Gets the total number of active enemies across all pools.
        /// </summary>
        public int TotalActiveEnemies
        {
            get
            {
                int count = 0;
                foreach (var pool in _pools.Values)
                {
                    count += pool.ActiveCount;
                }
                return count;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"Multiple EnemyPoolManager instances detected. Destroying duplicate on {gameObject.name}.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Create pool container if not assigned
            if (_poolContainer == null)
            {
                GameObject container = new GameObject("EnemyPool");
                container.transform.SetParent(transform);
                _poolContainer = container.transform;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                // Clear all pools
                foreach (var pool in _pools.Values)
                {
                    pool.Clear();
                }
                _pools.Clear();
                Instance = null;
            }
        }

        /// <summary>
        /// Gets an enemy from the pool for the specified enemy data.
        /// Creates a new pool if one doesn't exist for this enemy type.
        /// </summary>
        /// <param name="enemyData">The enemy data/type to spawn</param>
        /// <returns>An enemy instance ready for use</returns>
        public Enemy GetEnemy(EnemyData enemyData)
        {
            if (enemyData == null)
            {
                Debug.LogError("EnemyPoolManager.GetEnemy called with null EnemyData.");
                return null;
            }

            if (enemyData.Prefab == null)
            {
                Debug.LogError($"EnemyData '{enemyData.EnemyName}' has no prefab assigned.");
                return null;
            }

            // Get or create pool for this enemy type
            if (!_pools.TryGetValue(enemyData, out ObjectPool<Enemy> pool))
            {
                pool = CreatePool(enemyData);
                _pools[enemyData] = pool;
            }

            Enemy enemy = pool.Get();
            enemy.ResetEnemy(); // Ensure clean state before initialization
            enemy.Initialize(enemyData);
            return enemy;
        }

        /// <summary>
        /// Gets an enemy from the pool and positions it.
        /// </summary>
        /// <param name="enemyData">The enemy data/type to spawn</param>
        /// <param name="position">Spawn position</param>
        /// <param name="rotation">Spawn rotation</param>
        /// <returns>An enemy instance ready for use</returns>
        public Enemy GetEnemy(EnemyData enemyData, Vector3 position, Quaternion rotation)
        {
            Enemy enemy = GetEnemy(enemyData);
            if (enemy != null)
            {
                enemy.transform.SetPositionAndRotation(position, rotation);
            }
            return enemy;
        }

        /// <summary>
        /// Returns an enemy to its pool.
        /// </summary>
        /// <param name="enemy">The enemy to return</param>
        public void ReturnEnemy(Enemy enemy)
        {
            if (enemy == null)
            {
                Debug.LogWarning("EnemyPoolManager.ReturnEnemy called with null enemy.");
                return;
            }

            // Get data reference before reset clears it
            EnemyData data = enemy.Data;
            if (data == null)
            {
                Debug.LogWarning($"Enemy '{enemy.name}' has no data, cannot determine pool. Destroying instead.");
                Destroy(enemy.gameObject);
                return;
            }

            if (_pools.TryGetValue(data, out ObjectPool<Enemy> pool))
            {
                // Note: ResetEnemy is called by the pool's onReturn callback
                pool.Return(enemy);
            }
            else
            {
                Debug.LogWarning($"No pool found for enemy type '{data.EnemyName}'. Destroying instead.");
                Destroy(enemy.gameObject);
            }
        }

        /// <summary>
        /// Returns all active enemies to their pools.
        /// </summary>
        public void ReturnAllEnemies()
        {
            foreach (var pool in _pools.Values)
            {
                pool.ReturnAll();
            }
        }

        /// <summary>
        /// Prewarms a pool for the specified enemy type.
        /// </summary>
        /// <param name="enemyData">The enemy data/type to prewarm</param>
        /// <param name="count">Number of instances to create</param>
        public void PrewarmPool(EnemyData enemyData, int count)
        {
            if (enemyData == null || enemyData.Prefab == null)
            {
                Debug.LogWarning("Cannot prewarm pool with null enemy data or prefab.");
                return;
            }

            if (!_pools.TryGetValue(enemyData, out ObjectPool<Enemy> pool))
            {
                pool = CreatePool(enemyData);
                _pools[enemyData] = pool;
            }

            pool.Prewarm(count);
        }

        private ObjectPool<Enemy> CreatePool(EnemyData enemyData)
        {
            // Get the Enemy component from the prefab
            Enemy prefabEnemy = enemyData.Prefab.GetComponent<Enemy>();
            if (prefabEnemy == null)
            {
                Debug.LogError($"Prefab for '{enemyData.EnemyName}' does not have an Enemy component.");
                return null;
            }

            // Create pool container for this enemy type
            GameObject typeContainer = new GameObject($"Pool_{enemyData.EnemyName}");
            typeContainer.transform.SetParent(_poolContainer);

            return new ObjectPool<Enemy>(
                prefabEnemy,
                typeContainer.transform,
                _initialPoolSize,
                onGet: null,
                onReturn: enemy => enemy.ResetEnemy()
            );
        }
    }
}
