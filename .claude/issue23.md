## Context

The lives system tracks how much damage the player can take before losing. When enemies reach the exit, lives are reduced. At zero lives, the game is lost. This issue ensures the lives system is fully integrated with UI feedback.

**Builds upon:** Issues 2, 11, 21 (Game Manager, Enemy systems, HUD)

## Detailed Implementation Instructions

### Lives Manager

Create `LivesManager.cs` in `_Project/Scripts/Runtime/Core/`:

```csharp
using UnityEngine;
using TowerDefense.Core.Events;

namespace TowerDefense.Core
{
    public class LivesManager : MonoBehaviour
    {
        public static LivesManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int _maxLives = 20;
        [SerializeField] private int _startingLives = 20;

        [Header("Events")]
        [SerializeField] private IntEventChannel _onLivesChanged;
        [SerializeField] private GameEventChannel _onGameOver;

        private int _currentLives;

        public int CurrentLives => _currentLives;
        public int MaxLives => _maxLives;
        public float LivesPercent => (float)_currentLives / _maxLives;
        public bool IsAlive => _currentLives > 0;

        public event System.Action<int> OnLivesChanged;
        public event System.Action OnGameOver;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Initialize()
        {
            _currentLives = _startingLives;
            NotifyLivesChanged();
        }

        public void TakeDamage(int damage)
        {
            if (!IsAlive || damage <= 0) return;

            _currentLives = Mathf.Max(0, _currentLives - damage);
            NotifyLivesChanged();

            if (_currentLives <= 0)
            {
                TriggerGameOver();
            }
        }

        public void AddLives(int amount)
        {
            if (amount <= 0) return;

            _currentLives = Mathf.Min(_maxLives, _currentLives + amount);
            NotifyLivesChanged();
        }

        private void NotifyLivesChanged()
        {
            OnLivesChanged?.Invoke(_currentLives);
            _onLivesChanged?.RaiseEvent(_currentLives);

            // Sync with GameManager
            // GameManager.Instance property should read from here
        }

        private void TriggerGameOver()
        {
            Debug.Log("GAME OVER - Lives depleted!");
            OnGameOver?.Invoke();
            _onGameOver?.RaiseEvent();

            GameManager.Instance?.SetGameState(GameState.Defeat);
        }

        public void Reset()
        {
            Initialize();
        }
    }
}
```

### Lives UI Component

Create `LivesDisplay.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerDefense.UI
{
    public class LivesDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI _livesText;
        [SerializeField] private Image _livesBar;
        [SerializeField] private Image _heartIcon;

        [Header("Animation")]
        [SerializeField] private float _shakeIntensity = 5f;
        [SerializeField] private float _shakeDuration = 0.3f;
        [SerializeField] private Color _damageFlashColor = Color.red;

        private Vector3 _originalPosition;
        private Color _originalHeartColor;

        private void Awake()
        {
            _originalPosition = transform.localPosition;
            if (_heartIcon != null)
                _originalHeartColor = _heartIcon.color;
        }

        private void Start()
        {
            if (LivesManager.Instance != null)
            {
                LivesManager.Instance.OnLivesChanged += UpdateDisplay;
                UpdateDisplay(LivesManager.Instance.CurrentLives);
            }
        }

        private void OnDestroy()
        {
            if (LivesManager.Instance != null)
                LivesManager.Instance.OnLivesChanged -= UpdateDisplay;
        }

        private void UpdateDisplay(int lives)
        {
            if (_livesText != null)
                _livesText.text = lives.ToString();

            if (_livesBar != null && LivesManager.Instance != null)
                _livesBar.fillAmount = LivesManager.Instance.LivesPercent;

            // Play damage animation
            StopAllCoroutines();
            StartCoroutine(DamageAnimation());
        }

        private System.Collections.IEnumerator DamageAnimation()
        {
            float elapsed = 0f;

            while (elapsed < _shakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _shakeDuration;

                // Shake
                Vector2 offset = Random.insideUnitCircle * _shakeIntensity * (1f - t);
                transform.localPosition = _originalPosition + (Vector3)offset;

                // Flash heart
                if (_heartIcon != null)
                    _heartIcon.color = Color.Lerp(_damageFlashColor, _originalHeartColor, t);

                yield return null;
            }

            transform.localPosition = _originalPosition;
            if (_heartIcon != null)
                _heartIcon.color = _originalHeartColor;
        }
    }
}
```

### Update ExitPoint Integration

Update ExitPoint to use LivesManager:

```csharp
// In ExitPoint.cs OnTriggerEnter:
private void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Enemy"))
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsDead)
        {
            int damage = enemy.Data.LivesDamage;
            LivesManager.Instance?.TakeDamage(damage);
        }
    }
}
```

### Scene Setup

1. Create LivesManager GameObject under --- MANAGEMENT ---
2. Configure max and starting lives
3. Add LivesDisplay to HUD with references

## Testing and Acceptance Criteria

### Done When

- [ ] LivesManager tracks current lives
- [ ] TakeDamage reduces lives correctly
- [ ] Lives cannot go below 0
- [ ] UI updates on lives change
- [ ] Shake/flash animation on damage
- [ ] GameOver triggered at 0 lives
- [ ] Lives bar fills proportionally
- [ ] Reset restores starting lives

## Dependencies

- Issue 2: Game Manager
- Issue 11: Enemy reaching exit
- Issue 21: HUD system
