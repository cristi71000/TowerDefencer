using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Static utility class for damage calculations.
    /// Provides methods for calculating final damage after armor, critical hits, and multipliers.
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// Calculates the final damage after applying multiplier and armor reduction.
        /// Formula: (baseDamage * damageMultiplier) - armor, minimum 1 damage.
        /// </summary>
        /// <param name="baseDamage">The base damage amount before modifications.</param>
        /// <param name="armor">The armor value to reduce damage by.</param>
        /// <param name="damageMultiplier">A multiplier applied to base damage (default 1.0).</param>
        /// <returns>The final damage amount, minimum of 1.</returns>
        public static float CalculateDamage(float baseDamage, float armor, float damageMultiplier = 1f)
        {
            // Apply multiplier first
            float modifiedDamage = baseDamage * damageMultiplier;

            // Then apply armor reduction
            float finalDamage = modifiedDamage - armor;

            // Ensure minimum of 1 damage
            return Mathf.Max(1f, finalDamage);
        }

        /// <summary>
        /// Calculates critical damage by applying a critical multiplier to base damage.
        /// </summary>
        /// <param name="baseDamage">The base damage amount.</param>
        /// <param name="critMultiplier">The critical hit multiplier (e.g., 2.0 for double damage).</param>
        /// <returns>The critical damage amount.</returns>
        public static float CalculateCriticalDamage(float baseDamage, float critMultiplier)
        {
            return baseDamage * critMultiplier;
        }

        /// <summary>
        /// Rolls to determine if a critical hit occurs.
        /// </summary>
        /// <param name="critChance">The chance to critically hit (0.0 to 1.0, where 0.25 = 25% chance).</param>
        /// <returns>True if the roll results in a critical hit, false otherwise.</returns>
        public static bool RollCritical(float critChance)
        {
            return Random.value < critChance;
        }

        /// <summary>
        /// Calculates damage with optional critical hit processing.
        /// Combines critical roll, critical damage calculation, and armor reduction.
        /// </summary>
        /// <param name="baseDamage">The base damage amount.</param>
        /// <param name="armor">The armor value (ignored if damageType is True).</param>
        /// <param name="critChance">The chance to critically hit (0.0 to 1.0).</param>
        /// <param name="critMultiplier">The critical hit multiplier.</param>
        /// <param name="damageType">The type of damage being dealt.</param>
        /// <param name="wasCritical">Output parameter indicating if the hit was critical.</param>
        /// <returns>The final calculated damage.</returns>
        public static float CalculateDamageWithCrit(
            float baseDamage,
            float armor,
            float critChance,
            float critMultiplier,
            DamageType damageType,
            out bool wasCritical)
        {
            wasCritical = RollCritical(critChance);

            float damage = baseDamage;

            // Apply critical multiplier if critical
            if (wasCritical)
            {
                damage = CalculateCriticalDamage(damage, critMultiplier);
            }

            // Apply armor reduction unless damage type is True
            if (damageType != DamageType.True)
            {
                damage = CalculateDamage(damage, armor, 1f);
            }
            else
            {
                // True damage still has minimum of 1
                damage = Mathf.Max(1f, damage);
            }

            return damage;
        }

        /// <summary>
        /// Applies armor reduction based on damage type.
        /// True damage ignores armor completely.
        /// </summary>
        /// <param name="damage">The damage amount to reduce.</param>
        /// <param name="armor">The armor value.</param>
        /// <param name="damageType">The type of damage.</param>
        /// <returns>The damage after armor reduction (if applicable).</returns>
        public static float ApplyArmorReduction(float damage, float armor, DamageType damageType)
        {
            if (damageType == DamageType.True)
            {
                // True damage ignores armor
                return Mathf.Max(1f, damage);
            }

            return CalculateDamage(damage, armor, 1f);
        }
    }
}
