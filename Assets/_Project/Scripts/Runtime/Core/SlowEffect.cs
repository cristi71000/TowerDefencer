using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// A status effect that reduces an enemy's movement speed.
    /// Does not stack - reapplying refreshes the duration instead.
    /// </summary>
    public class SlowEffect : StatusEffect
    {
        private float _slowAmount;
        private TowerDefense.Enemies.Enemy _targetEnemy;

        /// <summary>
        /// The amount of slow applied (0-1 range).
        /// 0 = no slow, 1 = completely stopped.
        /// </summary>
        public float SlowAmount
        {
            get => _slowAmount;
            private set => _slowAmount = Mathf.Clamp01(value);
        }

        /// <summary>
        /// Slow effects do not stack by default.
        /// Reapplying a slow effect refreshes its duration instead.
        /// </summary>
        public override bool CanStack => false;

        /// <summary>
        /// Creates a new slow effect.
        /// </summary>
        /// <param name="slowAmount">The slow amount (0-1 range, clamped).</param>
        /// <param name="duration">The duration of the slow effect in seconds.</param>
        /// <param name="source">The source GameObject that applied this effect.</param>
        public SlowEffect(float slowAmount, float duration, GameObject source = null)
            : base(duration, source)
        {
            SlowAmount = slowAmount;
        }

        /// <summary>
        /// Applies the slow effect to the enemy.
        /// </summary>
        /// <param name="enemy">The enemy to slow.</param>
        public override void Apply(TowerDefense.Enemies.Enemy enemy)
        {
            base.Apply(enemy);

            if (enemy != null)
            {
                _targetEnemy = enemy;
                enemy.ApplySlow(SlowAmount, Duration);
            }
        }

        /// <summary>
        /// Called when the slow effect is removed.
        /// Restores the enemy's original movement speed.
        /// </summary>
        /// <param name="enemy">The enemy to restore speed to.</param>
        public override void Remove(TowerDefense.Enemies.Enemy enemy)
        {
            base.Remove(enemy);

            if (enemy != null)
            {
                enemy.RemoveSlow();
            }
        }

        /// <summary>
        /// Updates the slow effect with a new slow amount.
        /// Used when refreshing with potentially different parameters.
        /// Re-applies the slow to the enemy if a stronger slow is applied.
        /// </summary>
        /// <param name="newSlowAmount">The new slow amount (0-1 range).</param>
        /// <param name="newDuration">The new duration.</param>
        public void UpdateSlowParameters(float newSlowAmount, float newDuration)
        {
            // Only update and refresh if the new slow is stronger or equal
            if (newSlowAmount >= SlowAmount)
            {
                SlowAmount = newSlowAmount;
                // Re-apply the slow to actually change the enemy's speed
                if (_targetEnemy != null && !_targetEnemy.IsDead)
                {
                    _targetEnemy.ApplySlow(SlowAmount, newDuration);
                }
                Refresh(newDuration);
            }
            // Weaker slows do not refresh duration - enemy keeps stronger slow for remaining time
        }
    }
}
