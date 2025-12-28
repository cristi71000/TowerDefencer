## Context

With enemies able to navigate the level, we need a spawning system that creates enemies at the spawn point. This issue implements the basic EnemySpawner component and an object pool for efficient enemy instantiation. The pool prevents garbage collection spikes from frequent spawning/destroying.

**Builds upon:** Issues 9-10 (Enemy Prefab, NavMesh)

## Detailed Implementation Instructions

### Generic Object Pool

Create `ObjectPool.cs` in `_Project/Scripts/Runtime/Core/`:

```csharp
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Core
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _available;
        private readonly List<T> _all;
        private readonly int _initialSize;
        private readonly bool _expandable;

        public int CountAll => _all.Count;
        public int CountActive => _all.Count - _available.Count;
        public int CountInactive => _available.Count;

        public ObjectPool(T prefab, int initialSize, Transform parent = null, bool expandable = true)
        {
            _prefab = prefab;
            _initialSize = initialSize;
            _parent = parent;
            _expandable = expandable;
            _available = new Queue<T>();
            _all = new List<T>();
            Prewarm();
        }

        private void Prewarm()
        {
            for (int i = 0; i < _initialSize; i++)
                CreateInstance();
        }

        private T CreateInstance()
        {
            T instance = Object.Instantiate(_prefab, _parent);
            instance.gameObject.SetActive(false);
            _available.Enqueue(instance);
            _all.Add(instance);
            return instance;
        }

        public T Get()
        {
            T instance = _available.Count > 0 ? _available.Dequeue() : (_expandable ? CreateInstance() : null);
            if (instance != null) instance.gameObject.SetActive(true);
            return instance;
        }

        public void Return(T instance)
        {
            if (instance == null) return;
            instance.gameObject.SetActive(false);
            _available.Enqueue(instance);
        }

        public void ReturnAll()
        {
            foreach (var instance in _all)
            {
                if (instance != null && instance.gameObject.activeInHierarchy)
                {
                    instance.gameObject.SetActive(false);
                    if (!_available.Contains(instance))
                        _available.Enqueue(instance);
                }
            }
        }
    }
}
```

### Enemy Pool Manager

Create `EnemyPoolManager.cs` in `_Project/Scripts/Runtime/Enemies/`:

```csharp
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Enemies
{
    public class EnemyPoolManager : MonoBehaviour
    {
        public static EnemyPoolManager Instance { get; private set; }
        [SerializeField] private int _defaultPoolSize = 20;
        private Dictionary<EnemyData, ObjectPool<Enemy>> _pools;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            _pools = new Dictionary<EnemyData, ObjectPool<Enemy>>();
        }

        public Enemy GetEnemy(EnemyData data)
        {
            if (!_pools.ContainsKey(data))
                _pools[data] = new ObjectPool<Enemy>(data.Prefab.GetComponent<Enemy>(), _defaultPoolSize, transform);
            var enemy = _pools[data].Get();
            enemy?.ResetEnemy();
            return enemy;
        }

        public void ReturnEnemy(Enemy enemy)
        {
            if (enemy?.Data != null && _pools.TryGetValue(enemy.Data, out var pool))
            {
                enemy.ResetEnemy();
                pool.Return(enemy);
            }
        }
    }
}
```

### Enemy Spawner

Create `EnemySpawner.cs` in `_Project/Scripts/Runtime/Enemies/`:

```csharp
using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance { get; private set; }
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private EnemyPoolManager _poolManager;

        private int _activeEnemyCount;
        public int ActiveEnemyCount => _activeEnemyCount;
        public event System.Action OnAllEnemiesDefeated;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            if (_spawnPoint == null) _spawnPoint = SpawnPoint.Instance?.transform;
        }

        public Enemy SpawnEnemy(EnemyData data)
        {
            var enemy = _poolManager.GetEnemy(data);
            enemy.transform.position = _spawnPoint.position;
            enemy.Initialize(data);
            enemy.OnDeath += HandleDeath;
            enemy.OnReachedEnd += HandleReachedEnd;
            _activeEnemyCount++;
            return enemy;
        }

        private void HandleDeath(Enemy enemy)
        {
            enemy.OnDeath -= HandleDeath;
            enemy.OnReachedEnd -= HandleReachedEnd;
            GameManager.Instance?.ModifyCurrency(enemy.Data.CurrencyReward);
            _activeEnemyCount--;
            _poolManager.ReturnEnemy(enemy);
            if (_activeEnemyCount <= 0) OnAllEnemiesDefeated?.Invoke();
        }

        private void HandleReachedEnd(Enemy enemy)
        {
            enemy.OnDeath -= HandleDeath;
            enemy.OnReachedEnd -= HandleReachedEnd;
            GameManager.Instance?.ModifyLives(-enemy.Data.LivesDamage);
            _activeEnemyCount--;
            _poolManager.ReturnEnemy(enemy);
            if (_activeEnemyCount <= 0) OnAllEnemiesDefeated?.Invoke();
        }
    }
}
```

## Testing and Acceptance Criteria

### Done When

- [ ] ObjectPool generic class implemented
- [ ] EnemyPoolManager manages enemy-specific pools
- [ ] EnemySpawner spawns enemies at spawn point
- [ ] Death triggers currency reward
- [ ] ReachEnd triggers lives damage
- [ ] Enemies return to pool correctly
- [ ] ActiveEnemyCount tracks correctly

## Dependencies

- Issue 9: Enemy Prefab
- Issue 10: NavMesh Setup
- Issue 2: Game Manager
