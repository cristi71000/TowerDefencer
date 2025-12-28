## Context

The settings menu allows players to adjust audio volumes, graphics quality, and other preferences. This completes the meta systems milestone.

**Builds upon:** Issues 45, 49 (Audio, Save System)

## Detailed Implementation Instructions

### Settings Manager

Create `SettingsManager.cs`:

```csharp
using UnityEngine;
using TowerDefense.Audio;

namespace TowerDefense.Core
{
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        [Header("Default Values")]
        [SerializeField] private float _defaultMasterVolume = 1f;
        [SerializeField] private float _defaultMusicVolume = 0.5f;
        [SerializeField] private float _defaultSFXVolume = 1f;
        [SerializeField] private int _defaultQualityLevel = 2;
        [SerializeField] private bool _defaultFullscreen = true;

        public float MasterVolume { get; private set; }
        public float MusicVolume { get; private set; }
        public float SFXVolume { get; private set; }
        public int QualityLevel { get; private set; }
        public bool IsFullscreen { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadSettings();
            ApplySettings();
        }

        public void LoadSettings()
        {
            if (SaveManager.Instance?.CurrentSave != null)
            {
                var save = SaveManager.Instance.CurrentSave;
                MasterVolume = save.MasterVolume;
                MusicVolume = save.MusicVolume;
                SFXVolume = save.SFXVolume;
            }
            else
            {
                MasterVolume = _defaultMasterVolume;
                MusicVolume = _defaultMusicVolume;
                SFXVolume = _defaultSFXVolume;
            }

            QualityLevel = PlayerPrefs.GetInt("QualityLevel", _defaultQualityLevel);
            IsFullscreen = PlayerPrefs.GetInt("Fullscreen", _defaultFullscreen ? 1 : 0) == 1;
        }

        public void ApplySettings()
        {
            // Audio
            AudioManager.Instance?.SetMasterVolume(MasterVolume);
            AudioManager.Instance?.SetMusicVolume(MusicVolume);
            AudioManager.Instance?.SetSFXVolume(SFXVolume);

            // Graphics
            QualitySettings.SetQualityLevel(QualityLevel);
            Screen.fullScreen = IsFullscreen;
        }

        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);
            AudioManager.Instance?.SetMasterVolume(MasterVolume);
            SaveSettings();
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            AudioManager.Instance?.SetMusicVolume(MusicVolume);
            SaveSettings();
        }

        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
            AudioManager.Instance?.SetSFXVolume(SFXVolume);
            SaveSettings();
        }

        public void SetQualityLevel(int level)
        {
            QualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(QualityLevel);
            PlayerPrefs.SetInt("QualityLevel", QualityLevel);
            PlayerPrefs.Save();
        }

        public void SetFullscreen(bool fullscreen)
        {
            IsFullscreen = fullscreen;
            Screen.fullScreen = fullscreen;
            PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void ResetToDefaults()
        {
            MasterVolume = _defaultMasterVolume;
            MusicVolume = _defaultMusicVolume;
            SFXVolume = _defaultSFXVolume;
            QualityLevel = _defaultQualityLevel;
            IsFullscreen = _defaultFullscreen;

            ApplySettings();
            SaveSettings();
        }

        private void SaveSettings()
        {
            SaveManager.Instance?.SetVolumes(MasterVolume, MusicVolume, SFXVolume);
        }
    }
}
```

### Settings UI Panel

Create `SettingsPanel.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerDefense.UI
{
    public class SettingsPanel : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI _masterValueText;
        [SerializeField] private TextMeshProUGUI _musicValueText;
        [SerializeField] private TextMeshProUGUI _sfxValueText;

        [Header("Graphics")]
        [SerializeField] private TMP_Dropdown _qualityDropdown;
        [SerializeField] private Toggle _fullscreenToggle;

        [Header("Buttons")]
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _backButton;

        private void Start()
        {
            InitializeUI();
            LoadCurrentSettings();

            _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            _sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            _qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            _fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            _resetButton.onClick.AddListener(OnResetClicked);
        }

        private void InitializeUI()
        {
            // Populate quality dropdown
            _qualityDropdown.ClearOptions();
            _qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
        }

        private void LoadCurrentSettings()
        {
            var settings = SettingsManager.Instance;
            if (settings == null) return;

            _masterVolumeSlider.value = settings.MasterVolume;
            _musicVolumeSlider.value = settings.MusicVolume;
            _sfxVolumeSlider.value = settings.SFXVolume;
            _qualityDropdown.value = settings.QualityLevel;
            _fullscreenToggle.isOn = settings.IsFullscreen;

            UpdateValueTexts();
        }

        private void UpdateValueTexts()
        {
            _masterValueText.text = $"{Mathf.RoundToInt(_masterVolumeSlider.value * 100)}%";
            _musicValueText.text = $"{Mathf.RoundToInt(_musicVolumeSlider.value * 100)}%";
            _sfxValueText.text = $"{Mathf.RoundToInt(_sfxVolumeSlider.value * 100)}%";
        }

        private void OnMasterVolumeChanged(float value)
        {
            SettingsManager.Instance?.SetMasterVolume(value);
            UpdateValueTexts();
        }

        private void OnMusicVolumeChanged(float value)
        {
            SettingsManager.Instance?.SetMusicVolume(value);
            UpdateValueTexts();
        }

        private void OnSFXVolumeChanged(float value)
        {
            SettingsManager.Instance?.SetSFXVolume(value);
            UpdateValueTexts();
        }

        private void OnQualityChanged(int index)
        {
            SettingsManager.Instance?.SetQualityLevel(index);
        }

        private void OnFullscreenChanged(bool isOn)
        {
            SettingsManager.Instance?.SetFullscreen(isOn);
        }

        private void OnResetClicked()
        {
            SettingsManager.Instance?.ResetToDefaults();
            LoadCurrentSettings();
        }
    }
}
```

### Settings Panel Layout

```
SettingsPanel
|-- Header ("Settings")
|-- AudioSection
|   |-- MasterVolume
|   |   |-- Label ("Master Volume")
|   |   |-- Slider
|   |   +-- ValueText ("100%")
|   |-- MusicVolume
|   |   |-- Label ("Music")
|   |   |-- Slider
|   |   +-- ValueText
|   +-- SFXVolume
|       |-- Label ("Sound Effects")
|       |-- Slider
|       +-- ValueText
|-- GraphicsSection
|   |-- QualityDropdown
|   +-- FullscreenToggle
|-- ButtonSection
    |-- ResetButton
    +-- BackButton
```

### Main Menu Integration

Add Settings button to main menu and pause menu.

## Testing and Acceptance Criteria

### Done When

- [ ] Settings panel UI created
- [ ] Volume sliders adjust audio in real-time
- [ ] Quality dropdown changes quality settings
- [ ] Fullscreen toggle works
- [ ] Settings persist between sessions
- [ ] Reset to defaults works
- [ ] Settings accessible from main menu
- [ ] Settings accessible from pause menu

## Dependencies

- Issue 45: Audio System
- Issue 49: Save System

## Final Notes

This completes the core game loop! After Issue 50, you have:
- Playable tower defense with multiple towers and enemies
- 10-wave level with boss
- Upgrade system
- Multiple maps (level select)
- Persistent save data
- Audio and VFX polish
- Settings menu

Consider future enhancements:
- More levels
- Endless mode
- Achievements
- Leaderboards
- Mobile touch controls
