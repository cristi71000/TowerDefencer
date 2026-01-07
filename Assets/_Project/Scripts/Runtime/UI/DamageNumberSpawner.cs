using UnityEngine;

namespace TowerDefense.UI
{
    /// <summary>
    /// Singleton that spawns floating damage numbers in the world.
    /// Provides a simple interface for spawning damage indicators at any position.
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        private static DamageNumberSpawner _instance;
        public static DamageNumberSpawner Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<DamageNumberSpawner>();

                    if (_instance == null)
                    {
                        UnityEngine.Debug.LogWarning("DamageNumberSpawner not found in scene. Creating temporary instance.");
                        GameObject spawnerObject = new GameObject("DamageNumberSpawner");
                        _instance = spawnerObject.AddComponent<DamageNumberSpawner>();
                    }
                }
                return _instance;
            }
        }

        [Header("Prefab")]
        [SerializeField] private GameObject _damageNumberPrefab;

        [Header("Spawn Settings")]
        [SerializeField] private float _spawnOffsetY = 0.5f;
        [SerializeField] private float _randomOffsetRange = 0.3f;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                UnityEngine.Debug.LogWarning($"Multiple DamageNumberSpawner instances found. Destroying duplicate on {gameObject.name}");
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Spawns a floating damage number at the specified position.
        /// </summary>
        /// <param name="position">The world position to spawn at.</param>
        /// <param name="damage">The damage amount to display.</param>
        /// <param name="isCritical">Whether this was a critical hit (affects color and size).</param>
        public void SpawnDamageNumber(Vector3 position, float damage, bool isCritical = false)
        {
            if (_damageNumberPrefab == null)
            {
                UnityEngine.Debug.LogWarning("DamageNumberSpawner: Prefab not assigned, cannot spawn damage number.");
                return;
            }

            // Apply offset and randomness
            Vector3 spawnPosition = position;
            spawnPosition.y += _spawnOffsetY;
            spawnPosition.x += Random.Range(-_randomOffsetRange, _randomOffsetRange);
            spawnPosition.z += Random.Range(-_randomOffsetRange, _randomOffsetRange);

            // Spawn the damage number
            GameObject damageNumberObject = Instantiate(_damageNumberPrefab, spawnPosition, Quaternion.identity);

            // Initialize the floating damage number component
            FloatingDamageNumber floatingNumber = damageNumberObject.GetComponent<FloatingDamageNumber>();
            if (floatingNumber != null)
            {
                floatingNumber.Initialize(damage, isCritical);
            }
            else
            {
                UnityEngine.Debug.LogError("DamageNumberSpawner: Prefab does not have FloatingDamageNumber component.");
                Destroy(damageNumberObject);
            }
        }

        /// <summary>
        /// Spawns a floating damage number at the specified position with a custom offset.
        /// </summary>
        /// <param name="position">The base world position.</param>
        /// <param name="offset">Additional offset to apply.</param>
        /// <param name="damage">The damage amount to display.</param>
        /// <param name="isCritical">Whether this was a critical hit.</param>
        public void SpawnDamageNumber(Vector3 position, Vector3 offset, float damage, bool isCritical = false)
        {
            SpawnDamageNumber(position + offset, damage, isCritical);
        }

        /// <summary>
        /// Checks if the spawner is ready to spawn damage numbers.
        /// </summary>
        public bool IsReady => _damageNumberPrefab != null;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _spawnOffsetY = Mathf.Max(0f, _spawnOffsetY);
            _randomOffsetRange = Mathf.Max(0f, _randomOffsetRange);
        }
#endif
    }
}
