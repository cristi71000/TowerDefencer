using System;
using UnityEngine;
using TowerDefense.Core;
using TowerDefense.Enemies;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Handles tower attack logic, including firing projectiles and dealing damage.
    /// Integrates with TowerTargeting for target acquisition and TowerAiming for aim verification.
    /// </summary>
    [RequireComponent(typeof(Tower))]
    [RequireComponent(typeof(TowerTargeting))]
    public class TowerAttack : MonoBehaviour
    {
        private const int MaxAOETargets = 32;
        private static readonly Collider[] _aoeHitBuffer = new Collider[MaxAOETargets];
        private static int _enemyLayerMask = -1;

        [Header("Attack Settings")]
        [SerializeField] private bool _requireAiming = true;
        [SerializeField] private bool _playAttackEffects = true;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;

        private Tower _tower;
        private TowerTargeting _targeting;
        private TowerAiming _aiming;
        private float _attackTimer;

        /// <summary>
        /// Event fired when the tower attacks.
        /// Parameters: tower instance, target
        /// </summary>
        public event Action<TowerAttack, ITargetable> OnAttack;

        /// <summary>
        /// Event fired when a projectile hits its target.
        /// Parameters: tower instance, damage dealt
        /// </summary>
        public event Action<TowerAttack, int> OnProjectileHit;

        /// <summary>
        /// Whether the tower is ready to attack (timer expired and can fire).
        /// </summary>
        public bool IsReadyToAttack => CanAttack();

        private void Awake()
        {
            _tower = GetComponent<Tower>();
            _targeting = GetComponent<TowerTargeting>();
            _aiming = GetComponent<TowerAiming>(); // Optional component
        }

        private void Start()
        {
            // Initialize audio source if not assigned
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }
        }

        private void Update()
        {
            if (_tower.Data == null) return;

            // Update attack timer
            _attackTimer += Time.deltaTime;

            // Attempt to attack if conditions are met
            if (CanAttack())
            {
                Attack();
            }
        }

        /// <summary>
        /// Checks if all attack conditions are met.
        /// </summary>
        private bool CanAttack()
        {
            // Must have tower data
            if (_tower.Data == null) return false;

            // Must have a valid target
            if (!_targeting.HasTarget) return false;

            // Must have waited long enough since last attack
            if (_attackTimer < _tower.Data.AttackInterval) return false;

            // If aiming is required and we have an aiming component, check if aimed
            if (_requireAiming && _aiming != null && !_aiming.IsAimed) return false;

            return true;
        }

        /// <summary>
        /// Performs the attack on the current target.
        /// </summary>
        private void Attack()
        {
            ITargetable target = _targeting.CurrentTarget;

            // Double-check target validity
            if (target == null || !target.IsValidTarget) return;

            // Reset attack timer
            _attackTimer = 0f;

            // Fire projectile or apply direct damage
            if (_tower.Data.ProjectilePrefab != null)
            {
                FireProjectile(target);
            }
            else
            {
                ApplyDirectDamage(target);
            }

            // Play attack effects
            if (_playAttackEffects)
            {
                PlayAttackSound();
                SpawnMuzzleFlash();
            }

            // Fire attack event
            OnAttack?.Invoke(this, target);
        }

        /// <summary>
        /// Fires a projectile at the target using the projectile pool.
        /// </summary>
        private void FireProjectile(ITargetable target)
        {
            if (ProjectilePoolManager.Instance == null)
            {
                Debug.LogWarning($"[TowerAttack] No ProjectilePoolManager instance found. Falling back to direct damage.");
                ApplyDirectDamage(target);
                return;
            }

            Transform firePoint = _tower.GetFirePoint();
            Vector3 spawnPosition = firePoint.position;
            Quaternion spawnRotation = firePoint.rotation;

            // Get projectile from pool
            Projectile projectile = ProjectilePoolManager.Instance.GetProjectile(
                _tower.Data.ProjectilePrefab,
                spawnPosition,
                spawnRotation
            );

            if (projectile == null)
            {
                Debug.LogWarning($"[TowerAttack] Failed to get projectile from pool. Falling back to direct damage.");
                ApplyDirectDamage(target);
                return;
            }

            // Initialize projectile with combat parameters
            // Use target.TargetPoint as the tracking transform
            projectile.Initialize(
                target.TargetPoint,
                _tower.Data.Damage,
                _tower.Data.ProjectileSpeed,
                _tower.Data.AOERadius
            );

            // Subscribe to projectile events for tracking
            projectile.OnHit += HandleProjectileHit;
            projectile.OnExpired += HandleProjectileExpired;
        }

        /// <summary>
        /// Applies damage directly to the target without a projectile.
        /// Used as a fallback when no projectile prefab is configured.
        /// </summary>
        private void ApplyDirectDamage(ITargetable target)
        {
            // Try to get IDamageable or Enemy component from target
            if (target.TargetPoint == null) return;

            Enemy primaryEnemy = target.TargetPoint.GetComponentInParent<Enemy>();
            if (primaryEnemy != null && !primaryEnemy.IsDead)
            {
                primaryEnemy.TakeDamage(_tower.Data.Damage);

                // Handle AOE damage if configured (excludes primary target)
                if (_tower.Data.AOERadius > 0f)
                {
                    ApplyAOEDamage(target.TargetPoint.position, primaryEnemy);
                }

                // Fire hit event for direct damage
                OnProjectileHit?.Invoke(this, _tower.Data.Damage);
            }
        }

        /// <summary>
        /// Applies AOE damage around the specified position, excluding the primary target.
        /// </summary>
        /// <param name="center">Center of the AOE effect</param>
        /// <param name="excludeEnemy">The primary target to exclude from AOE damage</param>
        private void ApplyAOEDamage(Vector3 center, Enemy excludeEnemy)
        {
            // Cache enemy layer mask on first use
            if (_enemyLayerMask < 0)
            {
                _enemyLayerMask = LayerMask.GetMask("Enemy");
            }

            int hitCount = Physics.OverlapSphereNonAlloc(
                center,
                _tower.Data.AOERadius,
                _aoeHitBuffer,
                _enemyLayerMask
            );

            for (int i = 0; i < hitCount; i++)
            {
                Enemy enemy = _aoeHitBuffer[i].GetComponentInParent<Enemy>();
                // Skip the primary target (already damaged) and dead enemies
                if (enemy != null && enemy != excludeEnemy && !enemy.IsDead)
                {
                    enemy.TakeDamage(_tower.Data.Damage);
                }
            }
        }

        /// <summary>
        /// Handles projectile hit event.
        /// </summary>
        private void HandleProjectileHit(Projectile projectile, int damage)
        {
            // Unsubscribe to prevent memory leaks
            projectile.OnHit -= HandleProjectileHit;
            projectile.OnExpired -= HandleProjectileExpired;

            // Fire hit event
            OnProjectileHit?.Invoke(this, damage);
        }

        /// <summary>
        /// Handles projectile expiration event.
        /// </summary>
        private void HandleProjectileExpired(Projectile projectile)
        {
            // Unsubscribe to prevent memory leaks
            projectile.OnHit -= HandleProjectileHit;
            projectile.OnExpired -= HandleProjectileExpired;
        }

        /// <summary>
        /// Plays the attack sound if configured.
        /// </summary>
        private void PlayAttackSound()
        {
            if (_tower.Data.AttackSound == null) return;

            if (_audioSource != null)
            {
                _audioSource.PlayOneShot(_tower.Data.AttackSound);
            }
            else
            {
                // Fallback: Play at position (less efficient, but works)
                AudioSource.PlayClipAtPoint(_tower.Data.AttackSound, transform.position);
            }
        }

        /// <summary>
        /// Spawns the muzzle flash effect if configured.
        /// </summary>
        private void SpawnMuzzleFlash()
        {
            if (_tower.Data.MuzzleFlashPrefab == null) return;

            Transform firePoint = _tower.GetFirePoint();
            GameObject muzzleFlash = Instantiate(
                _tower.Data.MuzzleFlashPrefab,
                firePoint.position,
                firePoint.rotation
            );

            // Auto-destroy muzzle flash after a short duration
            Destroy(muzzleFlash, 0.5f);
        }

        /// <summary>
        /// Forces an immediate attack if possible, bypassing the timer.
        /// Useful for special abilities or instant-fire mechanics.
        /// </summary>
        /// <returns>True if the attack was performed</returns>
        public bool ForceAttack()
        {
            if (!_targeting.HasTarget) return false;

            ITargetable target = _targeting.CurrentTarget;
            if (target == null || !target.IsValidTarget) return false;

            // Skip timer check but still respect aiming requirement
            if (_requireAiming && _aiming != null && !_aiming.IsAimed) return false;

            // Reset timer and attack
            _attackTimer = 0f;

            if (_tower.Data.ProjectilePrefab != null)
            {
                FireProjectile(target);
            }
            else
            {
                ApplyDirectDamage(target);
            }

            if (_playAttackEffects)
            {
                PlayAttackSound();
                SpawnMuzzleFlash();
            }

            OnAttack?.Invoke(this, target);
            return true;
        }

        /// <summary>
        /// Sets whether the tower requires aiming before firing.
        /// </summary>
        public void SetRequireAiming(bool requireAiming)
        {
            _requireAiming = requireAiming;
        }

        /// <summary>
        /// Gets the time remaining until the next attack is ready.
        /// </summary>
        public float GetCooldownRemaining()
        {
            if (_tower.Data == null) return 0f;
            return Mathf.Max(0f, _tower.Data.AttackInterval - _attackTimer);
        }

        /// <summary>
        /// Gets the attack cooldown progress as a value from 0 to 1.
        /// </summary>
        public float GetCooldownProgress()
        {
            if (_tower.Data == null || _tower.Data.AttackInterval <= 0f) return 1f;
            return Mathf.Clamp01(_attackTimer / _tower.Data.AttackInterval);
        }

        private void OnDisable()
        {
            // Clear event subscribers when disabled to prevent stale references
            OnAttack = null;
            OnProjectileHit = null;
        }

        private void OnDrawGizmosSelected()
        {
            Tower tower = _tower != null ? _tower : GetComponent<Tower>();
            if (tower == null || tower.Data == null) return;

            // Draw fire point
            Transform firePoint = tower.GetFirePoint();
            if (firePoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(firePoint.position, 0.1f);
                Gizmos.DrawRay(firePoint.position, firePoint.forward * 1f);
            }

            // Draw AOE radius at tower position (for visualization)
            if (tower.Data.AOERadius > 0f)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
                Gizmos.DrawWireSphere(transform.position, tower.Data.AOERadius);
            }
        }
    }
}
