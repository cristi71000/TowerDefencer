## Context

Player progress (level completion, stars, settings) must persist between sessions. This issue implements a save/load system using PlayerPrefs or JSON file storage.

**Builds upon:** Issue 48 (Level Select)

## Detailed Implementation Instructions

### Save Data Structure

Create `SaveData.cs`:

```csharp
using System;
using System.Collections.Generic;

namespace TowerDefense.Core
{
    [Serializable]
    public class SaveData
    {
        public int Version = 1;
        public DateTime LastSaved;

        // Level Progress
        public List<LevelProgress> LevelProgress = new List<LevelProgress>();

        // Settings
        public float MasterVolume = 1f;
        public float MusicVolume = 0.5f;
        public float SFXVolume = 1f;

        // Stats
        public int TotalEnemiesKilled;
        public int TotalGamesPlayed;
        public int TotalGamesWon;
    }

    [Serializable]
    public class LevelProgress
    {
        public int LevelIndex;
        public bool IsComplete;
        public int StarsEarned;
        public int BestLivesRemaining;
        public float BestTime;
    }
}
```

### Save Manager

Create `SaveManager.cs`:

```csharp
using UnityEngine;
using System.IO;

namespace TowerDefense.Core
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [SerializeField] private bool _useJsonFile = true;
        private string _saveFileName = "towerdefense_save.json";

        private SaveData _currentSave;

        public SaveData CurrentSave => _currentSave;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadGame();
        }

        private string SavePath => Path.Combine(Application.persistentDataPath, _saveFileName);

        public void SaveGame()
        {
            _currentSave.LastSaved = System.DateTime.Now;

            if (_useJsonFile)
            {
                string json = JsonUtility.ToJson(_currentSave, true);
                File.WriteAllText(SavePath, json);
            }
            else
            {
                string json = JsonUtility.ToJson(_currentSave);
                PlayerPrefs.SetString("SaveData", json);
                PlayerPrefs.Save();
            }

            Debug.Log("Game saved!");
        }

        public void LoadGame()
        {
            bool loaded = false;

            if (_useJsonFile && File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                _currentSave = JsonUtility.FromJson<SaveData>(json);
                loaded = true;
            }
            else if (!_useJsonFile && PlayerPrefs.HasKey("SaveData"))
            {
                string json = PlayerPrefs.GetString("SaveData");
                _currentSave = JsonUtility.FromJson<SaveData>(json);
                loaded = true;
            }

            if (!loaded)
            {
                _currentSave = new SaveData();
                Debug.Log("Created new save file");
            }
            else
            {
                Debug.Log("Save loaded!");
            }
        }

        public void DeleteSave()
        {
            if (_useJsonFile && File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
            else
            {
                PlayerPrefs.DeleteKey("SaveData");
            }

            _currentSave = new SaveData();
            Debug.Log("Save deleted!");
        }

        // Level Progress Helpers
        public bool IsLevelComplete(int levelIndex)
        {
            var progress = GetLevelProgress(levelIndex);
            return progress?.IsComplete ?? false;
        }

        public int GetLevelStars(int levelIndex)
        {
            var progress = GetLevelProgress(levelIndex);
            return progress?.StarsEarned ?? 0;
        }

        public void SetLevelComplete(int levelIndex, int stars, int livesRemaining, float time)
        {
            var progress = GetOrCreateLevelProgress(levelIndex);

            progress.IsComplete = true;
            progress.StarsEarned = Mathf.Max(progress.StarsEarned, stars);
            progress.BestLivesRemaining = Mathf.Max(progress.BestLivesRemaining, livesRemaining);

            if (progress.BestTime == 0 || time < progress.BestTime)
                progress.BestTime = time;

            SaveGame();
        }

        private LevelProgress GetLevelProgress(int levelIndex)
        {
            return _currentSave.LevelProgress.Find(p => p.LevelIndex == levelIndex);
        }

        private LevelProgress GetOrCreateLevelProgress(int levelIndex)
        {
            var progress = GetLevelProgress(levelIndex);
            if (progress == null)
            {
                progress = new LevelProgress { LevelIndex = levelIndex };
                _currentSave.LevelProgress.Add(progress);
            }
            return progress;
        }

        // Settings
        public void SetVolumes(float master, float music, float sfx)
        {
            _currentSave.MasterVolume = master;
            _currentSave.MusicVolume = music;
            _currentSave.SFXVolume = sfx;
            SaveGame();
        }

        // Stats
        public void IncrementStat(StatType stat, int amount = 1)
        {
            switch (stat)
            {
                case StatType.EnemiesKilled:
                    _currentSave.TotalEnemiesKilled += amount;
                    break;
                case StatType.GamesPlayed:
                    _currentSave.TotalGamesPlayed += amount;
                    break;
                case StatType.GamesWon:
                    _currentSave.TotalGamesWon += amount;
                    break;
            }
        }
    }

    public enum StatType
    {
        EnemiesKilled,
        GamesPlayed,
        GamesWon
    }
}
```

### Auto-Save on Level Complete

Update GameConditionChecker:

```csharp
private void HandleVictory()
{
    // ... existing victory code

    // Save progress
    var level = LevelSelectManager.SelectedLevel;
    if (level != null)
    {
        int lives = LivesManager.Instance.CurrentLives;
        int stars = CalculateStars(lives, level);
        float time = GameStats.Instance.PlayTime;

        SaveManager.Instance.SetLevelComplete(level.LevelIndex, stars, lives, time);
    }
}

private int CalculateStars(int livesRemaining, LevelData level)
{
    if (livesRemaining >= level.StarThreshold3) return 3;
    if (livesRemaining >= level.StarThreshold2) return 2;
    if (livesRemaining >= level.StarThreshold1) return 1;
    return 0;
}
```

### Save Persistence Location

- **Windows:** `C:\Users\Username\AppData\LocalLow\CompanyName\ProductName\`
- **Mac:** `~/Library/Application Support/CompanyName/ProductName/`

## Testing and Acceptance Criteria

### Done When

- [ ] SaveData structure stores all needed info
- [ ] SaveManager persists data to file
- [ ] Level completion saves correctly
- [ ] Stars calculated and saved
- [ ] Settings persist between sessions
- [ ] Save loads on game start
- [ ] Delete save option works
- [ ] No data loss on normal exit

## Dependencies

- Issue 48: Level Select
