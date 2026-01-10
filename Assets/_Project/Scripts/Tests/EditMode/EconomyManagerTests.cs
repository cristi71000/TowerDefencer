using System.Reflection;
using NUnit.Framework;
using TowerDefense.Economy;
using UnityEngine;

namespace TowerDefense.Tests.EditMode
{
    [TestFixture]
    public class EconomyManagerTests
    {
        private GameObject _economyManagerObject;
        private EconomyManager _economyManager;

        [SetUp]
        public void SetUp()
        {
            _economyManagerObject = new GameObject("EconomyManager");
            _economyManager = _economyManagerObject.AddComponent<EconomyManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_economyManagerObject != null)
            {
                Object.DestroyImmediate(_economyManagerObject);
            }
        }

        [Test]
        public void Initialize_SetsStartingCurrency()
        {
            // Arrange
            int startingCurrency = 150;

            // Act
            _economyManager.Initialize(startingCurrency);

            // Assert
            Assert.AreEqual(startingCurrency, _economyManager.CurrentCurrency, "Currency should be set to starting amount");
            Assert.AreEqual(startingCurrency, _economyManager.TotalEarned, "TotalEarned should equal starting currency");
            Assert.AreEqual(0, _economyManager.TotalSpent, "TotalSpent should be 0 after initialization");
        }

        [Test]
        public void AddCurrency_WithPositiveAmount_IncreasesCurrency()
        {
            // Arrange
            _economyManager.Initialize(100);

            // Act
            _economyManager.AddCurrency(50);

            // Assert
            Assert.AreEqual(150, _economyManager.CurrentCurrency, "Currency should increase by added amount");
            Assert.AreEqual(150, _economyManager.TotalEarned, "TotalEarned should include added amount");
        }

        [Test]
        public void AddCurrency_WithZeroOrNegative_DoesNotChangeCurrency()
        {
            // Arrange
            _economyManager.Initialize(100);

            // Act
            _economyManager.AddCurrency(0);
            _economyManager.AddCurrency(-10);

            // Assert
            Assert.AreEqual(100, _economyManager.CurrentCurrency, "Currency should remain unchanged");
            Assert.AreEqual(100, _economyManager.TotalEarned, "TotalEarned should remain unchanged");
        }

        [Test]
        public void TrySpend_WithSufficientFunds_ReturnsTrue()
        {
            // Arrange
            _economyManager.Initialize(100);

            // Act
            bool result = _economyManager.TrySpend(50);

            // Assert
            Assert.IsTrue(result, "TrySpend should return true when funds are sufficient");
            Assert.AreEqual(50, _economyManager.CurrentCurrency, "Currency should be reduced");
            Assert.AreEqual(50, _economyManager.TotalSpent, "TotalSpent should be updated");
        }

        [Test]
        public void TrySpend_WithExactFunds_ReturnsTrue()
        {
            // Arrange
            _economyManager.Initialize(100);

            // Act
            bool result = _economyManager.TrySpend(100);

            // Assert
            Assert.IsTrue(result, "TrySpend should return true when funds exactly match");
            Assert.AreEqual(0, _economyManager.CurrentCurrency, "Currency should be 0 after spending all");
            Assert.AreEqual(100, _economyManager.TotalSpent, "TotalSpent should reflect full spend");
        }

        [Test]
        public void TrySpend_WithInsufficientFunds_ReturnsFalse()
        {
            // Arrange
            _economyManager.Initialize(50);

            // Act
            bool result = _economyManager.TrySpend(100);

            // Assert
            Assert.IsFalse(result, "TrySpend should return false when funds are insufficient");
            Assert.AreEqual(50, _economyManager.CurrentCurrency, "Currency should remain unchanged");
            Assert.AreEqual(0, _economyManager.TotalSpent, "TotalSpent should remain 0");
        }

        [Test]
        public void TrySpend_WithZeroOrNegative_ReturnsTrue()
        {
            // Arrange
            _economyManager.Initialize(100);

            // Act
            bool resultZero = _economyManager.TrySpend(0);
            bool resultNegative = _economyManager.TrySpend(-10);

            // Assert
            Assert.IsTrue(resultZero, "TrySpend with 0 should return true");
            Assert.IsTrue(resultNegative, "TrySpend with negative should return true");
            Assert.AreEqual(100, _economyManager.CurrentCurrency, "Currency should remain unchanged");
        }

        [Test]
        public void CanAfford_WithSufficientFunds_ReturnsTrue()
        {
            // Arrange
            _economyManager.Initialize(100);

            // Act & Assert
            Assert.IsTrue(_economyManager.CanAfford(50), "Should afford 50 with 100 currency");
            Assert.IsTrue(_economyManager.CanAfford(100), "Should afford 100 with 100 currency");
        }

        [Test]
        public void CanAfford_WithInsufficientFunds_ReturnsFalse()
        {
            // Arrange
            _economyManager.Initialize(50);

            // Act & Assert
            Assert.IsFalse(_economyManager.CanAfford(100), "Should not afford 100 with 50 currency");
        }

        [Test]
        public void CalculateWaveBonus_ReturnsCorrectBonus()
        {
            // Arrange
            _economyManager.Initialize(100);
            // Default: _baseWaveBonus = 50, _waveBonusIncrement = 10

            // Act & Assert
            // Wave 1: 50 + (1 * 10) = 60
            Assert.AreEqual(60, _economyManager.CalculateWaveBonus(1), "Wave 1 bonus should be 60");
            // Wave 5: 50 + (5 * 10) = 100
            Assert.AreEqual(100, _economyManager.CalculateWaveBonus(5), "Wave 5 bonus should be 100");
            // Wave 10: 50 + (10 * 10) = 150
            Assert.AreEqual(150, _economyManager.CalculateWaveBonus(10), "Wave 10 bonus should be 150");
        }

        [Test]
        public void CalculateInterest_WhenDisabled_ReturnsZero()
        {
            // Arrange - interest is disabled by default
            _economyManager.Initialize(1000);

            // Act
            int interest = _economyManager.CalculateInterest();

            // Assert
            Assert.AreEqual(0, interest, "Interest should be 0 when disabled");
        }

        [Test]
        public void OnCurrencyChanged_FiresWhenCurrencyModified()
        {
            // Arrange
            _economyManager.Initialize(100);
            int receivedAmount = -1;
            int receivedDelta = 0;
            _economyManager.OnCurrencyChanged += (amount, delta) =>
            {
                receivedAmount = amount;
                receivedDelta = delta;
            };

            // Act
            _economyManager.AddCurrency(50);

            // Assert
            Assert.AreEqual(150, receivedAmount, "OnCurrencyChanged should receive correct amount");
            Assert.AreEqual(50, receivedDelta, "OnCurrencyChanged should receive correct delta");
        }

        [Test]
        public void OnCurrencyChanged_FiresWithNegativeDeltaWhenSpending()
        {
            // Arrange
            _economyManager.Initialize(100);
            int receivedAmount = -1;
            int receivedDelta = 0;
            _economyManager.OnCurrencyChanged += (amount, delta) =>
            {
                receivedAmount = amount;
                receivedDelta = delta;
            };

            // Act
            _economyManager.TrySpend(30);

            // Assert
            Assert.AreEqual(70, receivedAmount, "OnCurrencyChanged should receive correct amount");
            Assert.AreEqual(-30, receivedDelta, "OnCurrencyChanged should receive negative delta when spending");
        }

        [Test]
        public void Reset_ResetsToInitialState()
        {
            // Arrange
            _economyManager.Initialize(100);
            _economyManager.AddCurrency(50);
            _economyManager.TrySpend(30);

            // Act
            _economyManager.Reset();

            // Assert - Reset calls Initialize() which uses default _startingCurrency (200)
            Assert.AreEqual(200, _economyManager.CurrentCurrency, "Currency should be reset to default starting value");
            Assert.AreEqual(200, _economyManager.TotalEarned, "TotalEarned should be reset");
            Assert.AreEqual(0, _economyManager.TotalSpent, "TotalSpent should be reset to 0");
        }

        [Test]
        public void SetStateForTesting_SetsInternalState()
        {
            // Arrange
            _economyManager.Initialize(100);

            // Act
            _economyManager.SetStateForTesting(currency: 500, totalEarned: 1000, totalSpent: 500);

            // Assert
            Assert.AreEqual(500, _economyManager.CurrentCurrency, "Currency should be set");
            Assert.AreEqual(1000, _economyManager.TotalEarned, "TotalEarned should be set");
            Assert.AreEqual(500, _economyManager.TotalSpent, "TotalSpent should be set");
        }

        #region Wave Bonus Event Flow Tests (TC-06)

        [Test]
        public void HandleWaveCompleted_AddsCurrencyWithCorrectBonus()
        {
            // Arrange
            _economyManager.Initialize(100);

            // Get the private HandleWaveCompleted method via reflection
            var handleWaveCompletedMethod = typeof(EconomyManager).GetMethod(
                "HandleWaveCompleted",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(handleWaveCompletedMethod, "HandleWaveCompleted method should exist");

            // Act - Invoke HandleWaveCompleted for wave 3
            // Expected bonus: _baseWaveBonus (50) + (waveNumber * _waveBonusIncrement) = 50 + (3 * 10) = 80
            handleWaveCompletedMethod.Invoke(_economyManager, new object[] { 3 });

            // Assert
            Assert.AreEqual(180, _economyManager.CurrentCurrency, "Currency should increase by wave bonus (100 + 80 = 180)");
            Assert.AreEqual(180, _economyManager.TotalEarned, "TotalEarned should include wave bonus");
        }

        [Test]
        public void HandleWaveCompleted_ForMultipleWaves_AccumulatesBonuses()
        {
            // Arrange
            _economyManager.Initialize(100);

            // Get the private HandleWaveCompleted method via reflection
            var handleWaveCompletedMethod = typeof(EconomyManager).GetMethod(
                "HandleWaveCompleted",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(handleWaveCompletedMethod, "HandleWaveCompleted method should exist");

            // Act - Complete waves 1, 2, and 3
            // Wave 1: 50 + 10 = 60
            // Wave 2: 50 + 20 = 70
            // Wave 3: 50 + 30 = 80
            // Total: 60 + 70 + 80 = 210
            handleWaveCompletedMethod.Invoke(_economyManager, new object[] { 1 });
            handleWaveCompletedMethod.Invoke(_economyManager, new object[] { 2 });
            handleWaveCompletedMethod.Invoke(_economyManager, new object[] { 3 });

            // Assert
            Assert.AreEqual(310, _economyManager.CurrentCurrency, "Currency should be 100 + 210 = 310");
        }

        #endregion

        #region Interest Calculation Tests (TC-07)

        [Test]
        public void CalculateInterest_WhenEnabled_ReturnsCorrectInterest()
        {
            // Arrange
            _economyManager.Initialize(1000);

            // Enable interest via reflection (default rate is 0.05 = 5%)
            SetPrivateField(_economyManager, "_enableInterest", true);

            // Act - Interest on 1000 at 5% = 50
            int interest = _economyManager.CalculateInterest();

            // Assert
            Assert.AreEqual(50, interest, "Interest on 1000 at 5% should be 50");
        }

        [Test]
        public void CalculateInterest_WhenEnabled_ClampsToCap()
        {
            // Arrange - Set currency high enough that interest would exceed cap
            _economyManager.Initialize(2000);

            // Enable interest (default cap is 50, rate is 0.05)
            // 2000 * 0.05 = 100, but cap is 50
            SetPrivateField(_economyManager, "_enableInterest", true);

            // Act
            int interest = _economyManager.CalculateInterest();

            // Assert
            Assert.AreEqual(50, interest, "Interest should be clamped to cap of 50");
        }

        [Test]
        public void CalculateInterest_WhenEnabled_AtExactCap_ReturnsCap()
        {
            // Arrange - Set currency so interest exactly equals cap
            // 1000 * 0.05 = 50 (exactly at cap)
            _economyManager.Initialize(1000);
            SetPrivateField(_economyManager, "_enableInterest", true);

            // Act
            int interest = _economyManager.CalculateInterest();

            // Assert
            Assert.AreEqual(50, interest, "Interest at exactly cap should return cap value");
        }

        [Test]
        public void CalculateInterest_WhenEnabled_BelowCap_ReturnsCalculatedInterest()
        {
            // Arrange - Set currency so interest is below cap
            // 500 * 0.05 = 25 (below cap of 50)
            _economyManager.Initialize(500);
            SetPrivateField(_economyManager, "_enableInterest", true);

            // Act
            int interest = _economyManager.CalculateInterest();

            // Assert
            Assert.AreEqual(25, interest, "Interest below cap should return calculated value");
        }

        [Test]
        public void HandleWaveCompleted_WithInterestEnabled_AddsWaveBonusAndInterest()
        {
            // Arrange
            _economyManager.Initialize(1000);

            // Enable interest
            SetPrivateField(_economyManager, "_enableInterest", true);

            // Get the private HandleWaveCompleted method via reflection
            var handleWaveCompletedMethod = typeof(EconomyManager).GetMethod(
                "HandleWaveCompleted",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(handleWaveCompletedMethod, "HandleWaveCompleted method should exist");

            // Act - Complete wave 1
            // Wave bonus: 50 + (1 * 10) = 60
            // After wave bonus: 1000 + 60 = 1060
            // Interest: 1060 * 0.05 = 53, but capped at 50
            // Final: 1060 + 50 = 1110
            handleWaveCompletedMethod.Invoke(_economyManager, new object[] { 1 });

            // Assert
            Assert.AreEqual(1110, _economyManager.CurrentCurrency, "Currency should include wave bonus and interest");
        }

        #endregion

        #region Helper Methods

        private void SetPrivateField<T>(object target, string fieldName, T value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Field '{fieldName}' not found on type {target.GetType().Name}");
            field.SetValue(target, value);
        }

        #endregion
    }
}
