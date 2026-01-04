using UnityEngine;
using UnityEngine.AI;

namespace TowerDefense.Enemies
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class Enemy : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private EnemyData _enemyData;

        [Header("References")]
        [SerializeField] private Transform _healthBarAnchor;
        [SerializeField] private GameObject _healthBarPrefab;
        [SerializeField] private EnemyDeathEffect _deathEffect;

        private NavMeshAgent _navMeshAgent;
        private EnemyHealthBar _healthBar;
        private float _currentHealth;
        private bool _isDead;
        private bool _hasReachedEnd;

        public EnemyData Data => _enemyData;
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _enemyData != null ? _enemyData.MaxHealth : 0f;
        public float HealthPercent => MaxHealth > 0 ? _currentHealth / MaxHealth : 0f;
        public bool IsDead => _isDead;
        public bool HasReachedEnd => _hasReachedEnd;
        public Transform HealthBarAnchor => _healthBarAnchor;

        public event System.Action<Enemy> OnDeath;
        public event System.Action<Enemy> OnReachedEnd;
        public event System.Action<Enemy, float> OnDamageTaken;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        public void Initialize(EnemyData data)
        {
            if (data == null)
            {
                Debug.LogError($"Enemy.Initialize called with null data on {gameObject.name}");
                return;
            }

            _enemyData = data;
            _currentHealth = data.MaxHealth;
            _isDead = false;
            _hasReachedEnd = false;

            if (_navMeshAgent != null)
            {
                _navMeshAgent.speed = data.MoveSpeed;
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
            _currentHealth = 0f;
            _isDead = false;
            _hasReachedEnd = false;
            _enemyData = null;

            // Clear event subscribers to prevent stale references
            OnDeath = null;
            OnReachedEnd = null;
            OnDamageTaken = null;

            // Reset NavMeshAgent state
            if (_navMeshAgent != null)
            {
                _navMeshAgent.enabled = false;
                _navMeshAgent.isStopped = false;
            }

            // Reset health bar for pool reuse
            if (_healthBar != null)
            {
                _healthBar.ResetHealthBar();
            }
        }

        public void TakeDamage(float damage)
        {
            if (_isDead || _hasReachedEnd) return;

            _currentHealth -= damage;
            OnDamageTaken?.Invoke(this, damage);

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
                Debug.LogWarning($"Health bar prefab not assigned on {gameObject.name}");
                return;
            }

            Transform anchorTransform = _healthBarAnchor != null ? _healthBarAnchor : transform;
            GameObject healthBarInstance = Instantiate(_healthBarPrefab, anchorTransform.position, Quaternion.identity, anchorTransform);
            healthBarInstance.transform.localPosition = Vector3.zero;
            _healthBar = healthBarInstance.GetComponent<EnemyHealthBar>();

            if (_healthBar == null)
            {
                Debug.LogError($"Health bar prefab does not have EnemyHealthBar component on {gameObject.name}");
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
        }
    }
}
