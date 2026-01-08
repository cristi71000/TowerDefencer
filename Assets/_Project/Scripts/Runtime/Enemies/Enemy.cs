using UnityEngine;
using UnityEngine.AI;
using TowerDefense.Core;
using TowerDefense.UI;

namespace TowerDefense.Enemies
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class Enemy : MonoBehaviour, ITargetable
    {
        [Header("Data")]
        [SerializeField] private EnemyData _enemyData;

        [Header("References")]
        [SerializeField] private Transform _healthBarAnchor;
        [SerializeField] private GameObject _healthBarPrefab;
        [SerializeField] private EnemyDeathEffect _deathEffect;
        [SerializeField] private Transform _targetPoint;

        private NavMeshAgent _navMeshAgent;
        private StatusEffectManager _statusEffectManager;
        private EnemyHealthBar _healthBar;
        private float _currentHealth;
        private bool _isDead;
        private bool _hasReachedEnd;
        private float _distanceTraveled;
        private Vector3 _lastPosition;
        private float _originalSpeed;
        private float _currentSlowAmount;

        public EnemyData Data => _enemyData;
        public float CurrentHealthFloat => _currentHealth;
        public float MaxHealth => _enemyData != null ? _enemyData.MaxHealth : 0f;
        public float Armor => _enemyData != null ? _enemyData.Armor : 0f;
        public float HealthPercent => MaxHealth > 0 ? _currentHealth / MaxHealth : 0f;
        public bool IsDead => _isDead;
        public bool HasReachedEnd => _hasReachedEnd;
        public Transform HealthBarAnchor => _healthBarAnchor;

        #region ITargetable Implementation

        /// <summary>
        /// The transform point to aim at. Falls back to this transform if not set.
        /// </summary>
        public Transform TargetPoint => _targetPoint != null ? _targetPoint : transform;

        /// <summary>
        /// Whether this enemy is a valid target (alive and not at end).
        /// </summary>
        public bool IsValidTarget => !_isDead && !_hasReachedEnd;

        /// <summary>
        /// Current health as integer for targeting priority calculations.
        /// </summary>
        int ITargetable.CurrentHealth => Mathf.RoundToInt(_currentHealth);

        /// <summary>
        /// Distance traveled along the path (for First priority targeting).
        /// </summary>
        public float DistanceTraveled => _distanceTraveled;

        /// <summary>
        /// Current movement speed from NavMeshAgent velocity.
        /// </summary>
        public float CurrentSpeed
        {
            get
            {
                if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.hasPath)
                {
                    return _navMeshAgent.velocity.magnitude;
                }
                return 0f;
            }
        }

        /// <summary>
        /// Current velocity vector for lead prediction.
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.hasPath)
                {
                    return _navMeshAgent.velocity;
                }
                return Vector3.zero;
            }
        }

        #endregion

        public event System.Action<Enemy> OnDeath;
        public event System.Action<Enemy> OnReachedEnd;
        /// <summary>
        /// Event fired when enemy takes damage. Parameters: Enemy, damage amount, isCritical.
        /// </summary>
        public event System.Action<Enemy, float, bool> OnDamageTakenWithCrit;
        /// <summary>
        /// Legacy event for backwards compatibility. Parameters: Enemy, damage amount.
        /// </summary>
        public event System.Action<Enemy, float> OnDamageTaken;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _statusEffectManager = GetComponent<StatusEffectManager>();
            // Cache original speed for slow effect restoration
            if (_navMeshAgent != null)
            {
                _originalSpeed = _navMeshAgent.speed;
            }
        }

        private void Start()
        {
            _lastPosition = transform.position;
        }

        private void Update()
        {
            // Track distance traveled for "First" targeting priority
            // Only accumulate distance when the enemy is actually moving to avoid unnecessary calculations
            if (!_isDead && !_hasReachedEnd && CurrentSpeed > 0.01f)
            {
                float frameDistance = Vector3.Distance(transform.position, _lastPosition);
                _distanceTraveled += frameDistance;
                _lastPosition = transform.position;
            }
        }

        public void Initialize(EnemyData data)
        {
            if (data == null)
            {
                UnityEngine.Debug.LogError($"Enemy.Initialize called with null data on {gameObject.name}");
                return;
            }

            _enemyData = data;
            _currentHealth = data.MaxHealth;
            _isDead = false;
            _hasReachedEnd = false;
            _distanceTraveled = 0f;
            _lastPosition = transform.position;

            if (_navMeshAgent != null)
            {
                _navMeshAgent.speed = data.MoveSpeed;
                _originalSpeed = data.MoveSpeed;
                _currentSlowAmount = 0f;
                _navMeshAgent.enabled = !data.IsFlying;
            }

            // Initialize health bar
            EnsureHealthBar();
            if (_healthBar != null)
            {
                _healthBar.Initialize(HealthPercent);
            }
        }

        /// <summary>
        /// Resets the enemy state for object pool reuse.
        /// Call this when returning the enemy to the pool.
        /// </summary>
        public void ResetEnemy()
        {
            // Reset status effects first to prevent pool corruption
            _statusEffectManager?.ResetManager();

            _currentHealth = 0f;
            _isDead = false;
            _hasReachedEnd = false;
            _enemyData = null;
            _distanceTraveled = 0f;
            _currentSlowAmount = 0f;

            // Clear event subscribers to prevent stale references
            OnDeath = null;
            OnReachedEnd = null;
            OnDamageTaken = null;
            OnDamageTakenWithCrit = null;

            // Reset NavMeshAgent state
            if (_navMeshAgent != null)
            {
                _navMeshAgent.speed = _originalSpeed;
                _navMeshAgent.enabled = false;
                _navMeshAgent.isStopped = false;
            }

            // Reset health bar for pool reuse
            if (_healthBar != null)
            {
                _healthBar.ResetHealthBar();
            }
        }

        /// <summary>
        /// Takes damage using the new DamageInfo system.
        /// Applies armor reduction based on damage type.
        /// </summary>
        /// <param name="damageInfo">The damage information including amount, type, and critical status.</param>
        public void TakeDamage(DamageInfo damageInfo)
        {
            if (_isDead || _hasReachedEnd) return;

            // Apply armor reduction based on damage type
            float finalDamage = DamageCalculator.ApplyArmorReduction(damageInfo.Amount, Armor, damageInfo.Type);

            _currentHealth -= finalDamage;

            // Fire both events for compatibility
            OnDamageTakenWithCrit?.Invoke(this, finalDamage, damageInfo.IsCritical);
            OnDamageTaken?.Invoke(this, finalDamage);

            // Spawn floating damage number
            Vector3 hitPosition = damageInfo.HitPoint != Vector3.zero ? damageInfo.HitPoint : transform.position;
            if (DamageNumberSpawner.Instance != null)
            {
                DamageNumberSpawner.Instance.SpawnDamageNumber(hitPosition, finalDamage, damageInfo.IsCritical);
            }

            // Update health bar
            if (_healthBar != null)
            {
                _healthBar.UpdateHealth(HealthPercent);
            }

            if (_currentHealth <= 0f)
            {
                _currentHealth = 0f;
                Die();
            }
        }

        /// <summary>
        /// Takes raw damage without armor calculation.
        /// Maintained for backwards compatibility. Delegates to TakeDamage(DamageInfo).
        /// </summary>
        /// <param name="damage">The raw damage amount to apply.</param>
        public void TakeDamage(float damage)
        {
            // Delegate to TakeDamage(DamageInfo) to avoid code duplication
            // and ensure damage numbers spawn for legacy calls too.
            // Note: Using True damage type to bypass armor since legacy callers
            // expect raw damage to be applied directly.
            TakeDamage(new DamageInfo(damage, null, transform.position, false, DamageType.True));
        }

        /// <summary>
        /// Applies a slow effect to the enemy, reducing movement speed.
        /// Only applies if the new slow is stronger than the current slow.
        /// </summary>
        /// <param name="slowAmount">The slow amount (0-1 range). 0 = no slow, 1 = stopped.</param>
        /// <param name="duration">The duration of the slow effect (used by StatusEffectManager).</param>
        public void ApplySlow(float slowAmount, float duration)
        {
            if (_navMeshAgent == null) return;

            slowAmount = Mathf.Clamp01(slowAmount);
            if (slowAmount > _currentSlowAmount)
            {
                _currentSlowAmount = slowAmount;
                _navMeshAgent.speed = _originalSpeed * (1f - _currentSlowAmount);
            }
        }

        /// <summary>
        /// Removes the slow effect, restoring the enemy's original movement speed.
        /// </summary>
        public void RemoveSlow()
        {
            if (_navMeshAgent == null) return;

            _currentSlowAmount = 0f;
            _navMeshAgent.speed = _originalSpeed;
        }

        public void Die()
        {
            if (_isDead) return;

            _isDead = true;

            if (_navMeshAgent != null)
            {
                _navMeshAgent.enabled = false;
            }

            // Hide health bar on death
            if (_healthBar != null)
            {
                _healthBar.Hide();
            }

            OnDeath?.Invoke(this);

            // Play death effect via EnemyDeathEffect component
            _deathEffect?.PlayDeathEffect(transform.position, 1f);

            // Spawn death VFX if configured in EnemyData (fallback)
            if (_deathEffect == null && _enemyData != null && _enemyData.DeathVFXPrefab != null)
            {
                GameObject vfx = Instantiate(_enemyData.DeathVFXPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 2f); // Auto-cleanup after 2 seconds
            }

            // Note: Do NOT destroy the gameObject here - let the pool manager handle it
        }

        /// <summary>
        /// Called when the enemy reaches the exit point.
        /// </summary>
        public void ReachEnd()
        {
            if (_isDead || _hasReachedEnd) return;

            _hasReachedEnd = true;

            if (_navMeshAgent != null)
            {
                _navMeshAgent.enabled = false;
            }

            // Hide health bar when reaching end
            if (_healthBar != null)
            {
                _healthBar.Hide();
            }

            OnReachedEnd?.Invoke(this);

            // Note: Do NOT destroy the gameObject here - let the pool manager handle it
        }

        public void SetDestination(Vector3 destination)
        {
            if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.SetDestination(destination);
            }
        }

        public void StopMovement()
        {
            if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.isStopped = true;
            }
        }

        public void ResumeMovement()
        {
            if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.isStopped = false;
            }
        }

        /// <summary>
        /// Ensures the health bar is instantiated from the prefab if not already present.
        /// </summary>
        private void EnsureHealthBar()
        {
            if (_healthBar != null) return;

            if (_healthBarPrefab == null)
            {
                UnityEngine.Debug.LogWarning($"Health bar prefab not assigned on {gameObject.name}");
                return;
            }

            Transform anchorTransform = _healthBarAnchor != null ? _healthBarAnchor : transform;
            GameObject healthBarInstance = Instantiate(_healthBarPrefab, anchorTransform.position, Quaternion.identity, anchorTransform);
            healthBarInstance.transform.localPosition = Vector3.zero;
            _healthBar = healthBarInstance.GetComponent<EnemyHealthBar>();

            if (_healthBar == null)
            {
                UnityEngine.Debug.LogError($"Health bar prefab does not have EnemyHealthBar component on {gameObject.name}");
                Destroy(healthBarInstance);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw health bar anchor position
            if (_healthBarAnchor != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_healthBarAnchor.position, 0.1f);
            }

            // Draw target point
            Transform tp = _targetPoint != null ? _targetPoint : transform;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(tp.position, 0.15f);
        }
    }
}
