## Context

Players need a level selection screen to choose which map to play and see their progress. This supports multiple maps and replayability.

**Builds upon:** Issue 24 (Game state screens)

## Detailed Implementation Instructions

### Level Data ScriptableObject

Create `LevelData.cs`:

```csharp
using UnityEngine;
using TowerDefense.Waves;

namespace TowerDefense.Core
{
    [CreateAssetMenu(fileName = "NewLevel", menuName = "TD/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Identity")]
        public string LevelName;
        public string Description;
        public Sprite Thumbnail;
        public int LevelIndex;

        [Header("Scene")]
        public string SceneName;

        [Header("Configuration")]
        public LevelWaveConfig WaveConfig;
        public int StartingCurrency = 200;
        public int StartingLives = 20;

        [Header("Difficulty")]
        public DifficultyRating Difficulty;
        public int EstimatedMinutes;

        [Header("Unlock")]
        public bool IsLockedByDefault = false;
        public LevelData RequiredLevel;

        [Header("Rewards")]
        public int StarThreshold1 = 10; // Lives remaining for 1 star
        public int StarThreshold2 = 15; // Lives remaining for 2 stars
        public int StarThreshold3 = 20; // Lives remaining for 3 stars
    }

    public enum DifficultyRating
    {
        Easy,
        Medium,
        Hard,
        Expert
    }
}
```

### Level Select Manager

Create `LevelSelectManager.cs`:

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefense.UI
{
    public class LevelSelectManager : MonoBehaviour
    {
        [SerializeField] private LevelData[] _allLevels;
        [SerializeField] private Transform _levelButtonContainer;
        [SerializeField] private LevelButton _levelButtonPrefab;

        public static LevelData SelectedLevel { get; private set; }

        private void Start()
        {
            CreateLevelButtons();
        }

        private void CreateLevelButtons()
        {
            foreach (var level in _allLevels)
            {
                var button = Instantiate(_levelButtonPrefab, _levelButtonContainer);
                button.Initialize(level);
                button.OnSelected += SelectLevel;
            }
        }

        public void SelectLevel(LevelData level)
        {
            SelectedLevel = level;
            SceneManager.LoadScene(level.SceneName);
        }
    }
}
```

### Level Button Component

Create `LevelButton.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefense.Core;

namespace TowerDefense.UI
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _thumbnail;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _difficultyText;
        [SerializeField] private Image[] _stars;
        [SerializeField] private GameObject _lockedOverlay;

        private LevelData _levelData;

        public event System.Action<LevelData> OnSelected;

        public void Initialize(LevelData data)
        {
            _levelData = data;

            _nameText.text = data.LevelName;
            _difficultyText.text = data.Difficulty.ToString();

            if (data.Thumbnail != null)
                _thumbnail.sprite = data.Thumbnail;

            // Check unlock status
            bool isUnlocked = !data.IsLockedByDefault || IsLevelComplete(data.RequiredLevel);
            _lockedOverlay.SetActive(!isUnlocked);
            _button.interactable = isUnlocked;

            // Show earned stars
            int earnedStars = GetEarnedStars(data);
            for (int i = 0; i < _stars.Length; i++)
            {
                _stars[i].enabled = i < earnedStars;
            }

            _button.onClick.AddListener(() => OnSelected?.Invoke(_levelData));
        }

        private bool IsLevelComplete(LevelData level)
        {
            if (level == null) return true;
            return SaveManager.Instance?.IsLevelComplete(level.LevelIndex) ?? false;
        }

        private int GetEarnedStars(LevelData level)
        {
            return SaveManager.Instance?.GetLevelStars(level.LevelIndex) ?? 0;
        }
    }
}
```

### Level Select Scene

Create `LevelSelect.unity` scene:

```
LevelSelect (Scene)
|-- Canvas
|   |-- Header ("Select Level")
|   |-- LevelGrid (Grid Layout Group)
|   |   +-- LevelButton instances
|   +-- BackButton (returns to main menu)
+-- EventSystem
```

### Create Sample Levels

**LD_Level01.asset:**
```
LevelName: "Forest Path"
Description: "A simple path through the forest."
SceneName: "Level01"
Difficulty: Easy
IsLockedByDefault: false
```

**LD_Level02.asset:**
```
LevelName: "Desert Canyon"
Description: "Navigate the winding canyon."
SceneName: "Level02"
Difficulty: Medium
IsLockedByDefault: true
RequiredLevel: LD_Level01
```

**LD_Level03.asset:**
```
LevelName: "Frozen Tundra"
Description: "Survive the harsh cold."
SceneName: "Level03"
Difficulty: Hard
IsLockedByDefault: true
RequiredLevel: LD_Level02
```

## Testing and Acceptance Criteria

### Done When

- [ ] LevelData ScriptableObject created
- [ ] Level select scene created
- [ ] Level buttons display level info
- [ ] Locked levels show locked overlay
- [ ] Completed levels show stars
- [ ] Clicking level loads correct scene
- [ ] At least 3 level data assets created
- [ ] Level unlocking based on completion

## Dependencies

- Issue 24: Game state screens
