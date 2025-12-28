## Context

Enemies are the core challenge in tower defense games. Before implementing spawning or combat, we need the EnemyData ScriptableObject to define enemy properties and a basic enemy prefab with NavMeshAgent for pathfinding. This establishes the data-driven foundation for all enemy types.

**Builds upon:** Issue 4 (Test Level Blockout with path)

## Detailed Implementation Instructions

### EnemyData ScriptableObject

Create `EnemyData.cs` in `_Project/Scripts/Runtime/Enemies/`:

```csharp
using UnityEngine;

namespace TowerDefense.Enemies
{
    public enum EnemyType
    {
        Basic,
        Fast,
        Tank,
        Flying,
        Swarm,
        Boss
    }

    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "TD/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string EnemyName;
        public EnemyType Type;
        public Sprite Icon;

        [Header("Prefab")]
        public GameObject Prefab;

        [Header("Stats")]
        public int MaxHealth = 100;
        public float MoveSpeed = 3f;
        public int Armor = 0; // Flat damage reduction

        [Header("Rewards")]
        public int CurrencyReward = 10;
        public int ScoreValue = 100;

        [Header("Damage to Player")]
        public int LivesDamage = 1; // Lives lost when reaching exit

        [Header("Special Properties")]
        public bool IsFlying = false;
        public bool IsImmuneToSlow = false;
        public float SlowResistance = 0f; // 0-1, reduces slow effectiveness

        [Header("Visual Scale")]
        public float ModelScale = 1f;

        [Header("Audio")]
        public AudioClip SpawnSound;
        public AudioClip DeathSound;
        public AudioClip HitSound;
    }
}
```

### Enemy Component

Create `Enemy.cs` in `_Project/Scripts/Runtime/Enemies/`:

```csharp
using UnityEngine;
using UnityEngine.AI;
using TowerDefense.Core;

namespace TowerDefense.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private EnemyData _enemyData;

        [Header("References")]
        [SerializeField] private Transform _targetPoint; // For tower targeting

        // Components
        private NavMeshAgent _navAgent;

        // Runtime state
        private int _currentHealth;
        private float _currentSpeed;
        private float _distanceTraveled;
        private bool _isDead;
        private bool _reachedEnd;

        // Slow effect
        private float _slowMultiplier = 1f;
        private float _slowTimer;

        // Public accessors
        public EnemyData Data => _enemyData;
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _enemyData.MaxHealth;
        public float HealthPercent => (float)_currentHealth / _enemyData.MaxHealth;
        public bool IsDead => _isDead;
        public bool IsFlying => _enemyData.IsFlying;
        public float DistanceTraveled => _distanceTraveled;
        public float CurrentSpeed => _navAgent != null ? _navAgent.velocity.magnitude : 0f;
        public Transform TargetPoint => _targetPoint != null ? _targetPoint : transform;

        // Events
        public event System.Action<Enemy> OnDeath;
        public event System.Action<Enemy> OnReachedEnd;
        public event System.Action<Enemy, int, int> OnHealthChanged; // enemy, current, max

        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
        }

        public void Initialize(EnemyData data)
        {
            _enemyData = data;
            _currentHealth = data.MaxHealth;
            _currentSpeed = data.MoveSpeed;
            _isDead = false;
            _reachedEnd = false;
            _distanceTraveled = 0f;

            // Configure NavMeshAgent
            _navAgent.speed = _currentSpeed;
            _navAgent.acceleration = 100f; // Quick acceleration
            _navAgent.angularSpeed = 720f;
            _navAgent.stoppingDistance = 0.5f;

            // Apply visual scale
            transform.localScale = Vector3.one * data.ModelScale;

            // Set destination to exit
            if (ExitPoint.Instance != null)
            {
                _navAgent.SetDestination(ExitPoint.Instance.transform.position);
            }
        }

        private void Update()
        {
            if (_isDead || _reachedEnd) return;

            UpdateSlowEffect();
            UpdateDistanceTraveled();
            CheckReachedEnd();
        }

        private void UpdateSlowEffect()
        {
            if (_slowTimer > 0)
            {
                _slowTimer -= Time.deltaTime;
                if (_slowTimer <= 0)
                {
                    _slowMultiplier = 1f;
                    UpdateSpeed();
                }
            }
        }

        private void UpdateDistanceTraveled()
        {
            _distanceTraveled += _navAgent.velocity.magnitude * Time.deltaTime;
        }

        private void CheckReachedEnd()
        {
            if (_navAgent.remainingDistance <= _navAgent.stoppingDistance && !_navAgent.pathPending)
            {
                ReachEnd();
            }
        }

        public void TakeDamage(int damage)
        {
            if (_isDead) return;

            // Apply armor
            int actualDamage = Mathf.Max(1, damage - _enemyData.Armor);
            _currentHealth -= actualDamage;

            OnHealthChanged?.Invoke(this, _currentHealth, _enemyData.MaxHealth);

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void ApplySlow(float slowAmount, float duration)
        {
            if (_enemyData.IsImmuneToSlow) return;

            // Apply slow resistance
            float effectiveSlow = slowAmount * (1f - _enemyData.SlowResistance);

            // Only apply if stronger than current slow
            float newMultiplier = 1f - effectiveSlow;
            if (newMultiplier < _slowMultiplier)
            {
                _slowMultiplier = newMultiplier;
                _slowTimer = duration;
                UpdateSpeed();
            }
        }

        private void UpdateSpeed()
        {
            _navAgent.speed = _currentSpeed * _slowMultiplier;
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            _navAgent.isStopped = true;

            OnDeath?.Invoke(this);

            // Will be destroyed by spawner/pool manager
        }

        private void ReachEnd()
        {
            if (_reachedEnd || _isDead) return;
            _reachedEnd = true;

            _navAgent.isStopped = true;

            OnReachedEnd?.Invoke(this);

            // Will be destroyed by spawner/pool manager
        }

        // Called when returning to pool
        public void ResetEnemy()
        {
            _isDead = false;
            _reachedEnd = false;
            _distanceTraveled = 0f;
            _slowMultiplier = 1f;
            _slowTimer = 0f;
            _navAgent.isStopped = false;

            // Clear events (pool reuse safety)
            OnDeath = null;
            OnReachedEnd = null;
            OnHealthChanged = null;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw path
            if (_navAgent != null && _navAgent.hasPath)
            {
                Gizmos.color = Color.yellow;
                Vector3[] corners = _navAgent.path.corners;
                for (int i = 0; i < corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(corners[i], corners[i + 1]);
                }
            }
        }
    }
}
```

### Health Component (Separate for Reusability)

Create `Health.cs` in `_Project/Scripts/Runtime/Core/`:

```csharp
using UnityEngine;

namespace TowerDefense.Core
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _armor = 0;

        private int _currentHealth;

        public int MaxHealth => _maxHealth;
        public int CurrentHealth => _currentHealth;
        public float HealthPercent => (float)_currentHealth / _maxHealth;
        public bool IsDead => _currentHealth <= 0;

        public event System.Action<int, int> OnHealthChanged; // current, max
        public event System.Action OnDeath;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        public void Initialize(int maxHealth, int armor = 0)
        {
            _maxHealth = maxHealth;
            _armor = armor;
            _currentHealth = _maxHealth;
        }

        public void TakeDamage(int damage)
        {
            if (IsDead) return;

            int actualDamage = Mathf.Max(1, damage - _armor);
            _currentHealth = Mathf.Max(0, _currentHealth - actualDamage);

            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (_currentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            if (IsDead) return;

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void Reset()
        {
            _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
    }
}
```

### IDamageable Interface

Create `IDamageable.cs` in `_Project/Scripts/Runtime/Core/`:

```csharp
namespace TowerDefense.Core
{
    public interface IDamageable
    {
        void TakeDamage(int damage);
        int CurrentHealth { get; }
        int MaxHealth { get; }
        bool IsDead { get; }
    }
}
```

### Basic Enemy Prefab Structure

```
BasicEnemy (Enemy.cs, NavMeshAgent, CapsuleCollider)
|-- Model
|   +-- Body (Capsule scaled 0.5, 0.75, 0.5)
|-- TargetPoint (empty, at center mass)
+-- HealthBarAnchor (empty, above head for UI)
```

### Prefab Setup Instructions

1. Create empty GameObject named "BasicEnemy"
2. Add Enemy component
3. Add NavMeshAgent component:
   - Speed: 3 (will be overridden by data)
   - Angular Speed: 720
   - Acceleration: 100
   - Stopping Distance: 0.5
   - Auto Braking: true
4. Add CapsuleCollider:
   - Radius: 0.4
   - Height: 1.5
   - Center: (0, 0.75, 0)
5. Set Layer to "Enemy" (Layer 8)
6. Add tag "Enemy"

**Model:**
- Create child named "Model"
- Add Capsule primitive as child named "Body"
- Scale: (0.5, 0.75, 0.5)
- Position: (0, 0.75, 0)
- Create material M_Enemy_Basic - red color RGB(200, 50, 50)

**Target Point:**
- Create empty child named "TargetPoint"
- Position: (0, 0.75, 0) - center mass

**Health Bar Anchor:**
- Create empty child named "HealthBarAnchor"
- Position: (0, 1.8, 0) - above head

7. Wire up Enemy component:
   - Target Point reference

8. Save as `_Project/Prefabs/Enemies/BasicEnemy.prefab`

### Create Basic Enemy Data Asset

Create at `_Project/ScriptableObjects/EnemyData/ED_BasicEnemy.asset`:

- Enemy Name: "Basic Enemy"
- Type: Basic
- Prefab: BasicEnemy prefab
- Max Health: 100
- Move Speed: 3
- Armor: 0
- Currency Reward: 10
- Lives Damage: 1
- Is Flying: false
- Model Scale: 1

## Testing and Acceptance Criteria

### Manual Test Steps

1. Open BasicEnemy prefab in Prefab Mode
2. Verify hierarchy matches specification
3. Verify NavMeshAgent configured correctly
4. Verify collider size and position
5. Place enemy in scene manually:
   - Position at spawn point
   - Enter play mode
   - Verify enemy navigates toward exit
6. Verify enemy stops at exit point
7. Call TakeDamage(50) via debug - verify health decreases
8. Call TakeDamage(100) - verify death state

### Done When

- [ ] EnemyData ScriptableObject created with all fields
- [ ] Enemy component created with health and movement
- [ ] IDamageable interface implemented
- [ ] NavMeshAgent configured for pathfinding
- [ ] BasicEnemy prefab on correct layer (8) with tag
- [ ] ED_BasicEnemy asset created
- [ ] Enemy moves toward exit when placed in scene
- [ ] TakeDamage reduces health correctly
- [ ] Armor reduces damage (minimum 1)
- [ ] Slow effect works and respects immunity
- [ ] Death and ReachEnd events fire correctly
- [ ] Gizmo shows navigation path when selected

## Dependencies

- Issue 4: Test Level Blockout (ExitPoint, path)

## Notes

- NavMesh must be baked on the level before enemies can navigate (Issue 10)
- Flying enemies will need separate handling (ignore NavMesh obstacles)
- The TargetPoint is where towers aim - center mass, not feet
- HealthBarAnchor will be used for world-space health UI
- Pool-friendly design with ResetEnemy() method
