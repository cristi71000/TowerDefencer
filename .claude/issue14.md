## Context

Towers need to detect and select targets within their range. This issue implements the targeting system that finds enemies based on priority (First, Nearest, Strongest, Weakest, Fastest) and tracks them for attacking. This is the foundation of tower combat.

**Builds upon:** Issues 6, 9-11 (Tower, Enemy systems)

## Detailed Implementation Instructions

### ITargetable Interface

Create `ITargetable.cs` in `_Project/Scripts/Runtime/Core/`:

```csharp
using UnityEngine;

namespace TowerDefense.Core
{
    public interface ITargetable
    {
        Transform TargetPoint { get; }
        bool IsValidTarget { get; }
        int CurrentHealth { get; }
        float DistanceTraveled { get; }
        float CurrentSpeed { get; }
    }
}
```

### Tower Targeting Component

Create `TowerTargeting.cs` in `_Project/Scripts/Runtime/Towers/`:

```csharp
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Towers
{
    public class TowerTargeting : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _detectionInterval = 0.1f;
        [SerializeField] private LayerMask _enemyLayer;

        private Tower _tower;
        private Enemy _currentTarget;
        private float _detectionTimer;
        private List<Enemy> _enemiesInRange = new List<Enemy>();
        private Collider[] _overlapResults = new Collider[50];

        public Enemy CurrentTarget => _currentTarget;
        public bool HasTarget => _currentTarget != null && !_currentTarget.IsDead;
        public List<Enemy> EnemiesInRange => _enemiesInRange;

        private void Awake()
        {
            _tower = GetComponent<Tower>();
        }

        private void Update()
        {
            _detectionTimer += Time.deltaTime;
            if (_detectionTimer >= _detectionInterval)
            {
                _detectionTimer = 0f;
                UpdateTargets();
            }

            ValidateCurrentTarget();
        }

        private void UpdateTargets()
        {
            _enemiesInRange.Clear();
            float range = _tower.Data.Range;

            int count = Physics.OverlapSphereNonAlloc(
                transform.position,
                range,
                _overlapResults,
                _enemyLayer
            );

            for (int i = 0; i < count; i++)
            {
                Enemy enemy = _overlapResults[i].GetComponentInParent<Enemy>();
                if (enemy != null && !enemy.IsDead)
                {
                    _enemiesInRange.Add(enemy);
                }
            }

            SelectTarget();
        }

        private void ValidateCurrentTarget()
        {
            if (_currentTarget == null) return;

            if (_currentTarget.IsDead || !IsInRange(_currentTarget))
            {
                _currentTarget = null;
                SelectTarget();
            }
        }

        private bool IsInRange(Enemy enemy)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            return distance <= _tower.Data.Range;
        }

        private void SelectTarget()
        {
            if (_enemiesInRange.Count == 0)
            {
                _currentTarget = null;
                return;
            }

            _currentTarget = GetBestTarget(_tower.CurrentPriority);
        }

        private Enemy GetBestTarget(TargetingPriority priority)
        {
            if (_enemiesInRange.Count == 0) return null;

            return priority switch
            {
                TargetingPriority.First => GetFirstTarget(),
                TargetingPriority.Nearest => GetNearestTarget(),
                TargetingPriority.Strongest => GetStrongestTarget(),
                TargetingPriority.Weakest => GetWeakestTarget(),
                TargetingPriority.Fastest => GetFastestTarget(),
                _ => GetFirstTarget()
            };
        }

        private Enemy GetFirstTarget()
        {
            Enemy best = null;
            float maxDistance = -1f;

            foreach (var enemy in _enemiesInRange)
            {
                if (enemy.DistanceTraveled > maxDistance)
                {
                    maxDistance = enemy.DistanceTraveled;
                    best = enemy;
                }
            }
            return best;
        }

        private Enemy GetNearestTarget()
        {
            Enemy best = null;
            float minDist = float.MaxValue;

            foreach (var enemy in _enemiesInRange)
            {
                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    best = enemy;
                }
            }
            return best;
        }

        private Enemy GetStrongestTarget()
        {
            Enemy best = null;
            int maxHealth = -1;

            foreach (var enemy in _enemiesInRange)
            {
                if (enemy.CurrentHealth > maxHealth)
                {
                    maxHealth = enemy.CurrentHealth;
                    best = enemy;
                }
            }
            return best;
        }

        private Enemy GetWeakestTarget()
        {
            Enemy best = null;
            int minHealth = int.MaxValue;

            foreach (var enemy in _enemiesInRange)
            {
                if (enemy.CurrentHealth < minHealth)
                {
                    minHealth = enemy.CurrentHealth;
                    best = enemy;
                }
            }
            return best;
        }

        private Enemy GetFastestTarget()
        {
            Enemy best = null;
            float maxSpeed = -1f;

            foreach (var enemy in _enemiesInRange)
            {
                if (enemy.Data.MoveSpeed > maxSpeed)
                {
                    maxSpeed = enemy.Data.MoveSpeed;
                    best = enemy;
                }
            }
            return best;
        }

        public void ForceRetarget()
        {
            _currentTarget = null;
            UpdateTargets();
        }
    }
}
```

### Tower Aiming Component

Create `TowerAiming.cs` in `_Project/Scripts/Runtime/Towers/`:

```csharp
using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Towers
{
    public class TowerAiming : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private bool _predictTargetMovement = true;

        private Tower _tower;
        private TowerTargeting _targeting;

        private void Awake()
        {
            _tower = GetComponent<Tower>();
            _targeting = GetComponent<TowerTargeting>();
        }

        private void Update()
        {
            if (_targeting == null || !_targeting.HasTarget) return;

            AimAtTarget(_targeting.CurrentTarget);
        }

        private void AimAtTarget(Enemy target)
        {
            Transform turret = _tower.GetTurretPivot();
            if (turret == null) return;

            Vector3 targetPos = _predictTargetMovement
                ? PredictPosition(target)
                : target.TargetPoint.position;

            Vector3 direction = targetPos - turret.position;
            direction.y = 0; // Keep turret level

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                turret.rotation = Quaternion.Slerp(
                    turret.rotation,
                    targetRotation,
                    Time.deltaTime * _rotationSpeed
                );
            }
        }

        private Vector3 PredictPosition(Enemy target)
        {
            if (_tower.Data == null) return target.TargetPoint.position;

            float projectileSpeed = _tower.Data.ProjectileSpeed;
            if (projectileSpeed <= 0) return target.TargetPoint.position;

            Vector3 targetPos = target.TargetPoint.position;
            Vector3 targetVelocity = target.GetComponent<UnityEngine.AI.NavMeshAgent>()?.velocity ?? Vector3.zero;

            float distance = Vector3.Distance(_tower.GetFirePoint().position, targetPos);
            float timeToHit = distance / projectileSpeed;

            return targetPos + targetVelocity * timeToHit;
        }

        public bool IsAimedAtTarget()
        {
            if (!_targeting.HasTarget) return false;

            Transform turret = _tower.GetTurretPivot();
            Vector3 toTarget = (_targeting.CurrentTarget.TargetPoint.position - turret.position).normalized;
            toTarget.y = 0;
            toTarget.Normalize();

            Vector3 forward = turret.forward;
            forward.y = 0;
            forward.Normalize();

            return Vector3.Dot(forward, toTarget) > 0.95f;
        }
    }
}
```

### Update Tower Prefab

Add components to BasicTower prefab:
1. Add TowerTargeting component
2. Add TowerAiming component
3. Configure:
   - Enemy Layer: Layer 8 (Enemy)
   - Detection Interval: 0.1

### Update Enemy to Implement ITargetable

```csharp
// Add to Enemy.cs class declaration:
public class Enemy : MonoBehaviour, ITargetable
{
    public bool IsValidTarget => !_isDead && !_reachedEnd;
    // TargetPoint, CurrentHealth, DistanceTraveled, CurrentSpeed already implemented
}
```

## Testing and Acceptance Criteria

### Done When

- [ ] TowerTargeting detects enemies in range
- [ ] All 5 targeting priorities work correctly
- [ ] Tower turret rotates toward target
- [ ] Target prediction accounts for enemy movement
- [ ] Target lost when enemy dies or exits range
- [ ] Automatic retargeting when target lost
- [ ] No errors when no enemies present

## Dependencies

- Issue 6: Tower Prefab
- Issues 9-11: Enemy systems
