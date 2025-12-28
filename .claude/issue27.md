## Context

Players need to see wave information: current wave number, enemies remaining, countdown to next wave, and a button to start waves manually. This issue implements the wave-related UI elements.

**Builds upon:** Issues 21, 26 (HUD, WaveManager)

## Detailed Implementation Instructions

### Wave HUD Component

Create `WaveHUD.cs` in `_Project/Scripts/Runtime/UI/`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefense.Waves;
using TowerDefense.Enemies;

namespace TowerDefense.UI
{
    public class WaveHUD : MonoBehaviour
    {
        [Header("Wave Display")]
        [SerializeField] private TextMeshProUGUI _waveText;
        [SerializeField] private TextMeshProUGUI _waveNameText;
        [SerializeField] private string _waveFormat = "Wave {0}/{1}";

        [Header("Enemy Counter")]
        [SerializeField] private TextMeshProUGUI _enemyCountText;
        [SerializeField] private string _enemyFormat = "Enemies: {0}";

        [Header("Next Wave")]
        [SerializeField] private GameObject _nextWavePanel;
        [SerializeField] private Button _startWaveButton;
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private string _countdownFormat = "Next wave in {0}s";

        [Header("Animation")]
        [SerializeField] private Animator _waveAnimator;

        private float _countdownTimer;
        private bool _showingCountdown;

        private void Start()
        {
            if (_startWaveButton != null)
                _startWaveButton.onClick.AddListener(OnStartWaveClicked);

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStarted += HandleWaveStarted;
                WaveManager.Instance.OnWaveCompleted += HandleWaveCompleted;
            }

            UpdateDisplay();
            ShowNextWavePanel(true);
        }

        private void OnDestroy()
        {
            if (_startWaveButton != null)
                _startWaveButton.onClick.RemoveListener(OnStartWaveClicked);

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnWaveStarted -= HandleWaveStarted;
                WaveManager.Instance.OnWaveCompleted -= HandleWaveCompleted;
            }
        }

        private void Update()
        {
            UpdateEnemyCount();

            if (_showingCountdown && _countdownTimer > 0)
            {
                _countdownTimer -= Time.deltaTime;
                UpdateCountdownDisplay();

                if (_countdownTimer <= 0)
                {
                    _showingCountdown = false;
                }
            }
        }

        private void HandleWaveStarted(int waveNumber)
        {
            UpdateDisplay();
            ShowNextWavePanel(false);

            if (_waveAnimator != null)
                _waveAnimator.SetTrigger("WaveStart");
        }

        private void HandleWaveCompleted(int waveNumber)
        {
            if (WaveManager.Instance.AllWavesCompleted)
            {
                ShowNextWavePanel(false);
            }
            else
            {
                ShowNextWavePanel(true);
                StartCountdown(10f); // Match level config time
            }
        }

        private void UpdateDisplay()
        {
            if (_waveText != null && WaveManager.Instance != null)
            {
                _waveText.text = string.Format(_waveFormat,
                    WaveManager.Instance.CurrentWaveNumber,
                    WaveManager.Instance.TotalWaves);
            }

            if (_waveNameText != null && WaveManager.Instance?.CurrentWave != null)
            {
                _waveNameText.text = WaveManager.Instance.CurrentWave.WaveName;
            }
        }

        private void UpdateEnemyCount()
        {
            if (_enemyCountText != null && EnemySpawner.Instance != null)
            {
                _enemyCountText.text = string.Format(_enemyFormat,
                    EnemySpawner.Instance.ActiveEnemyCount);
            }
        }

        private void ShowNextWavePanel(bool show)
        {
            if (_nextWavePanel != null)
                _nextWavePanel.SetActive(show);
        }

        private void StartCountdown(float seconds)
        {
            _countdownTimer = seconds;
            _showingCountdown = true;
            UpdateCountdownDisplay();
        }

        private void UpdateCountdownDisplay()
        {
            if (_countdownText != null)
            {
                _countdownText.text = string.Format(_countdownFormat,
                    Mathf.CeilToInt(_countdownTimer));
            }
        }

        private void OnStartWaveClicked()
        {
            WaveManager.Instance?.StartNextWave();
            _showingCountdown = false;
        }
    }
}
```

### Wave Announcement

Create `WaveAnnouncement.cs`:

```csharp
using UnityEngine;
using TMPro;

namespace TowerDefense.UI
{
    public class WaveAnnouncement : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _announcementText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _displayDuration = 2f;
        [SerializeField] private float _fadeDuration = 0.5f;

        public void ShowAnnouncement(string text)
        {
            _announcementText.text = text;
            StartCoroutine(AnimateAnnouncement());
        }

        private System.Collections.IEnumerator AnimateAnnouncement()
        {
            // Fade in
            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = elapsed / _fadeDuration;
                yield return null;
            }
            _canvasGroup.alpha = 1f;

            // Hold
            yield return new WaitForSeconds(_displayDuration);

            // Fade out
            elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = 1f - (elapsed / _fadeDuration);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
        }
    }
}
```

### UI Layout

Add to HUD:

```
HUDCanvas
|-- TopPanel
|   |-- WaveDisplay
|   |   |-- WaveText ("Wave 1/10")
|   |   +-- WaveNameText ("Tutorial")
|   +-- EnemyCounter
|       +-- EnemyCountText ("Enemies: 5")
|-- CenterAnnouncement (WaveAnnouncement)
|   +-- AnnouncementText
+-- NextWavePanel (bottom or corner)
    |-- CountdownText ("Next wave in 5s")
    +-- StartWaveButton ("Start Wave")
```

## Testing and Acceptance Criteria

### Done When

- [ ] Wave number displays current/total
- [ ] Wave name displays if available
- [ ] Enemy counter updates in real-time
- [ ] Start Wave button begins next wave
- [ ] Countdown shows time to auto-start
- [ ] Wave announcement shows on wave start
- [ ] Panel hides during active wave
- [ ] Panel shows between waves

## Dependencies

- Issue 21: HUD
- Issue 26: WaveManager
