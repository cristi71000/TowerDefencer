## Context

Visual effects make the game feel alive and responsive. This issue consolidates and polishes all VFX: projectile trails, impacts, tower muzzle flashes, and environmental effects.

**Builds upon:** All combat and tower issues

## Detailed Implementation Instructions

### VFX Manager

Create `VFXManager.cs`:

```csharp
using UnityEngine;

namespace TowerDefense.VFX
{
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        [Header("Projectile VFX")]
        [SerializeField] private GameObject _basicProjectileImpact;
        [SerializeField] private GameObject _explosionImpact;
        [SerializeField] private GameObject _freezeImpact;
        [SerializeField] private GameObject _sniperImpact;

        [Header("Tower VFX")]
        [SerializeField] private GameObject _muzzleFlashBasic;
        [SerializeField] private GameObject _muzzleFlashCannon;

        [Header("Enemy VFX")]
        [SerializeField] private GameObject _enemyDeath;
        [SerializeField] private GameObject _enemySlowed;

        [Header("Environment")]
        [SerializeField] private GameObject _dustParticles;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void SpawnImpact(VFXType type, Vector3 position)
        {
            GameObject prefab = GetVFXPrefab(type);
            if (prefab != null)
            {
                var instance = Instantiate(prefab, position, Quaternion.identity);
                Destroy(instance, 2f);
            }
        }

        private GameObject GetVFXPrefab(VFXType type)
        {
            return type switch
            {
                VFXType.BasicImpact => _basicProjectileImpact,
                VFXType.Explosion => _explosionImpact,
                VFXType.FreezeImpact => _freezeImpact,
                VFXType.SniperImpact => _sniperImpact,
                VFXType.MuzzleFlash => _muzzleFlashBasic,
                VFXType.CannonFlash => _muzzleFlashCannon,
                VFXType.EnemyDeath => _enemyDeath,
                VFXType.EnemySlowed => _enemySlowed,
                _ => null
            };
        }
    }

    public enum VFXType
    {
        BasicImpact,
        Explosion,
        FreezeImpact,
        SniperImpact,
        MuzzleFlash,
        CannonFlash,
        EnemyDeath,
        EnemySlowed
    }
}
```

### VFX Prefabs

Create the following particle system prefabs:

**VFX_BasicImpact.prefab:**
- Small spark burst
- 10-15 particles
- Lifetime: 0.3s
- Yellow/orange color

**VFX_Explosion.prefab:**
- Large burst with fire colors
- 30-50 particles
- Shockwave ring
- Smoke follow-up

**VFX_FreezeImpact.prefab:**
- Ice crystal shatter
- Blue/white colors
- Frost ring on ground

**VFX_SniperImpact.prefab:**
- Sharp spark
- Quick flash
- Small debris

**VFX_MuzzleFlash.prefab:**
- Quick bright flash
- Small smoke puff
- Lifetime: 0.1s

**VFX_EnemyDeath.prefab:**
- Burst of enemy color
- Fade particles
- Consider different per enemy type

### Projectile Trails

Update projectiles with TrailRenderer:

```csharp
// Add to projectile prefabs
TrailRenderer settings:
- Time: 0.2
- Start Width: 0.1
- End Width: 0
- Material: Additive particle material
```

### Screen Effects

Create `ScreenEffects.cs`:

```csharp
using UnityEngine;
using Cinemachine;

namespace TowerDefense.VFX
{
    public class ScreenEffects : MonoBehaviour
    {
        public static ScreenEffects Instance { get; private set; }

        [SerializeField] private CinemachineImpulseSource _impulseSource;

        private void Awake()
        {
            Instance = this;
        }

        public void ShakeCamera(float intensity = 0.5f)
        {
            _impulseSource?.GenerateImpulse(intensity);
        }
    }
}
```

### Camera Shake Integration

Add shake on:
- Enemy death (small)
- Boss damage (medium)
- Boss death (large)
- Cannon fire (small)

## Testing and Acceptance Criteria

### Done When

- [ ] VFXManager centralizes VFX spawning
- [ ] All projectile impacts have VFX
- [ ] Muzzle flashes on tower attacks
- [ ] Projectile trails visible
- [ ] Enemy death VFX plays
- [ ] Camera shake on significant events
- [ ] VFX auto-cleanup (no memory leaks)
- [ ] Performance acceptable with many VFX

## Dependencies

- All combat issues
