## Context

With targeting and projectiles ready, towers need to actually attack. This issue implements the TowerAttack component that fires projectiles at the current target based on attack speed and coordinates with the targeting and aiming systems.

**Builds upon:** Issues 14-15 (Targeting, Projectiles)

## Detailed Implementation Instructions

### Tower Attack Component

Create `TowerAttack.cs` in `_Project/Scripts/Runtime/Towers/`:

```csharp
using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Towers
{
    [RequireComponent(typeof(Tower))]
    [RequireComponent(typeof(TowerTargeting))]
    public class TowerAttack : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _requireAiming = true;
        [SerializeField] private float _aimThreshold = 0.9f;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;

        private Tower _tower;
        private TowerTargeting _targeting;
        private TowerAiming _aiming;
        private float _attackTimer;
        private GameObject _projectilePrefab;

        public event System.Action OnAttack;

        private void Awake()
        {
            _tower = GetComponent<Tower>();
            _targeting = GetComponent<TowerTargeting>();
            _aiming = GetComponent<TowerAiming>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (_tower.Data != null)
            {
                _projectilePrefab = _tower.Data.ProjectilePrefab;
            }
        }

        private void Update()
        {
            if (_tower.Data == null) return;

            _attackTimer += Time.deltaTime;

            if (CanAttack())
            {
                Attack();
            }
        }

        private bool CanAttack()
        {
            if (!_targeting.HasTarget) return false;
            if (_attackTimer < _tower.Data.AttackInterval) return false;
            if (_requireAiming && _aiming != null && !_aiming.IsAimedAtTarget()) return false;

            return true;
        }

        private void Attack()
        {
            _attackTimer = 0f;

            Enemy target = _targeting.CurrentTarget;
            if (target == null) return;

            FireProjectile(target);
            PlayAttackEffects();

            OnAttack?.Invoke();
        }

        private void FireProjectile(Enemy target)
        {
            if (_projectilePrefab == null)
            {
                // Direct damage if no projectile
                target.TakeDamage(_tower.Data.Damage);
                return;
            }

            Transform firePoint = _tower.GetFirePoint();

            Projectile projectile = ProjectilePoolManager.Instance?.GetProjectile(_projectilePrefab);
            if (projectile == null)
            {
                projectile = Instantiate(_projectilePrefab, firePoint.position, firePoint.rotation)
                    .GetComponent<Projectile>();
            }
            else
            {
                projectile.transform.position = firePoint.position;
                projectile.transform.rotation = firePoint.rotation;
            }

            projectile.Initialize(
                target,
                _tower.Data.Damage,
                _tower.Data.ProjectileSpeed,
                _tower.Data.AOERadius
            );

            // Subscribe to return projectile to pool
            projectile.OnHit += HandleProjectileHit;
            projectile.OnExpired += HandleProjectileExpired;
        }

        private void HandleProjectileHit(Projectile projectile)
        {
            projectile.OnHit -= HandleProjectileHit;
            projectile.OnExpired -= HandleProjectileExpired;
            ReturnProjectile(projectile);
        }

        private void HandleProjectileExpired(Projectile projectile)
        {
            projectile.OnHit -= HandleProjectileHit;
            projectile.OnExpired -= HandleProjectileExpired;
            ReturnProjectile(projectile);
        }

        private void ReturnProjectile(Projectile projectile)
        {
            if (ProjectilePoolManager.Instance != null && _projectilePrefab != null)
            {
                ProjectilePoolManager.Instance.ReturnProjectile(projectile, _projectilePrefab);
            }
            else
            {
                Destroy(projectile.gameObject);
            }
        }

        private void PlayAttackEffects()
        {
            // Audio
            if (_audioSource != null && _tower.Data.AttackSound != null)
            {
                _audioSource.PlayOneShot(_tower.Data.AttackSound);
            }

            // Muzzle flash
            if (_tower.Data.MuzzleFlashPrefab != null)
            {
                Transform firePoint = _tower.GetFirePoint();
                GameObject flash = Instantiate(_tower.Data.MuzzleFlashPrefab, firePoint.position, firePoint.rotation);
                Destroy(flash, 0.5f);
            }
        }
    }
}
```

### Update Tower Prefab

Add to BasicTower prefab:
1. Add TowerAttack component
2. Add AudioSource component (optional, for attack sounds)

### Scene Setup

Add ProjectilePoolManager:
1. Create ProjectilePoolManager GameObject under --- MANAGEMENT ---
2. Add ProjectilePoolManager component

## Testing and Acceptance Criteria

### Manual Test Steps

1. Place tower in scene
2. Spawn enemy in range
3. Verify tower aims at enemy
4. Verify tower fires projectiles at attack speed interval
5. Verify projectiles hit enemy and deal damage
6. Verify enemy dies when health depleted
7. Verify tower retargets when enemy dies

### Done When

- [ ] TowerAttack component fires projectiles
- [ ] Attack respects attack speed interval
- [ ] Attack waits for aiming (if required)
- [ ] Projectiles spawn at fire point
- [ ] Projectiles return to pool after hit
- [ ] Attack sound plays (if configured)
- [ ] Muzzle flash spawns (if configured)
- [ ] Direct damage works when no projectile prefab

## Dependencies

- Issue 14: Targeting System
- Issue 15: Projectile System
