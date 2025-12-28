## Context

Players need screens for pausing, winning, and losing. This issue implements the game state screens that appear during these conditions with appropriate options (resume, restart, main menu).

**Builds upon:** Issues 2, 21 (Game Manager, HUD)

## Detailed Implementation Instructions

### Game State Screen Manager

Create `GameStateScreenManager.cs` in `_Project/Scripts/Runtime/UI/`:

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using TowerDefense.Core;

namespace TowerDefense.UI
{
    public class GameStateScreenManager : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] private GameObject _pauseScreen;
        [SerializeField] private GameObject _victoryScreen;
        [SerializeField] private GameObject _defeatScreen;

        [Header("Input")]
        [SerializeField] private KeyCode _pauseKey = KeyCode.Escape;

        private void Start()
        {
            HideAllScreens();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(_pauseKey))
            {
                TogglePause();
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            HideAllScreens();

            switch (state)
            {
                case GameState.Paused:
                    ShowScreen(_pauseScreen);
                    break;
                case GameState.Victory:
                    ShowScreen(_victoryScreen);
                    break;
                case GameState.Defeat:
                    ShowScreen(_defeatScreen);
                    break;
            }
        }

        private void HideAllScreens()
        {
            _pauseScreen?.SetActive(false);
            _victoryScreen?.SetActive(false);
            _defeatScreen?.SetActive(false);
        }

        private void ShowScreen(GameObject screen)
        {
            if (screen != null)
            {
                screen.SetActive(true);
            }
        }

        public void TogglePause()
        {
            if (GameManager.Instance == null) return;

            var currentState = GameManager.Instance.CurrentState;

            if (currentState == GameState.Victory || currentState == GameState.Defeat)
                return;

            if (currentState == GameState.Paused)
            {
                GameManager.Instance.ResumeGame();
            }
            else
            {
                GameManager.Instance.PauseGame();
            }
        }

        // Button callbacks
        public void OnResumeClicked()
        {
            GameManager.Instance?.ResumeGame();
        }

        public void OnRestartClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu"); // Or index 0
        }

        public void OnQuitClicked()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
```

### Screen Prefabs

Create UI panels for each state:

**Pause Screen:**
```
PauseScreen (Panel)
|-- Background (Image, semi-transparent black)
|-- Panel (centered)
    |-- Title ("PAUSED")
    |-- ResumeButton
    |-- RestartButton
    |-- MainMenuButton
    +-- QuitButton
```

**Victory Screen:**
```
VictoryScreen (Panel)
|-- Background (Image, semi-transparent)
|-- Panel (centered)
    |-- Title ("VICTORY!")
    |-- StatsPanel
    |   |-- WavesSurvived
    |   |-- EnemiesKilled
    |   +-- CurrencyEarned
    |-- NextLevelButton
    |-- RestartButton
    +-- MainMenuButton
```

**Defeat Screen:**
```
DefeatScreen (Panel)
|-- Background (Image, semi-transparent red tint)
|-- Panel (centered)
    |-- Title ("DEFEAT")
    |-- StatsPanel
    |   |-- WaveReached
    |   +-- EnemiesKilled
    |-- RestartButton
    +-- MainMenuButton
```

### Button Styling

Use consistent button style:
- Background: Rounded rectangle
- Normal: Dark gray
- Hover: Lighter gray
- Pressed: Darkest gray
- Text: White, size 24

### Screen Transitions (Optional)

Add fade animation:

```csharp
public class ScreenFade : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 0.3f;

    public void FadeIn()
    {
        StartCoroutine(Fade(0f, 1f));
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(1f, 0f));
    }

    private System.Collections.IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / _fadeDuration);
            yield return null;
        }
        _canvasGroup.alpha = to;
    }
}
```

### Scene Setup

1. Add screens to HUD Canvas (initially inactive)
2. Add GameStateScreenManager to UI management
3. Wire up button OnClick events

## Testing and Acceptance Criteria

### Done When

- [ ] Escape toggles pause screen
- [ ] Pause freezes gameplay (Time.timeScale = 0)
- [ ] Resume continues gameplay
- [ ] Restart reloads scene
- [ ] Victory screen shows on win
- [ ] Defeat screen shows on loss
- [ ] Buttons navigate correctly
- [ ] Screens block input to gameplay

## Dependencies

- Issue 2: Game Manager states
- Issue 21: HUD Canvas
