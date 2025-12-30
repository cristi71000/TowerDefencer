using System;
using UnityEngine;

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
        public int CurrentCurrency { get; private set; }
        public int CurrentWave { get; private set; }

        // Events
        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnLivesChanged;
        public event Action<int> OnCurrencyChanged;
        public event Action<int> OnWaveChanged;

        private GameState _previousState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void InitializeGame()
        {
            CurrentLives = _startingLives;
            CurrentCurrency = _startingCurrency;
            CurrentWave = 0;
            _previousState = GameState.Initializing;
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

        public void ModifyCurrency(int delta)
        {
            int previousCurrency = CurrentCurrency;
            CurrentCurrency = Mathf.Max(0, CurrentCurrency + delta);

            if (CurrentCurrency != previousCurrency)
            {
                OnCurrencyChanged?.Invoke(CurrentCurrency);
            }
        }

        public bool TrySpendCurrency(int amount)
        {
            if (amount < 0)
            {
                UnityEngine.Debug.LogWarning("TrySpendCurrency called with negative amount. Use ModifyCurrency to add currency.");
                return false;
            }

            if (CurrentCurrency >= amount)
            {
                ModifyCurrency(-amount);
                return true;
            }

            return false;
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
            CurrentCurrency = currency;
            CurrentState = state;
        }
    }
}
