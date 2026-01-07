using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Types of damage that can be dealt.
    /// </summary>
    public enum DamageType
    {
        /// <summary>
        /// Physical damage, reduced by armor.
        /// </summary>
        Physical,

        /// <summary>
        /// Magic damage, reduced by armor.
        /// </summary>
        Magic,

        /// <summary>
        /// True damage, ignores armor completely.
        /// </summary>
        True
    }

    /// <summary>
    /// Contains information about a damage instance.
    /// </summary>
    public struct DamageInfo
    {
        /// <summary>
        /// The amount of damage to deal (before armor reduction).
        /// </summary>
        public float Amount;

        /// <summary>
        /// The source GameObject that caused the damage.
        /// </summary>
        public GameObject Source;

        /// <summary>
        /// The world position where the damage was applied.
        /// </summary>
        public Vector3 HitPoint;

        /// <summary>
        /// Whether this damage instance was a critical hit.
        /// </summary>
        public bool IsCritical;

        /// <summary>
        /// The type of damage being dealt.
        /// </summary>
        public DamageType Type;

        /// <summary>
        /// Creates a new DamageInfo instance.
        /// </summary>
        /// <param name="amount">The damage amount.</param>
        /// <param name="source">The source of the damage.</param>
        /// <param name="hitPoint">The world position of the hit.</param>
        /// <param name="isCritical">Whether this is a critical hit.</param>
        /// <param name="type">The damage type.</param>
        public DamageInfo(float amount, GameObject source, Vector3 hitPoint, bool isCritical = false, DamageType type = DamageType.Physical)
        {
            Amount = amount;
            Source = source;
            HitPoint = hitPoint;
            IsCritical = isCritical;
            Type = type;
        }

        /// <summary>
        /// Creates a simple DamageInfo with just amount and type.
        /// </summary>
        /// <param name="amount">The damage amount.</param>
        /// <param name="type">The damage type.</param>
        public static DamageInfo Create(float amount, DamageType type = DamageType.Physical)
        {
            return new DamageInfo
            {
                Amount = amount,
                Source = null,
                HitPoint = Vector3.zero,
                IsCritical = false,
                Type = type
            };
        }

        /// <summary>
        /// Creates a DamageInfo with position and critical information.
        /// </summary>
        /// <param name="amount">The damage amount.</param>
        /// <param name="hitPoint">The world position of the hit.</param>
        /// <param name="isCritical">Whether this is a critical hit.</param>
        /// <param name="type">The damage type.</param>
        public static DamageInfo Create(float amount, Vector3 hitPoint, bool isCritical = false, DamageType type = DamageType.Physical)
        {
            return new DamageInfo
            {
                Amount = amount,
                Source = null,
                HitPoint = hitPoint,
                IsCritical = isCritical,
                Type = type
            };
        }
    }
}
