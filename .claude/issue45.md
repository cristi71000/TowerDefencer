## Context

Audio makes the game feel complete. This issue implements the audio system with sound effects for all actions and background music.

**Builds upon:** All gameplay issues

## Detailed Implementation Instructions

### Audio Manager

Create `AudioManager.cs`:

```csharp
using UnityEngine;
using UnityEngine.Audio;

namespace TowerDefense.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;
        [SerializeField] private AudioSource _uiSource;

        [Header("Music")]
        [SerializeField] private AudioClip _menuMusic;
        [SerializeField] private AudioClip _gameplayMusic;
        [SerializeField] private AudioClip _victoryMusic;
        [SerializeField] private AudioClip _defeatMusic;

        [Header("SFX - Towers")]
        [SerializeField] private AudioClip _towerPlace;
        [SerializeField] private AudioClip _towerSell;
        [SerializeField] private AudioClip _towerUpgrade;
        [SerializeField] private AudioClip _towerShootBasic;
        [SerializeField] private AudioClip _towerShootCannon;
        [SerializeField] private AudioClip _towerShootSniper;

        [Header("SFX - Enemies")]
        [SerializeField] private AudioClip _enemyHit;
        [SerializeField] private AudioClip _enemyDeath;
        [SerializeField] private AudioClip _bossSpawn;
        [SerializeField] private AudioClip _enemyReachEnd;

        [Header("SFX - UI")]
        [SerializeField] private AudioClip _buttonClick;
        [SerializeField] private AudioClip _buttonHover;
        [SerializeField] private AudioClip _waveStart;
        [SerializeField] private AudioClip _waveComplete;
        [SerializeField] private AudioClip _insufficientFunds;

        [Header("Settings")]
        [SerializeField] private float _masterVolume = 1f;
        [SerializeField] private float _musicVolume = 0.5f;
        [SerializeField] private float _sfxVolume = 1f;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayMusic(MusicType type)
        {
            AudioClip clip = type switch
            {
                MusicType.Menu => _menuMusic,
                MusicType.Gameplay => _gameplayMusic,
                MusicType.Victory => _victoryMusic,
                MusicType.Defeat => _defeatMusic,
                _ => null
            };

            if (clip != null && _musicSource.clip != clip)
            {
                _musicSource.clip = clip;
                _musicSource.loop = true;
                _musicSource.Play();
            }
        }

        public void PlaySFX(SFXType type)
        {
            AudioClip clip = GetSFXClip(type);
            if (clip != null)
            {
                _sfxSource.PlayOneShot(clip);
            }
        }

        public void PlaySFXAtPosition(SFXType type, Vector3 position)
        {
            AudioClip clip = GetSFXClip(type);
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, _sfxVolume * _masterVolume);
            }
        }

        public void PlayUI(UISFXType type)
        {
            AudioClip clip = type switch
            {
                UISFXType.ButtonClick => _buttonClick,
                UISFXType.ButtonHover => _buttonHover,
                UISFXType.InsufficientFunds => _insufficientFunds,
                _ => null
            };

            if (clip != null)
            {
                _uiSource.PlayOneShot(clip);
            }
        }

        private AudioClip GetSFXClip(SFXType type)
        {
            return type switch
            {
                SFXType.TowerPlace => _towerPlace,
                SFXType.TowerSell => _towerSell,
                SFXType.TowerUpgrade => _towerUpgrade,
                SFXType.TowerShootBasic => _towerShootBasic,
                SFXType.TowerShootCannon => _towerShootCannon,
                SFXType.TowerShootSniper => _towerShootSniper,
                SFXType.EnemyHit => _enemyHit,
                SFXType.EnemyDeath => _enemyDeath,
                SFXType.BossSpawn => _bossSpawn,
                SFXType.EnemyReachEnd => _enemyReachEnd,
                SFXType.WaveStart => _waveStart,
                SFXType.WaveComplete => _waveComplete,
                _ => null
            };
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            _musicSource.volume = _musicVolume * _masterVolume;
            _sfxSource.volume = _sfxVolume * _masterVolume;
            _uiSource.volume = _sfxVolume * _masterVolume;
        }
    }

    public enum MusicType { Menu, Gameplay, Victory, Defeat }
    public enum SFXType
    {
        TowerPlace, TowerSell, TowerUpgrade,
        TowerShootBasic, TowerShootCannon, TowerShootSniper,
        EnemyHit, EnemyDeath, BossSpawn, EnemyReachEnd,
        WaveStart, WaveComplete
    }
    public enum UISFXType { ButtonClick, ButtonHover, InsufficientFunds }
}
```

### Free Audio Resources

Use free audio from:
- **Kenney.nl** - Free sound effects
- **OpenGameArt.org** - Free game audio
- **Freesound.org** - CC0 sounds

### Integration Points

Add audio calls throughout the game:

```csharp
// Tower placement
AudioManager.Instance?.PlaySFX(SFXType.TowerPlace);

// Tower attack
AudioManager.Instance?.PlaySFXAtPosition(SFXType.TowerShootBasic, transform.position);

// Enemy death
AudioManager.Instance?.PlaySFXAtPosition(SFXType.EnemyDeath, transform.position);

// Wave start
AudioManager.Instance?.PlaySFX(SFXType.WaveStart);
```

### UI Button Sounds

Add to all buttons:

```csharp
using TowerDefense.Audio;

public class ButtonSounds : MonoBehaviour
{
    public void OnClick()
    {
        AudioManager.Instance?.PlayUI(UISFXType.ButtonClick);
    }

    public void OnHover()
    {
        AudioManager.Instance?.PlayUI(UISFXType.ButtonHover);
    }
}
```

## Testing and Acceptance Criteria

### Done When

- [ ] AudioManager handles all audio
- [ ] Music plays during gameplay
- [ ] Music changes on victory/defeat
- [ ] Tower attacks have sounds
- [ ] Enemy deaths have sounds
- [ ] UI buttons have click sounds
- [ ] Wave start/complete sounds play
- [ ] Volume settings work
- [ ] Audio does not overlap harshly

## Dependencies

- All gameplay issues
