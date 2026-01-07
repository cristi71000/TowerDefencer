using NUnit.Framework;
using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.Tests.EditMode
{
    /// <summary>
    /// Temporary test suite for Issue #17: Damage System validation.
    /// Tests DamageCalculator static methods and DamageInfo struct.
    /// </summary>
    public class DamageSystemTests
    {
        [Test]
        public void TC1_ArmorReduction_ReducesDamageCorrectly()
        {
            // Test: 100 damage with 30 armor = 70 damage
            float result = DamageCalculator.CalculateDamage(100f, 30f, 1f);
            Assert.AreEqual(70f, result, 0.01f, "100 damage - 30 armor should equal 70");
        }

        [Test]
        public void TC1_ArmorReduction_MinimumDamageIs1()
        {
            // Test: 10 damage with 30 armor = 1 damage (minimum)
            float result = DamageCalculator.CalculateDamage(10f, 30f, 1f);
            Assert.AreEqual(1f, result, 0.01f, "Damage should never go below 1");
        }

        [Test]
        public void TC1_ArmorReduction_ZeroArmorNoReduction()
        {
            // Test: 100 damage with 0 armor = 100 damage
            float result = DamageCalculator.CalculateDamage(100f, 0f, 1f);
            Assert.AreEqual(100f, result, 0.01f, "Zero armor should not reduce damage");
        }

        [Test]
        public void TC2_TrueDamage_IgnoresArmor()
        {
            // Test: True damage bypasses armor completely
            float result = DamageCalculator.ApplyArmorReduction(100f, 30f, DamageType.True);
            Assert.AreEqual(100f, result, 0.01f, "True damage should ignore armor");
        }

        [Test]
        public void TC2_PhysicalDamage_AppliesArmor()
        {
            // Test: Physical damage applies armor
            float result = DamageCalculator.ApplyArmorReduction(100f, 30f, DamageType.Physical);
            Assert.AreEqual(70f, result, 0.01f, "Physical damage should apply armor reduction");
        }

        [Test]
        public void TC2_MagicDamage_AppliesArmor()
        {
            // Test: Magic damage applies armor
            float result = DamageCalculator.ApplyArmorReduction(100f, 30f, DamageType.Magic);
            Assert.AreEqual(70f, result, 0.01f, "Magic damage should apply armor reduction");
        }

        [Test]
        public void TC3_DamageMultiplier_AppliesCorrectly()
        {
            // Test: 100 damage with 1.5x multiplier = 150 damage
            float result = DamageCalculator.CalculateDamage(100f, 0f, 1.5f);
            Assert.AreEqual(150f, result, 0.01f, "Damage multiplier should scale base damage");
        }

        [Test]
        public void TC3_DamageMultiplier_WithArmor()
        {
            // Test: (100 * 1.5) - 30 armor = 120 damage
            float result = DamageCalculator.CalculateDamage(100f, 30f, 1.5f);
            Assert.AreEqual(120f, result, 0.01f, "Multiplier should apply before armor");
        }

        [Test]
        public void TC4_CriticalDamage_DoublesBaseDamage()
        {
            // Test: 100 damage with 2.0x crit = 200 damage
            float result = DamageCalculator.CalculateCriticalDamage(100f, 2f);
            Assert.AreEqual(200f, result, 0.01f, "Critical multiplier should scale damage");
        }

        [Test]
        public void TC4_CriticalDamage_CustomMultiplier()
        {
            // Test: 100 damage with 2.5x crit = 250 damage
            float result = DamageCalculator.CalculateCriticalDamage(100f, 2.5f);
            Assert.AreEqual(250f, result, 0.01f, "Custom crit multiplier should work");
        }

        [Test]
        public void TC4_CriticalRoll_100PercentChance()
        {
            // Test: 100% crit chance always returns true
            bool result = DamageCalculator.RollCritical(1f);
            Assert.IsTrue(result, "100% crit chance should always crit");
        }

        [Test]
        public void TC4_CriticalRoll_ZeroPercentChance()
        {
            // Test: 0% crit chance always returns false
            bool result = DamageCalculator.RollCritical(0f);
            Assert.IsFalse(result, "0% crit chance should never crit");
        }

        [Test]
        public void TC4_CalculateDamageWithCrit_NonCritical()
        {
            // Force non-crit by using 0% chance
            float result = DamageCalculator.CalculateDamageWithCrit(
                100f, 30f, 0f, 2f, DamageType.Physical, out bool wasCritical);

            Assert.IsFalse(wasCritical, "Should not be critical");
            Assert.AreEqual(70f, result, 0.01f, "Non-crit should apply armor normally");
        }

        [Test]
        public void TC4_CalculateDamageWithCrit_Critical()
        {
            // Force crit by using 100% chance
            float result = DamageCalculator.CalculateDamageWithCrit(
                100f, 30f, 1f, 2f, DamageType.Physical, out bool wasCritical);

            Assert.IsTrue(wasCritical, "Should be critical");
            Assert.AreEqual(170f, result, 0.01f, "Crit should apply: (100*2)-30 = 170");
        }

        [Test]
        public void TC4_CalculateDamageWithCrit_TrueDamageCritical()
        {
            // Force crit with true damage
            float result = DamageCalculator.CalculateDamageWithCrit(
                100f, 30f, 1f, 2f, DamageType.True, out bool wasCritical);

            Assert.IsTrue(wasCritical, "Should be critical");
            Assert.AreEqual(200f, result, 0.01f, "True damage crit should ignore armor: 100*2 = 200");
        }

        [Test]
        public void DamageInfo_Create_SimpleVersion()
        {
            // Test: Simple factory method
            DamageInfo info = DamageInfo.Create(50f, DamageType.Physical);

            Assert.AreEqual(50f, info.Amount, "Amount should match");
            Assert.AreEqual(DamageType.Physical, info.Type, "Type should match");
            Assert.IsFalse(info.IsCritical, "Should not be critical by default");
            Assert.IsNull(info.Source, "Source should be null");
            Assert.AreEqual(Vector3.zero, info.HitPoint, "HitPoint should be zero");
        }

        [Test]
        public void DamageInfo_Create_WithPosition()
        {
            // Test: Factory method with position and critical
            Vector3 hitPos = new Vector3(5f, 2f, 3f);
            DamageInfo info = DamageInfo.Create(75f, hitPos, true, DamageType.Magic);

            Assert.AreEqual(75f, info.Amount, "Amount should match");
            Assert.AreEqual(DamageType.Magic, info.Type, "Type should match");
            Assert.IsTrue(info.IsCritical, "Should be critical");
            Assert.AreEqual(hitPos, info.HitPoint, "HitPoint should match");
        }

        [Test]
        public void DamageInfo_Constructor_AllParameters()
        {
            // Test: Full constructor
            GameObject source = new GameObject("TestSource");
            Vector3 hitPos = new Vector3(1f, 2f, 3f);

            DamageInfo info = new DamageInfo(100f, source, hitPos, true, DamageType.True);

            Assert.AreEqual(100f, info.Amount, "Amount should match");
            Assert.AreEqual(source, info.Source, "Source should match");
            Assert.AreEqual(hitPos, info.HitPoint, "HitPoint should match");
            Assert.IsTrue(info.IsCritical, "Should be critical");
            Assert.AreEqual(DamageType.True, info.Type, "Type should match");

            Object.DestroyImmediate(source);
        }
    }
}
