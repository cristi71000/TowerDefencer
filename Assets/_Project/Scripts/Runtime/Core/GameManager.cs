using System;
using UnityEngine;
using TowerDefense.Economy;

namespace TowerDefense.Core
{
    public enum GameState
    {
        Initializing,
        PreWave,      // Before first wave or between waves
        WaveActive,   // Enemies spawning/alive
        Paused,
        Victory,
        Defeat
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Starting Values")]
        [SerializeField] private int _startingLives = 20;
        [SerializeField] private int _startingCurrency = 100;

        public GameState CurrentState { get; private set; }
        public int CurrentLives { get; private set; }

        /// <summary>
        /// Current currency balance. Delegates to EconomyManager if available.
        /// </summary>
        public int CurrentCurrency => EconomyManager.Instance != null
            ? EconomyManager.Instance.CurrentCurrency
            : _fallbackCurrency;

        public int CurrentWave { get; private set; }

        // Fallback currency for when EconomyManager is not available
        private int _fallbackCurrency;

        // Events
        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnLivesChanged;
        public event Action<int> OnCurrencyChanged;
        public event Action<int> OnWaveChanged;

        private GameState _previousState;
        private bool _isSubscribedToEconomyManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Attempt to subscribe in Start, after all Awake calls have completed
            TrySubscribeToEconomyManager();
        }

        private void OnEnable()
        {
            // Re-subscribe after disable/enable cycle
            TrySubscribeToEconomyManager();
        }

        private void OnDisable()
        {
            UnsubscribeFromEconomyManager();
        }

        private void TrySubscribeToEconomyManager()
        {
            if (_isSubscribedToEconomyManager)
            {
                return;
            }

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnCurrencyChanged += HandleEconomyCurrencyChanged;
                _isSubscribedToEconomyManager = true;
            }
        }

        private void UnsubscribeFromEconomyManager()
        {
            if (!_isSubscribedToEconomyManager)
            {
                return;
            }

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnCurrencyChanged -= HandleEconomyCurrencyChanged;
            }
            _isSubscribedToEconomyManager = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void HandleEconomyCurrencyChanged(int newAmount, int delta)
        {
            // Relay the event through GameManager for backwards compatibility
            OnCurrencyChanged?.Invoke(newAmount);
        }

        public void InitializeGame()
        {
            CurrentLives = _startingLives;
            _fallbackCurrency = _startingCurrency;
            CurrentWave = 0;
            _previousState = GameState.Initializing;

            // Ensure subscription to EconomyManager before initialization
            TrySubscribeToEconomyManager();

            // Initialize EconomyManager if available
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.Initialize(_startingCurrency);
            }

            SetGameState(GameState.PreWave);

            OnLivesChanged?.Invoke(CurrentLives);
            OnCurrencyChanged?.Invoke(CurrentCurrency);
        }

        public void SetGameState(GameState newState)
        {
            if (CurrentState == newState)
            {
                return;
            }

            _previousState = CurrentState;
            CurrentState = newState;
            OnGameStateChanged?.Invoke(CurrentState);
        }

        public void ModifyLives(int delta)
        {
            int previousLives = CurrentLives;
            CurrentLives = Mathf.Max(0, CurrentLives + delta);

            if (CurrentLives != previousLives)
            {
                OnLivesChanged?.Invoke(CurrentLives);
            }

            if (CurrentLives <= 0 && CurrentState != GameState.Defeat)
            {
                SetGameState(GameState.Defeat);
            }
        }

        /// <summary>
        /// Modifies currency by the specified delta. Delegates to EconomyManager if available.
        /// </summary>
        /// <param name="delta">Amount to add (positive) or subtract (negative).</param>
        public void ModifyCurrency(int delta)
        {
            if (EconomyManager.Instance != null)
            {
                if (delta > 0)
                {
                    EconomyManager.Instance.AddCurrency(delta);
                }
                else if (delta < 0)
                {
                    bool success = EconomyManager.Instance.TrySpend(-delta);
                    if (!success)
                    {
                        UnityEngine.Debug.LogWarning($"[GameManager] ModifyCurrency: TrySpend failed for amount {-delta}. Insufficient funds.");
                    }
                }
                // Note: OnCurrencyChanged is fired through HandleEconomyCurrencyChanged
            }
            else
            {
                // Fallback behavior when EconomyManager is not available
                int previousCurrency = _fallbackCurrency;
                _fallbackCurrency = Mathf.Max(0, _fallbackCurrency + delta);

                if (_fallbackCurrency != previousCurrency)
                {
                    OnCurrencyChanged?.Invoke(_fallbackCurrency);
                }
            }
        }

        /// <summary>
        /// Attempts to spend the specified amount of currency. Delegates to EconomyManager if available.
        /// </summary>
        /// <param name="amount">Amount to spend (must be positive).</param>
        /// <returns>True if the purchase was successful, false if insufficient funds.</returns>
        public bool TrySpendCurrency(int amount)
        {
            if (amount < 0)
            {
                UnityEngine.Debug.LogWarning("TrySpendCurrency called with negative amount. Use ModifyCurrency to add currency.");
                return false;
            }

            if (EconomyManager.Instance != null)
            {
                return EconomyManager.Instance.TrySpend(amount);
            }

            // Fallback behavior
            if (_fallbackCurrency >= amount)
            {
                ModifyCurrency(-amount);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the player can afford the specified amount. Delegates to EconomyManager if available.
        /// </summary>
        /// <param name="amount">Amount to check.</param>
        /// <returns>True if the player has enough currency.</returns>
        public bool CanAfford(int amount)
        {
            if (EconomyManager.Instance != null)
            {
                return EconomyManager.Instance.CanAfford(amount);
            }

            return _fallbackCurrency >= amount;
        }

        public void AdvanceWave()
        {
            CurrentWave++;
            OnWaveChanged?.Invoke(CurrentWave);
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Paused ||
                CurrentState == GameState.Victory ||
                CurrentState == GameState.Defeat)
            {
                return;
            }

            _previousState = CurrentState;
            SetGameState(GameState.Paused);
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused)
            {
                return;
            }

            Time.timeScale = 1f;
            SetGameState(_previousState);
        }

        // For testing purposes - allows setting internal state without triggering events
        internal void SetStateForTesting(int lives, int currency, GameState state)
        {
            CurrentLives = lives;
            _fallbackCurrency = currency;
            CurrentState = state;
        }
    }
}
