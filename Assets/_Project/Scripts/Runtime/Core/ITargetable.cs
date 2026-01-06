using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Interface for objects that can be targeted by towers.
    /// Implemented by enemies to provide targeting data.
    /// </summary>
    public interface ITargetable
    {
        /// <summary>
        /// The transform point to aim at (typically center mass or head).
        /// </summary>
        Transform TargetPoint { get; }

        /// <summary>
        /// Whether this target is currently valid (alive and not at end).
        /// </summary>
        bool IsValidTarget { get; }

        /// <summary>
        /// Current health of the target (for Strongest/Weakest priority).
        /// </summary>
        int CurrentHealth { get; }

        /// <summary>
        /// Distance traveled along the path (for First priority).
        /// </summary>
        float DistanceTraveled { get; }

        /// <summary>
        /// Current movement speed (for Fastest priority).
        /// </summary>
        float CurrentSpeed { get; }
    }
}
