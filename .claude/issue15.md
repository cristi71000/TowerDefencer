## Context

Towers need projectiles to damage enemies. This issue implements the projectile system with a base projectile component, projectile pooling, and basic straight-line movement toward targets.

**Builds upon:** Issues 6, 14 (Tower, Targeting)

## Detailed Implementation Instructions

### Projectile Component

Create `Projectile.cs` in `_Project/Scripts/Runtime/Towers/`:

```csharp
using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Towers
{
    public class Projectile : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _speed = 15f;
        [SerializeField] private float _lifetime = 5f;
        [SerializeField] private bool _trackTarget = true;

        private Enemy _target;
        private Vector3 _targetPosition;
        private int _damage;
        private float _aoeRadius;
        private float _lifeTimer;
        private bool _isActive;

        public event System.Action<Projectile> OnHit;
        public event System.Action<Projectile> OnExpired;

        public void Initialize(Enemy target, int damage, float speed, float aoeRadius = 0f)
        {
            _target = target;
            _targetPosition = target != null ? target.TargetPoint.position : transform.position + transform.forward * 10f;
            _damage = damage;
            _speed = speed;
            _aoeRadius = aoeRadius;
            _lifeTimer = 0f;
            _isActive = true;
        }

        private void Update()
        {
            if (!_isActive) return;

            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= _lifetime)
            {
                Expire();
                return;
            }

            MoveTowardTarget();
        }

        private void MoveTowardTarget()
        {
            Vector3 targetPos = _trackTarget && _target != null && !_target.IsDead
                ? _target.TargetPoint.position
                : _targetPosition;

            Vector3 direction = (targetPos - transform.position).normalized;
            transform.position += direction * _speed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direction);

            // Check if reached target
            if (Vector3.Distance(transform.position, targetPos) < 0.5f)
            {
                Hit();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;

            Enemy enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null && !enemy.IsDead)
            {
                _target = enemy;
                Hit();
            }
        }

        private void Hit()
        {
            _isActive = false;

            if (_aoeRadius > 0)
            {
                DealAOEDamage();
            }
            else
            {
                DealSingleTargetDamage();
            }

            OnHit?.Invoke(this);
        }

        private void DealSingleTargetDamage()
        {
            if (_target != null && !_target.IsDead)
            {
                _target.TakeDamage(_damage);
            }
        }

        private void DealAOEDamage()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, _aoeRadius, LayerMask.GetMask("Enemy"));
            foreach (var hit in hits)
            {
                Enemy enemy = hit.GetComponentInParent<Enemy>();
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.TakeDamage(_damage);
                }
            }
        }

        private void Expire()
        {
            _isActive = false;
            OnExpired?.Invoke(this);
        }

        public void Reset()
        {
            _target = null;
            _isActive = false;
            _lifeTimer = 0f;
            OnHit = null;
            OnExpired = null;
        }
    }
}
```

### Projectile Pool Manager

Create `ProjectilePoolManager.cs` in `_Project/Scripts/Runtime/Towers/`:

```csharp
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Towers
{
    public class ProjectilePoolManager : MonoBehaviour
    {
        public static ProjectilePoolManager Instance { get; private set; }

        [SerializeField] private int _defaultPoolSize = 30;

        private Dictionary<GameObject, ObjectPool<Projectile>> _pools;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            _pools = new Dictionary<GameObject, ObjectPool<Projectile>>();
        }

        public Projectile GetProjectile(GameObject prefab)
        {
            if (!_pools.ContainsKey(prefab))
            {
                var projComponent = prefab.GetComponent<Projectile>();
                _pools[prefab] = new ObjectPool<Projectile>(projComponent, _defaultPoolSize, transform);
            }

            var proj = _pools[prefab].Get();
            proj.Reset();
            return proj;
        }

        public void ReturnProjectile(Projectile projectile, GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out var pool))
            {
                projectile.Reset();
                pool.Return(projectile);
            }
            else
            {
                Destroy(projectile.gameObject);
            }
        }
    }
}
```

### Basic Projectile Prefab

Create prefab structure:
```
BasicProjectile (Projectile.cs, SphereCollider-trigger, Rigidbody-kinematic)
+-- Model (Sphere scaled 0.2, 0.2, 0.2)
```

Setup:
1. Create Sphere primitive, scale (0.2, 0.2, 0.2)
2. Add Projectile component
3. Add SphereCollider (IsTrigger = true, Radius = 0.15)
4. Add Rigidbody (IsKinematic = true, UseGravity = false)
5. Set Layer to "Projectile" (Layer 9)
6. Create material M_Projectile - bright yellow/white

Save as `_Project/Prefabs/Projectiles/BasicProjectile.prefab`

### Update TowerData

Assign BasicProjectile prefab to TD_BasicTower.asset ProjectilePrefab field.

## Testing and Acceptance Criteria

### Done When

- [ ] Projectile prefab created with proper components
- [ ] Projectile moves toward target
- [ ] Projectile damages enemy on hit
- [ ] Projectile tracks moving targets
- [ ] AOE damage hits multiple enemies
- [ ] Projectiles return to pool after hit
- [ ] Projectiles expire after lifetime

## Dependencies

- Issue 6: Tower Data (ProjectilePrefab reference)
- Issue 14: Targeting System
