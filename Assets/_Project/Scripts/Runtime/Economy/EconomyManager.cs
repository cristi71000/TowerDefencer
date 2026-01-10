using System;
using UnityEngine;
using TowerDefense.Core;
using TowerDefense.Core.Events;

namespace TowerDefense.Economy
{
    /// <summary>
    /// Manages player currency including wave bonuses and optional interest mechanics.
    /// Singleton pattern ensures single source of truth for economy state.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Starting Values")]
        [SerializeField] private int _startingCurrency = 200;

        [Header("Wave Bonuses")]
        [SerializeField] private int _baseWaveBonus = 50;
        [SerializeField] private int _waveBonusIncrement = 10;
        [SerializeField] private bool _enableInterest = false;
        [SerializeField] private float _interestRate = 0.05f;
        [SerializeField] private int _interestCap = 50;

        [Header("Events")]
        [SerializeField] private IntEventChannel _onCurrencyChanged;
        [SerializeField] private IntEventChannel _onWaveCompleted;

        private int _currentCurrency;
        private int _totalEarned;
        private int _totalSpent;

        /// <summary>
        /// Current player currency balance.
        /// </summary>
        public int CurrentCurrency => _currentCurrency;

        /// <summary>
        /// Total currency earned during the game session.
        /// </summary>
        public int TotalEarned => _totalEarned;

        /// <summary>
        /// Total currency spent during the game session.
        /// </summary>
        public int TotalSpent => _totalSpent;

        /// <summary>
        /// Event fired when currency changes. Parameters: newAmount, delta.
        /// </summary>
        public event Action<int, int> OnCurrencyChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            if (_onWaveCompleted != null)
            {
                _onWaveCompleted.OnEventRaised += HandleWaveCompleted;
            }
        }

        private void OnDisable()
        {
            if (_onWaveCompleted != null)
            {
                _onWaveCompleted.OnEventRaised -= HandleWaveCompleted;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Initializes the economy system with starting currency.
        /// Call this at the start of a new game.
        /// </summary>
        public void Initialize()
        {
            _currentCurrency = _startingCurrency;
            _totalEarned = _startingCurrency;
            _totalSpent = 0;
            NotifyCurrencyChanged(0);
        }

        /// <summary>
        /// Initializes the economy system with a custom starting currency amount.
        /// </summary>
        /// <param name="startingAmount">The amount of currency to start with.</param>
        public void Initialize(int startingAmount)
        {
            _currentCurrency = startingAmount;
            _totalEarned = startingAmount;
            _totalSpent = 0;
            NotifyCurrencyChanged(0);
        }

        /// <summary>
        /// Adds currency to the player's balance.
        /// </summary>
        /// <param name="amount">Amount to add (must be positive).</param>
        public void AddCurrency(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _currentCurrency += amount;
            _totalEarned += amount;
            NotifyCurrencyChanged(amount);
        }

        /// <summary>
        /// Attempts to spend the specified amount of currency.
        /// </summary>
        /// <param name="amount">Amount to spend (must be positive).</param>
        /// <returns>True if the purchase was successful, false if insufficient funds.</returns>
        public bool TrySpend(int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            if (_currentCurrency < amount)
            {
                return false;
            }

            _currentCurrency -= amount;
            _totalSpent += amount;
            NotifyCurrencyChanged(-amount);
            return true;
        }

        /// <summary>
        /// Checks if the player can afford the specified amount.
        /// </summary>
        /// <param name="amount">Amount to check.</param>
        /// <returns>True if the player has enough currency.</returns>
        public bool CanAfford(int amount)
        {
            return _currentCurrency >= amount;
        }

        /// <summary>
        /// Calculates the wave bonus for the specified wave number.
        /// </summary>
        /// <param name="waveNumber">The wave number (1-based).</param>
        /// <returns>The bonus amount for completing the wave.</returns>
        public int CalculateWaveBonus(int waveNumber)
        {
            return _baseWaveBonus + (waveNumber * _waveBonusIncrement);
        }

        /// <summary>
        /// Calculates the interest earned on current savings.
        /// </summary>
        /// <returns>The interest amount (capped by interestCap).</returns>
        public int CalculateInterest()
        {
            if (!_enableInterest)
            {
                return 0;
            }

            return Mathf.Min(
                Mathf.FloorToInt(_currentCurrency * _interestRate),
                _interestCap
            );
        }

        private void HandleWaveCompleted(int waveNumber)
        {
            // Wave completion bonus
            int waveBonus = CalculateWaveBonus(waveNumber);
            AddCurrency(waveBonus);

            UnityEngine.Debug.Log($"[EconomyManager] Wave {waveNumber} bonus: {waveBonus}");

            // Interest on saved currency
            if (_enableInterest)
            {
                int interest = CalculateInterest();
                if (interest > 0)
                {
                    AddCurrency(interest);
                    UnityEngine.Debug.Log($"[EconomyManager] Interest earned: {interest}");
                }
            }
        }

        private void NotifyCurrencyChanged(int delta)
        {
            OnCurrencyChanged?.Invoke(_currentCurrency, delta);

            if (_onCurrencyChanged != null)
            {
                _onCurrencyChanged.RaiseEvent(_currentCurrency);
            }
        }

        /// <summary>
        /// Resets the economy to initial state.
        /// </summary>
        public void Reset()
        {
            Initialize();
        }

        /// <summary>
        /// For testing purposes - allows setting internal state without triggering events.
        /// </summary>
        internal void SetStateForTesting(int currency, int totalEarned, int totalSpent)
        {
            _currentCurrency = currency;
            _totalEarned = totalEarned;
            _totalSpent = totalSpent;
        }
    }
}
