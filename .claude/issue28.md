## Context

Victory and defeat conditions need proper triggers based on wave completion and lives. This issue ensures the win/lose states are properly detected and communicated to the game state system.

**Builds upon:** Issues 2, 23, 26 (GameManager, Lives, Waves)

## Detailed Implementation Instructions

### Game Condition Checker

Create `GameConditionChecker.cs` in `_Project/Scripts/Runtime/Core/`:

```csharp
using UnityEngine;
using TowerDefense.Waves;

namespace TowerDefense.Core
{
    public class GameConditionChecker : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool _logConditions = true;

        private bool _gameEnded;

        private void Start()
        {
            _gameEnded = false;

            // Subscribe to relevant events
            if (LivesManager.Instance != null)
            {
                LivesManager.Instance.OnGameOver += HandleDefeat;
            }

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnAllWavesCompleted += HandleVictory;
            }
        }

        private void OnDestroy()
        {
            if (LivesManager.Instance != null)
            {
                LivesManager.Instance.OnGameOver -= HandleDefeat;
            }

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnAllWavesCompleted -= HandleVictory;
            }
        }

        private void HandleVictory()
        {
            if (_gameEnded) return;
            _gameEnded = true;

            if (_logConditions)
                Debug.Log("VICTORY - All waves completed!");

            GameManager.Instance?.SetGameState(GameState.Victory);
        }

        private void HandleDefeat()
        {
            if (_gameEnded) return;
            _gameEnded = true;

            if (_logConditions)
                Debug.Log("DEFEAT - Lives depleted!");

            GameManager.Instance?.SetGameState(GameState.Defeat);
        }

        public void ForceVictory()
        {
            HandleVictory();
        }

        public void ForceDefeat()
        {
            HandleDefeat();
        }
    }
}
```

### Update GameManager State Handling

Ensure GameManager handles end states:

```csharp
// In GameManager.cs
public void SetGameState(GameState newState)
{
    if (CurrentState == newState) return;

    GameState previousState = CurrentState;
    CurrentState = newState;

    switch (newState)
    {
        case GameState.Paused:
            Time.timeScale = 0f;
            break;

        case GameState.Victory:
        case GameState.Defeat:
            Time.timeScale = 0f; // Or slow-mo: 0.2f
            break;

        default:
            Time.timeScale = 1f;
            break;
    }

    OnGameStateChanged?.Invoke(newState);
    Debug.Log($"Game state: {previousState} -> {newState}");
}
```

### Victory/Defeat Stats Tracker

Create `GameStats.cs`:

```csharp
using UnityEngine;

namespace TowerDefense.Core
{
    public class GameStats : MonoBehaviour
    {
        public static GameStats Instance { get; private set; }

        private int _enemiesKilled;
        private int _towersPlaced;
        private int _currencyEarned;
        private int _currencySpent;
        private float _playTime;
        private int _wavesCompleted;

        public int EnemiesKilled => _enemiesKilled;
        public int TowersPlaced => _towersPlaced;
        public int CurrencyEarned => _currencyEarned;
        public int CurrencySpent => _currencySpent;
        public float PlayTime => _playTime;
        public int WavesCompleted => _wavesCompleted;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentState == GameState.WaveActive ||
                GameManager.Instance?.CurrentState == GameState.PreWave)
            {
                _playTime += Time.deltaTime;
            }
        }

        public void RecordEnemyKill()
        {
            _enemiesKilled++;
        }

        public void RecordTowerPlaced()
        {
            _towersPlaced++;
        }

        public void RecordWaveCompleted()
        {
            _wavesCompleted++;
        }

        public void Reset()
        {
            _enemiesKilled = 0;
            _towersPlaced = 0;
            _currencyEarned = 0;
            _currencySpent = 0;
            _playTime = 0f;
            _wavesCompleted = 0;
        }
    }
}
```

### Update Victory/Defeat Screens with Stats

```csharp
// In VictoryScreen or DefeatScreen component
public void ShowStats()
{
    if (GameStats.Instance == null) return;

    _enemiesKilledText.text = $"Enemies Killed: {GameStats.Instance.EnemiesKilled}";
    _wavesCompletedText.text = $"Waves Completed: {GameStats.Instance.WavesCompleted}";
    _playTimeText.text = $"Time: {FormatTime(GameStats.Instance.PlayTime)}";
}

private string FormatTime(float seconds)
{
    int mins = Mathf.FloorToInt(seconds / 60f);
    int secs = Mathf.FloorToInt(seconds % 60f);
    return $"{mins}:{secs:00}";
}
```

## Testing and Acceptance Criteria

### Done When

- [ ] Victory triggers when all waves complete
- [ ] Defeat triggers when lives reach 0
- [ ] Game time freezes on end state
- [ ] Stats tracked during gameplay
- [ ] Stats displayed on end screens
- [ ] Cannot trigger both win and lose
- [ ] Force win/lose for testing works

## Dependencies

- Issue 2: GameManager
- Issue 23: Lives System
- Issue 26: Wave Manager
