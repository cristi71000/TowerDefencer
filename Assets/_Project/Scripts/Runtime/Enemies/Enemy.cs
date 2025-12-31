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

        public EnemyData Data => _enemyData;
        public float CurrentHealth => _currentHealth;
        public float MaxHealth => _enemyData != null ? _enemyData.MaxHealth : 0f;
        public float HealthPercent => MaxHealth > 0 ? _currentHealth / MaxHealth : 0f;
        public bool IsDead => _isDead;
        public Transform HealthBarAnchor => _healthBarAnchor;

        public event System.Action<Enemy> OnDeath;
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

            if (_navMeshAgent != null)
            {
                _navMeshAgent.speed = data.MoveSpeed;
                _navMeshAgent.enabled = !data.IsFlying;
            }
        }

        public void TakeDamage(float damage)
        {
            if (_isDead) return;

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

            // Destroy this enemy
            Destroy(gameObject);
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
