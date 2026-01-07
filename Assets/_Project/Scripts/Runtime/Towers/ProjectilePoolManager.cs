using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Manages object pools for different projectile prefab types.
    /// Provides efficient projectile spawning and recycling.
    /// Uses the generic ObjectPool from Core.
    /// </summary>
    public class ProjectilePoolManager : MonoBehaviour
    {
        public static ProjectilePoolManager Instance { get; private set; }

        [Header("Pool Settings")]
        [SerializeField] private int _initialPoolSize = 20;
        [SerializeField] private Transform _poolContainer;

        private readonly Dictionary<GameObject, ObjectPool<Projectile>> _pools =
            new Dictionary<GameObject, ObjectPool<Projectile>>();

        /// <summary>
        /// Gets the total number of active projectiles across all pools.
        /// </summary>
        public int TotalActiveProjectiles
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

        /// <summary>
        /// Gets the total number of inactive projectiles across all pools.
        /// </summary>
        public int TotalInactiveProjectiles
        {
            get
            {
                int count = 0;
                foreach (var pool in _pools.Values)
                {
                    count += pool.InactiveCount;
                }
                return count;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                UnityEngine.Debug.LogWarning($"Multiple ProjectilePoolManager instances detected. Destroying duplicate on {gameObject.name}.");
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Create pool container if not assigned
            if (_poolContainer == null)
            {
                GameObject container = new GameObject("ProjectilePool");
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
        /// Gets a projectile from the pool for the specified prefab.
        /// Creates a new pool if one doesn't exist for this prefab type.
        /// </summary>
        /// <param name="prefab">The projectile prefab to spawn</param>
        /// <returns>A projectile instance ready for use, or null if prefab is invalid</returns>
        public Projectile GetProjectile(GameObject prefab)
        {
            if (prefab == null)
            {
                UnityEngine.Debug.LogError("ProjectilePoolManager.GetProjectile called with null prefab.");
                return null;
            }

            // Get or create pool for this prefab type
            if (!_pools.TryGetValue(prefab, out ObjectPool<Projectile> pool))
            {
                pool = CreatePool(prefab);
                if (pool == null)
                {
                    return null;
                }
                _pools[prefab] = pool;
            }

            Projectile projectile = pool.Get();
            projectile.Reset(); // Ensure clean state
            projectile.PrefabSource = prefab; // Store prefab reference for return
            return projectile;
        }

        /// <summary>
        /// Gets a projectile from the pool and positions it.
        /// </summary>
        /// <param name="prefab">The projectile prefab to spawn</param>
        /// <param name="position">Spawn position</param>
        /// <param name="rotation">Spawn rotation</param>
        /// <returns>A projectile instance ready for use</returns>
        public Projectile GetProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            Projectile projectile = GetProjectile(prefab);
            if (projectile != null)
            {
                projectile.transform.SetPositionAndRotation(position, rotation);
            }
            return projectile;
        }

        /// <summary>
        /// Returns a projectile to its pool.
        /// </summary>
        /// <param name="projectile">The projectile to return</param>
        /// <param name="prefab">The prefab the projectile was spawned from</param>
        public void ReturnProjectile(Projectile projectile, GameObject prefab)
        {
            if (projectile == null)
            {
                UnityEngine.Debug.LogWarning("ProjectilePoolManager.ReturnProjectile called with null projectile.");
                return;
            }

            if (prefab == null)
            {
                UnityEngine.Debug.LogWarning($"Projectile '{projectile.name}' has no prefab reference. Destroying instead.");
                Destroy(projectile.gameObject);
                return;
            }

            if (_pools.TryGetValue(prefab, out ObjectPool<Projectile> pool))
            {
                pool.Return(projectile);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"No pool found for prefab '{prefab.name}'. Destroying projectile instead.");
                Destroy(projectile.gameObject);
            }
        }

        /// <summary>
        /// Returns all active projectiles to their pools.
        /// </summary>
        public void ReturnAllProjectiles()
        {
            foreach (var pool in _pools.Values)
            {
                pool.ReturnAll();
            }
        }

        /// <summary>
        /// Prewarms a pool for the specified projectile prefab.
        /// </summary>
        /// <param name="prefab">The projectile prefab to prewarm</param>
        /// <param name="count">Number of instances to create</param>
        public void PrewarmPool(GameObject prefab, int count)
        {
            if (prefab == null)
            {
                UnityEngine.Debug.LogWarning("Cannot prewarm pool with null prefab.");
                return;
            }

            Projectile prefabProjectile = prefab.GetComponent<Projectile>();
            if (prefabProjectile == null)
            {
                UnityEngine.Debug.LogWarning($"Prefab '{prefab.name}' does not have a Projectile component. Cannot prewarm.");
                return;
            }

            if (!_pools.TryGetValue(prefab, out ObjectPool<Projectile> pool))
            {
                pool = CreatePool(prefab);
                if (pool == null)
                {
                    return;
                }
                _pools[prefab] = pool;
            }

            pool.Prewarm(count);
        }

        /// <summary>
        /// Checks if a pool exists for the given prefab.
        /// </summary>
        /// <param name="prefab">The projectile prefab to check</param>
        /// <returns>True if a pool exists for this prefab</returns>
        public bool HasPool(GameObject prefab)
        {
            return prefab != null && _pools.ContainsKey(prefab);
        }

        /// <summary>
        /// Gets pool statistics for a specific prefab.
        /// </summary>
        /// <param name="prefab">The projectile prefab</param>
        /// <param name="active">Number of active projectiles</param>
        /// <param name="inactive">Number of inactive projectiles</param>
        /// <returns>True if pool exists and stats were retrieved</returns>
        public bool GetPoolStats(GameObject prefab, out int active, out int inactive)
        {
            active = 0;
            inactive = 0;

            if (prefab == null || !_pools.TryGetValue(prefab, out ObjectPool<Projectile> pool))
            {
                return false;
            }

            active = pool.ActiveCount;
            inactive = pool.InactiveCount;
            return true;
        }

        private ObjectPool<Projectile> CreatePool(GameObject prefab)
        {
            // Validate prefab has Projectile component
            Projectile prefabProjectile = prefab.GetComponent<Projectile>();
            if (prefabProjectile == null)
            {
                UnityEngine.Debug.LogError($"Prefab '{prefab.name}' does not have a Projectile component.");
                return null;
            }

            // Create pool container for this prefab type
            GameObject typeContainer = new GameObject($"Pool_{prefab.name}");
            typeContainer.transform.SetParent(_poolContainer);

            return new ObjectPool<Projectile>(
                prefabProjectile,
                typeContainer.transform,
                _initialPoolSize,
                onGet: null,
                onReturn: projectile =>
                {
                    projectile.Reset();
                    // Reparent to pool container when returned
                    projectile.transform.SetParent(typeContainer.transform);
                }
            );
        }
    }
}
