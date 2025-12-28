## Context

The player needs to see their currency, lives, and wave information. This issue implements the main gameplay HUD with essential information displayed clearly.

**Builds upon:** Issues 2, 20 (Game Manager, Economy)

## Detailed Implementation Instructions

### HUD Manager

Create `HUDManager.cs` in `_Project/Scripts/Runtime/UI/`:

```csharp
using UnityEngine;
using TMPro;
using TowerDefense.Core;
using TowerDefense.Economy;

namespace TowerDefense.UI
{
    public class HUDManager : MonoBehaviour
    {
        public static HUDManager Instance { get; private set; }

        [Header("Currency")]
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private string _currencyFormat = "{0}";

        [Header("Lives")]
        [SerializeField] private TextMeshProUGUI _livesText;
        [SerializeField] private string _livesFormat = "{0}";

        [Header("Wave")]
        [SerializeField] private TextMeshProUGUI _waveText;
        [SerializeField] private string _waveFormat = "Wave {0}";

        [Header("Animation")]
        [SerializeField] private float _punchScale = 1.2f;
        [SerializeField] private float _punchDuration = 0.2f;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            // Subscribe to events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLivesChanged += UpdateLives;
                GameManager.Instance.OnCurrencyChanged += UpdateCurrency;
                GameManager.Instance.OnWaveChanged += UpdateWave;

                // Initial update
                UpdateLives(GameManager.Instance.CurrentLives);
                UpdateCurrency(GameManager.Instance.CurrentCurrency);
                UpdateWave(GameManager.Instance.CurrentWave);
            }

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnCurrencyChanged += OnCurrencyChangedWithDelta;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLivesChanged -= UpdateLives;
                GameManager.Instance.OnCurrencyChanged -= UpdateCurrency;
                GameManager.Instance.OnWaveChanged -= UpdateWave;
            }

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnCurrencyChanged -= OnCurrencyChangedWithDelta;
            }
        }

        private void UpdateCurrency(int amount)
        {
            if (_currencyText != null)
                _currencyText.text = string.Format(_currencyFormat, amount);
        }

        private void OnCurrencyChangedWithDelta(int newAmount, int delta)
        {
            UpdateCurrency(newAmount);

            if (delta != 0)
                PunchElement(_currencyText?.transform, delta > 0 ? Color.green : Color.red);
        }

        private void UpdateLives(int amount)
        {
            if (_livesText != null)
                _livesText.text = string.Format(_livesFormat, amount);
        }

        private void UpdateWave(int wave)
        {
            if (_waveText != null)
                _waveText.text = string.Format(_waveFormat, wave);
        }

        private void PunchElement(Transform element, Color flashColor)
        {
            if (element == null) return;
            StartCoroutine(PunchScaleCoroutine(element, flashColor));
        }

        private System.Collections.IEnumerator PunchScaleCoroutine(Transform element, Color flashColor)
        {
            var text = element.GetComponent<TextMeshProUGUI>();
            Color originalColor = text != null ? text.color : Color.white;

            float elapsed = 0f;
            while (elapsed < _punchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _punchDuration;

                float scale = t < 0.5f
                    ? Mathf.Lerp(1f, _punchScale, t * 2f)
                    : Mathf.Lerp(_punchScale, 1f, (t - 0.5f) * 2f);

                element.localScale = Vector3.one * scale;

                if (text != null)
                    text.color = Color.Lerp(flashColor, originalColor, t);

                yield return null;
            }

            element.localScale = Vector3.one;
            if (text != null)
                text.color = originalColor;
        }

        public void ShowMessage(string message, float duration = 2f)
        {
            // Implement message popup if needed
            Debug.Log($"HUD Message: {message}");
        }
    }
}
```

### HUD Canvas Setup

Create HUD hierarchy:

```
--- UI ---
+-- HUDCanvas (Canvas, CanvasScaler, GraphicRaycaster)
    |-- TopPanel (Horizontal Layout)
    |   |-- CurrencyDisplay
    |   |   |-- CurrencyIcon (Image)
    |   |   +-- CurrencyText (TMP)
    |   |-- LivesDisplay
    |   |   |-- LivesIcon (Image)
    |   |   +-- LivesText (TMP)
    |   +-- WaveDisplay
    |       +-- WaveText (TMP)
    +-- BottomPanel (for tower buttons, see Issue 22)
```

### Canvas Configuration

1. Create UI > Canvas
2. Set Canvas:
   - Render Mode: Screen Space - Overlay
   - Sort Order: 0
3. Add CanvasScaler:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920 x 1080
   - Match Width Or Height: 0.5

### Text Styling

Use TextMeshPro with:
- Font: Default TMP font or custom
- Currency: Size 36, Bold, Color white
- Lives: Size 36, Bold, Color red tint
- Wave: Size 28, Regular, Color white

### Icons (Placeholder)

Create simple icons using Unity UI:
- Currency: Yellow circle or coin sprite
- Lives: Red heart sprite
- Use free assets from Kenney.nl if available

## Testing and Acceptance Criteria

### Done When

- [ ] HUD displays currency amount
- [ ] HUD displays current lives
- [ ] HUD displays current wave
- [ ] Currency updates with animation on change
- [ ] Lives updates when damaged
- [ ] Wave number updates on wave start
- [ ] HUD scales properly at different resolutions
- [ ] HUD is readable and clear

## Dependencies

- Issue 2: Game Manager events
- Issue 20: Economy Manager
