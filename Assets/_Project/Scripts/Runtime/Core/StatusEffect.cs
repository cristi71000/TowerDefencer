using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Abstract base class for all status effects that can be applied to enemies.
    /// Provides common functionality for duration tracking and lifecycle management.
    /// </summary>
    public abstract class StatusEffect
    {
        /// <summary>
        /// The total duration of this effect in seconds.
        /// </summary>
        public float Duration { get; protected set; }

        /// <summary>
        /// The remaining time before this effect expires.
        /// </summary>
        public float RemainingTime { get; protected set; }

        /// <summary>
        /// Whether this effect has expired and should be removed.
        /// </summary>
        public bool IsExpired => RemainingTime <= 0f;

        /// <summary>
        /// The source GameObject that applied this effect (e.g., a tower or projectile).
        /// </summary>
        public GameObject Source { get; protected set; }

        /// <summary>
        /// Whether this effect can stack with other effects of the same type.
        /// Default is false (effects refresh instead of stacking).
        /// </summary>
        public virtual bool CanStack => false;

        /// <summary>
        /// Creates a new status effect with the specified duration.
        /// </summary>
        /// <param name="duration">The duration of the effect in seconds.</param>
        /// <param name="source">The source GameObject that applied this effect.</param>
        protected StatusEffect(float duration, GameObject source = null)
        {
            Duration = duration;
            RemainingTime = duration;
            Source = source;
        }

        /// <summary>
        /// Called when the effect is first applied to an enemy.
        /// Override to implement effect-specific application logic.
        /// </summary>
        /// <param name="enemy">The enemy to apply the effect to.</param>
        public virtual void Apply(TowerDefense.Enemies.Enemy enemy)
        {
            // Base implementation does nothing
            // Subclasses override to apply their specific effects
        }

        /// <summary>
        /// Called every frame while the effect is active.
        /// Override to implement per-frame effect logic (e.g., damage over time).
        /// </summary>
        /// <param name="enemy">The enemy affected by this effect.</param>
        /// <param name="deltaTime">Time elapsed since last update.</param>
        public virtual void Update(TowerDefense.Enemies.Enemy enemy, float deltaTime)
        {
            RemainingTime -= deltaTime;
        }

        /// <summary>
        /// Called when the effect is removed from an enemy (either expired or manually removed).
        /// Override to implement cleanup logic (e.g., restoring original values).
        /// </summary>
        /// <param name="enemy">The enemy to remove the effect from.</param>
        public virtual void Remove(TowerDefense.Enemies.Enemy enemy)
        {
            // Base implementation does nothing
            // Subclasses override to clean up their specific effects
        }

        /// <summary>
        /// Refreshes the effect by resetting its remaining time to the full duration.
        /// Called when a non-stacking effect is re-applied.
        /// </summary>
        public void Refresh()
        {
            RemainingTime = Duration;
        }

        /// <summary>
        /// Refreshes the effect with a new duration value.
        /// Useful when the same effect is applied with different parameters.
        /// </summary>
        /// <param name="newDuration">The new duration to set.</param>
        public void Refresh(float newDuration)
        {
            Duration = newDuration;
            RemainingTime = newDuration;
        }
    }
}
