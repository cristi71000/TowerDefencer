## Context

Performance optimization ensures the game runs smoothly with many enemies, towers, and effects on screen. This issue addresses object pooling verification, draw call reduction, and profiling.

**Builds upon:** All gameplay systems

## Detailed Implementation Instructions

### Performance Profiler Integration

Create `PerformanceMonitor.cs`:

```csharp
using UnityEngine;

namespace TowerDefense.Debug
{
    public class PerformanceMonitor : MonoBehaviour
    {
        [SerializeField] private bool _showStats = true;
        [SerializeField] private float _updateInterval = 0.5f;

        private float _deltaTime;
        private float _fps;
        private int _drawCalls;
        private int _activeEnemies;
        private int _activeProjectiles;

        private GUIStyle _style;

        private void Awake()
        {
            _style = new GUIStyle();
            _style.normal.textColor = Color.white;
            _style.fontSize = 14;
        }

        private void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
            _fps = 1f / _deltaTime;
        }

        private void OnGUI()
        {
            if (!_showStats) return;

            int y = 10;
            int lineHeight = 18;

            GUI.Label(new Rect(10, y, 200, lineHeight), $"FPS: {_fps:F1}", _style);
            y += lineHeight;

            GUI.Label(new Rect(10, y, 200, lineHeight), $"Enemies: {EnemySpawner.Instance?.ActiveEnemyCount ?? 0}", _style);
            y += lineHeight;

            GUI.Label(new Rect(10, y, 200, lineHeight), $"Memory: {System.GC.GetTotalMemory(false) / 1024 / 1024}MB", _style);
        }
    }
}
```

### Pool Verification

Ensure all frequently spawned objects use pools:

**Verify Pooled:**
- [ ] Enemies (EnemyPoolManager)
- [ ] Projectiles (ProjectilePoolManager)
- [ ] VFX (need VFXPool)
- [ ] Damage numbers (need pool)
- [ ] Currency popups (need pool)

### VFX Pool Manager

Create `VFXPoolManager.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.VFX
{
    public class VFXPoolManager : MonoBehaviour
    {
        public static VFXPoolManager Instance { get; private set; }

        [SerializeField] private int _defaultPoolSize = 10;
        private Dictionary<GameObject, ObjectPool<ParticleSystem>> _pools;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            _pools = new Dictionary<GameObject, ObjectPool<ParticleSystem>>();
        }

        public ParticleSystem Get(GameObject prefab, Vector3 position)
        {
            if (!_pools.ContainsKey(prefab))
            {
                var ps = prefab.GetComponent<ParticleSystem>();
                _pools[prefab] = new ObjectPool<ParticleSystem>(ps, _defaultPoolSize, transform);
            }

            var instance = _pools[prefab].Get();
            instance.transform.position = position;
            instance.Play();

            StartCoroutine(ReturnAfterComplete(instance, prefab));
            return instance;
        }

        private System.Collections.IEnumerator ReturnAfterComplete(ParticleSystem ps, GameObject prefab)
        {
            yield return new WaitForSeconds(ps.main.duration + ps.main.startLifetime.constantMax);
            _pools[prefab].Return(ps);
        }
    }
}
```

### Draw Call Optimization

**Static Batching:**
- Mark static environment objects as Static
- Use combined meshes where possible

**GPU Instancing:**
- Enable GPU Instancing on materials
- Use same material for similar objects

**LOD (Level of Detail):**
- Add LOD groups to complex models
- Reduce detail at distance

### Memory Management

```csharp
// Add to EnemySpawner for periodic cleanup
private void LateUpdate()
{
    // Force GC in safe moments (between waves)
    if (GameManager.Instance?.CurrentState == GameState.PreWave)
    {
        // Optional: Resources.UnloadUnusedAssets();
    }
}
```

### Performance Targets

| Metric | Target | Acceptable |
|--------|--------|------------|
| FPS | 60 | 45+ |
| Enemies | 50+ | 30+ |
| Draw Calls | <200 | <500 |
| Memory | <500MB | <1GB |

### Profiling Checklist

1. Open Unity Profiler (Window > Analysis > Profiler)
2. Test wave 7 (swarm wave) performance
3. Check for:
   - GC spikes
   - Draw call batching
   - Physics overhead
   - Script execution time

## Testing and Acceptance Criteria

### Done When

- [ ] 60 FPS with 50 enemies on screen
- [ ] No GC spikes during gameplay
- [ ] All spawned objects use pools
- [ ] Draw calls under 200 typical
- [ ] Memory stable during extended play
- [ ] Performance monitor available for testing
- [ ] Profiler shows no major bottlenecks

## Dependencies

- All gameplay systems

## Notes

- Test on target minimum spec hardware
- Consider quality settings for lower-end devices
- Pool sizes may need adjustment based on max wave size
