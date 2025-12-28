using NUnit.Framework;
using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.Tests.EditMode
{
    [TestFixture]
    public class GameManagerTests
    {
        private GameObject _gameManagerObject;
        private GameManager _gameManager;

        [SetUp]
        public void SetUp()
        {
            _gameManagerObject = new GameObject("GameManager");
            _gameManager = _gameManagerObject.AddComponent<GameManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gameManagerObject != null)
            {
                Object.DestroyImmediate(_gameManagerObject);
            }
        }

        [Test]
        public void TrySpendCurrency_WithSufficientFunds_ReturnsTrue()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 100, state: GameState.PreWave);

            // Act
            bool result = _gameManager.TrySpendCurrency(50);

            // Assert
            Assert.IsTrue(result, "TrySpendCurrency should return true when funds are sufficient");
            Assert.AreEqual(50, _gameManager.CurrentCurrency, "Currency should be reduced by the spent amount");
        }

        [Test]
        public void TrySpendCurrency_WithExactFunds_ReturnsTrue()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 100, state: GameState.PreWave);

            // Act
            bool result = _gameManager.TrySpendCurrency(100);

            // Assert
            Assert.IsTrue(result, "TrySpendCurrency should return true when funds exactly match");
            Assert.AreEqual(0, _gameManager.CurrentCurrency, "Currency should be 0 after spending all");
        }

        [Test]
        public void TrySpendCurrency_WithInsufficientFunds_ReturnsFalse()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 50, state: GameState.PreWave);

            // Act
            bool result = _gameManager.TrySpendCurrency(100);

            // Assert
            Assert.IsFalse(result, "TrySpendCurrency should return false when funds are insufficient");
            Assert.AreEqual(50, _gameManager.CurrentCurrency, "Currency should remain unchanged");
        }

        [Test]
        public void TrySpendCurrency_WithZeroFunds_ReturnsFalse()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 0, state: GameState.PreWave);

            // Act
            bool result = _gameManager.TrySpendCurrency(10);

            // Assert
            Assert.IsFalse(result, "TrySpendCurrency should return false when funds are zero");
            Assert.AreEqual(0, _gameManager.CurrentCurrency, "Currency should remain zero");
        }

        [Test]
        public void TrySpendCurrency_WithNegativeAmount_ReturnsFalse()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 100, state: GameState.PreWave);

            // Act
            bool result = _gameManager.TrySpendCurrency(-50);

            // Assert
            Assert.IsFalse(result, "TrySpendCurrency should return false for negative amounts");
            Assert.AreEqual(100, _gameManager.CurrentCurrency, "Currency should remain unchanged");
        }

        [Test]
        public void ModifyLives_WhenLivesReachZero_TriggersDefeat()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 5, currency: 100, state: GameState.WaveActive);

            // Act
            _gameManager.ModifyLives(-5);

            // Assert
            Assert.AreEqual(0, _gameManager.CurrentLives, "Lives should be 0");
            Assert.AreEqual(GameState.Defeat, _gameManager.CurrentState, "Game state should be Defeat");
        }

        [Test]
        public void ModifyLives_WhenLivesGoBelowZero_ClampsToZeroAndTriggersDefeat()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 3, currency: 100, state: GameState.WaveActive);

            // Act
            _gameManager.ModifyLives(-10);

            // Assert
            Assert.AreEqual(0, _gameManager.CurrentLives, "Lives should be clamped to 0");
            Assert.AreEqual(GameState.Defeat, _gameManager.CurrentState, "Game state should be Defeat");
        }

        [Test]
        public void ModifyLives_WhenLivesRemainPositive_DoesNotTriggerDefeat()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 10, currency: 100, state: GameState.WaveActive);

            // Act
            _gameManager.ModifyLives(-5);

            // Assert
            Assert.AreEqual(5, _gameManager.CurrentLives, "Lives should be reduced correctly");
            Assert.AreEqual(GameState.WaveActive, _gameManager.CurrentState, "Game state should remain WaveActive");
        }

        [Test]
        public void ModifyLives_AddingLives_IncreasesCorrectly()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 10, currency: 100, state: GameState.PreWave);

            // Act
            _gameManager.ModifyLives(5);

            // Assert
            Assert.AreEqual(15, _gameManager.CurrentLives, "Lives should increase correctly");
        }

        [Test]
        public void ModifyCurrency_AddingCurrency_IncreasesCorrectly()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 100, state: GameState.PreWave);

            // Act
            _gameManager.ModifyCurrency(50);

            // Assert
            Assert.AreEqual(150, _gameManager.CurrentCurrency, "Currency should increase correctly");
        }

        [Test]
        public void ModifyCurrency_RemovingCurrency_DecreasesCorrectly()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 100, state: GameState.PreWave);

            // Act
            _gameManager.ModifyCurrency(-30);

            // Assert
            Assert.AreEqual(70, _gameManager.CurrentCurrency, "Currency should decrease correctly");
        }

        [Test]
        public void ModifyCurrency_ClampsToZero()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 50, state: GameState.PreWave);

            // Act
            _gameManager.ModifyCurrency(-100);

            // Assert
            Assert.AreEqual(0, _gameManager.CurrentCurrency, "Currency should be clamped to 0");
        }

        [Test]
        public void SetGameState_ChangesState()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 100, state: GameState.PreWave);

            // Act
            _gameManager.SetGameState(GameState.WaveActive);

            // Assert
            Assert.AreEqual(GameState.WaveActive, _gameManager.CurrentState, "State should change to WaveActive");
        }

        [Test]
        public void SetGameState_SameState_DoesNothing()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 100, state: GameState.PreWave);
            bool eventFired = false;
            _gameManager.OnGameStateChanged += (state) => eventFired = true;

            // Act
            _gameManager.SetGameState(GameState.PreWave);

            // Assert
            Assert.IsFalse(eventFired, "OnGameStateChanged should not fire when state is the same");
        }

        [Test]
        public void AdvanceWave_IncrementsWaveNumber()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 100, state: GameState.PreWave);
            int initialWave = _gameManager.CurrentWave;

            // Act
            _gameManager.AdvanceWave();

            // Assert
            Assert.AreEqual(initialWave + 1, _gameManager.CurrentWave, "Wave should increment by 1");
        }

        [Test]
        public void OnLivesChanged_FiresWhenLivesModified()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 100, state: GameState.PreWave);
            int receivedLives = -1;
            _gameManager.OnLivesChanged += (lives) => receivedLives = lives;

            // Act
            _gameManager.ModifyLives(-5);

            // Assert
            Assert.AreEqual(15, receivedLives, "OnLivesChanged should fire with correct value");
        }

        [Test]
        public void OnCurrencyChanged_FiresWhenCurrencyModified()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 100, state: GameState.PreWave);
            int receivedCurrency = -1;
            _gameManager.OnCurrencyChanged += (currency) => receivedCurrency = currency;

            // Act
            _gameManager.ModifyCurrency(25);

            // Assert
            Assert.AreEqual(125, receivedCurrency, "OnCurrencyChanged should fire with correct value");
        }

        [Test]
        public void OnWaveChanged_FiresWhenWaveAdvanced()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 100, state: GameState.PreWave);
            int receivedWave = -1;
            _gameManager.OnWaveChanged += (wave) => receivedWave = wave;

            // Act
            _gameManager.AdvanceWave();

            // Assert
            Assert.AreEqual(1, receivedWave, "OnWaveChanged should fire with correct wave number");
        }

        [Test]
        public void OnGameStateChanged_FiresWhenStateChanges()
        {
            // Arrange
            _gameManager.SetStateForTesting(lives: 20, currency: 100, state: GameState.PreWave);
            GameState receivedState = GameState.Initializing;
            _gameManager.OnGameStateChanged += (state) => receivedState = state;

            // Act
            _gameManager.SetGameState(GameState.WaveActive);

            // Assert
            Assert.AreEqual(GameState.WaveActive, receivedState, "OnGameStateChanged should fire with correct state");
        }
    }
}
