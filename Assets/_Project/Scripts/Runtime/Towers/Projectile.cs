using System;
using UnityEngine;
using TowerDefense.Core;
using TowerDefense.Enemies;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Projectile component that handles movement, collision, and damage dealing.
    /// Tracks the target's current position (no lead prediction).
    /// Supports both single-target and AOE damage modes.
    /// Designed for use with object pooling.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        private const int MaxAOETargets = 32;
        private static readonly Collider[] _aoeHitBuffer = new Collider[MaxAOETargets];
        private static int _enemyLayer = -1;

        [Header("Settings")]
        [SerializeField] private float _defaultSpeed = 15f;
        [SerializeField] private float _defaultLifetime = 5f;
        [SerializeField] private bool _trackTarget = true;
        [SerializeField] private float _rotationSpeed = 720f;

        [Header("Hit Effects")]
        [SerializeField] private GameObject _impactVFXPrefab;
        [SerializeField] private AudioClip _impactSound;

        // Runtime state
        private Transform _target;
        private Vector3 _lastTargetPosition;
        private int _damage;
        private float _speed;
        private float _aoeRadius;
        private float _lifetime;
        private float _timer;
        private bool _isActive;
        private Rigidbody _rigidbody;
        private GameObject _prefabSource;
        private float _slowAmount;
        private float _slowDuration;

        /// <summary>
        /// Event fired when the projectile hits a target.
        /// Parameters: projectile instance, damage dealt
        /// </summary>
        public event Action<Projectile, int> OnHit;

        /// <summary>
        /// Event fired when the projectile expires without hitting anything.
        /// Parameter: projectile instance
        /// </summary>
        public event Action<Projectile> OnExpired;

        /// <summary>
        /// The prefab this projectile was instantiated from. Used for pool return.
        /// </summary>
        public GameObject PrefabSource
        {
            get => _prefabSource;
            set => _prefabSource = value;
        }

        /// <summary>
        /// Whether this projectile is currently active and should be updated.
        /// </summary>
        public bool IsActive => _isActive;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            // Ensure rigidbody is configured correctly for projectile behavior
            if (_rigidbody != null)
            {
                _rigidbody.isKinematic = true;
                _rigidbody.useGravity = false;
            }

            // Cache enemy layer (Layer 8) - only lookup once
            if (_enemyLayer < 0)
            {
                _enemyLayer = LayerMask.NameToLayer("Enemy");
            }
        }

        /// <summary>
        /// Initializes the projectile with target and combat parameters.
        /// </summary>
        /// <param name="target">The target transform to track (can be null for unguided)</param>
        /// <param name="damage">Damage to deal on hit</param>
        /// <param name="speed">Movement speed (uses default if 0)</param>
        /// <param name="aoeRadius">AOE radius (0 for single target)</param>
        public void Initialize(Transform target, int damage, float speed = 0f, float aoeRadius = 0f)
        {
            _target = target;
            _damage = damage;
            _speed = speed > 0f ? speed : _defaultSpeed;
            _aoeRadius = aoeRadius;
            _lifetime = _defaultLifetime;
            _timer = 0f;
            _isActive = true;

            // Cache initial target position for when target becomes invalid
            if (_target != null)
            {
                _lastTargetPosition = _target.position;
            }
            else
            {
                // If no target, move forward
                _lastTargetPosition = transform.position + transform.forward * 100f;
            }

            // Look toward target initially
            Vector3 direction = (_lastTargetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        /// <summary>
        /// Initializes the projectile with extended parameters.
        /// </summary>
        /// <param name="target">The target transform to track</param>
        /// <param name="damage">Damage to deal on hit</param>
        /// <param name="speed">Movement speed</param>
        /// <param name="aoeRadius">AOE radius (0 for single target)</param>
        /// <param name="lifetime">Maximum time before expiration</param>
        public void Initialize(Transform target, int damage, float speed, float aoeRadius, float lifetime)
        {
            Initialize(target, damage, speed, aoeRadius);
            _lifetime = lifetime > 0f ? lifetime : _defaultLifetime;
        }

        /// <summary>
        /// Initializes the projectile with status effect parameters.
        /// </summary>
        /// <param name="target">The target transform to track</param>
        /// <param name="damage">Damage to deal on hit</param>
        /// <param name="speed">Movement speed</param>
        /// <param name="aoeRadius">AOE radius (0 for single target)</param>
        /// <param name="lifetime">Maximum time before expiration</param>
        /// <param name="slowAmount">Slow effect amount (0-1, 0 = none)</param>
        /// <param name="slowDuration">Slow effect duration in seconds</param>
        public void Initialize(Transform target, int damage, float speed, float aoeRadius, float lifetime, float slowAmount, float slowDuration)
        {
            Initialize(target, damage, speed, aoeRadius, lifetime);
            _slowAmount = Mathf.Clamp01(slowAmount);
            _slowDuration = slowDuration;
        }

        private void Update()
        {
            if (!_isActive) return;

            // Update timer
            _timer += Time.deltaTime;
            if (_timer >= _lifetime)
            {
                Expire();
                return;
            }

            // Update target position if target is still valid
            if (_target != null)
            {
                ITargetable targetable = _target.GetComponentInParent<ITargetable>();
                if (targetable != null && targetable.IsValidTarget)
                {
                    _lastTargetPosition = targetable.TargetPoint.position;
                }
                else
                {
                    // Target became invalid, continue toward last known position
                    _target = null;
                }
            }

            // Move toward target
            MoveTowardTarget();
        }

        private void MoveTowardTarget()
        {
            Vector3 direction = (_lastTargetPosition - transform.position).normalized;

            if (direction == Vector3.zero)
            {
                // Fallback to forward direction
                direction = transform.forward;
            }

            // Rotate toward target if tracking is enabled
            if (_trackTarget && direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }

            // Move forward
            transform.position += transform.forward * _speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;

            // Only collide with enemies
            if (other.gameObject.layer != _enemyLayer) return;

            // Get the enemy component
            Enemy enemy = other.GetComponentInParent<Enemy>();
            if (enemy == null || enemy.IsDead) return;

            // Apply damage
            if (_aoeRadius > 0f)
            {
                ApplyAOEDamage();
            }
            else
            {
                ApplySingleTargetDamage(enemy);
            }

            // Spawn impact VFX
            SpawnImpactEffects();

            // Fire hit event
            OnHit?.Invoke(this, _damage);

            // Deactivate and return to pool
            Deactivate();
        }

        private void ApplySingleTargetDamage(Enemy enemy)
        {
            enemy.TakeDamage(_damage);
            ApplyStatusEffects(enemy);
        }

        private void ApplyAOEDamage()
        {
            // Use static buffer to avoid garbage allocation
            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                _aoeRadius,
                _aoeHitBuffer,
                1 << _enemyLayer
            );

            for (int i = 0; i < hitCount; i++)
            {
                Enemy enemy = _aoeHitBuffer[i].GetComponentInParent<Enemy>();
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.TakeDamage(_damage);
                    ApplyStatusEffects(enemy);
                }
            }
        }

        /// <summary>
        /// Applies status effects (like slow) to the hit enemy.
        /// </summary>
        /// <param name="enemy">The enemy to apply status effects to.</param>
        private void ApplyStatusEffects(Enemy enemy)
        {
            if (_slowAmount > 0f && _slowDuration > 0f)
            {
                var statusManager = enemy.GetComponent<StatusEffectManager>();
                if (statusManager != null)
                {
                    var slowEffect = new SlowEffect(_slowAmount, _slowDuration, gameObject);
                    statusManager.AddEffect(slowEffect);
                }
            }
        }

        private void SpawnImpactEffects()
        {
            if (_impactVFXPrefab != null)
            {
                GameObject vfx = Instantiate(_impactVFXPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 2f); // Auto-cleanup
            }

            // Audio would be played here via AudioSource or AudioManager
            // if (_impactSound != null) { ... }
        }

        private void Expire()
        {
            OnExpired?.Invoke(this);
            Deactivate();
        }

        private void Deactivate()
        {
            _isActive = false;

            // Return to pool
            if (ProjectilePoolManager.Instance != null && _prefabSource != null)
            {
                ProjectilePoolManager.Instance.ReturnProjectile(this, _prefabSource);
            }
            else
            {
                // Fallback: destroy if no pool manager
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Resets the projectile state for pool reuse.
        /// Called by the pool manager when returning to pool.
        /// </summary>
        public void Reset()
        {
            _target = null;
            _lastTargetPosition = Vector3.zero;
            _damage = 0;
            _speed = _defaultSpeed;
            _aoeRadius = 0f;
            _lifetime = _defaultLifetime;
            _timer = 0f;
            _isActive = false;
            _slowAmount = 0f;
            _slowDuration = 0f;

            // Clear event subscribers to prevent stale references
            OnHit = null;
            OnExpired = null;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw AOE radius if applicable
            if (_aoeRadius > 0f)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                Gizmos.DrawWireSphere(transform.position, _aoeRadius);
            }

            // Draw direction
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
    }
}
