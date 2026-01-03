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

        private NavMeshAgent _navMeshAgent;
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
        }

        public void TakeDamage(float damage)
        {
            if (_isDead || _hasReachedEnd) return;

            _currentHealth -= damage;
            OnDamageTaken?.Invoke(this, damage);

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

            OnDeath?.Invoke(this);

            // Spawn death VFX if configured
            if (_enemyData != null && _enemyData.DeathVFXPrefab != null)
            {
                Instantiate(_enemyData.DeathVFXPrefab, transform.position, Quaternion.identity);
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
