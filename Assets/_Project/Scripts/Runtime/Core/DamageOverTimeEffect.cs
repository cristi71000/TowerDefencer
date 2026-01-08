using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// A status effect that deals periodic damage to an enemy over time.
    /// Can stack with other DoT effects, allowing multiple instances to apply damage simultaneously.
    /// </summary>
    public class DamageOverTimeEffect : StatusEffect
    {
        private float _damagePerTick;
        private float _tickInterval;
        private float _timeSinceLastTick;

        /// <summary>
        /// The amount of damage dealt each tick.
        /// </summary>
        public float DamagePerTick
        {
            get => _damagePerTick;
            private set => _damagePerTick = Mathf.Max(0f, value);
        }

        /// <summary>
        /// The interval between damage ticks in seconds.
        /// </summary>
        public float TickInterval
        {
            get => _tickInterval;
            private set => _tickInterval = Mathf.Max(0.1f, value); // Minimum 0.1s to prevent excessive ticking
        }

        /// <summary>
        /// Damage over time effects CAN stack.
        /// Multiple DoT effects of the same type can be applied simultaneously.
        /// </summary>
        public override bool CanStack => true;

        /// <summary>
        /// The type of damage this DoT effect deals.
        /// Used for armor calculations.
        /// </summary>
        public DamageType DamageType { get; private set; }

        /// <summary>
        /// Creates a new damage over time effect.
        /// </summary>
        /// <param name="damagePerTick">The damage dealt each tick.</param>
        /// <param name="tickInterval">The interval between ticks in seconds.</param>
        /// <param name="duration">The total duration of the effect in seconds.</param>
        /// <param name="damageType">The type of damage to deal (default: Magic).</param>
        /// <param name="source">The source GameObject that applied this effect.</param>
        public DamageOverTimeEffect(float damagePerTick, float tickInterval, float duration,
            DamageType damageType = DamageType.Magic, GameObject source = null)
            : base(duration, source)
        {
            DamagePerTick = damagePerTick;
            TickInterval = tickInterval;
            DamageType = damageType;
            _timeSinceLastTick = 0f;
        }

        /// <summary>
        /// Called when the DoT effect is first applied.
        /// Optionally applies the first tick of damage immediately.
        /// </summary>
        /// <param name="enemy">The enemy to apply the effect to.</param>
        public override void Apply(TowerDefense.Enemies.Enemy enemy)
        {
            base.Apply(enemy);

            // Reset tick timer when applied
            _timeSinceLastTick = 0f;
        }

        /// <summary>
        /// Updates the DoT effect, applying damage at regular intervals.
        /// </summary>
        /// <param name="enemy">The enemy affected by this effect.</param>
        /// <param name="deltaTime">Time elapsed since last update.</param>
        public override void Update(TowerDefense.Enemies.Enemy enemy, float deltaTime)
        {
            base.Update(enemy, deltaTime);

            if (enemy == null || enemy.IsDead)
            {
                return;
            }

            _timeSinceLastTick += deltaTime;

            // Apply damage for each tick that has passed
            while (_timeSinceLastTick >= TickInterval && !IsExpired)
            {
                ApplyDamageTick(enemy);
                _timeSinceLastTick -= TickInterval;
            }
        }

        /// <summary>
        /// Applies a single tick of damage to the enemy.
        /// </summary>
        /// <param name="enemy">The enemy to damage.</param>
        private void ApplyDamageTick(TowerDefense.Enemies.Enemy enemy)
        {
            if (enemy == null || enemy.IsDead)
            {
                return;
            }

            // Create damage info for the tick
            DamageInfo damageInfo = new DamageInfo(
                DamagePerTick,
                Source,
                enemy.transform.position,
                false, // DoT is never critical
                DamageType
            );

            enemy.TakeDamage(damageInfo);
        }

        /// <summary>
        /// Calculates the total damage this effect will deal over its remaining duration.
        /// </summary>
        /// <returns>The total expected damage.</returns>
        public float GetTotalRemainingDamage()
        {
            if (TickInterval <= 0f)
            {
                return 0f;
            }

            int remainingTicks = Mathf.FloorToInt(RemainingTime / TickInterval);
            return remainingTicks * DamagePerTick;
        }

        /// <summary>
        /// Gets the damage per second (DPS) of this effect.
        /// </summary>
        /// <returns>The damage per second.</returns>
        public float GetDamagePerSecond()
        {
            if (TickInterval <= 0f)
            {
                return 0f;
            }

            return DamagePerTick / TickInterval;
        }
    }
}
