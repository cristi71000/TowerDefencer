using System.Collections;
using UnityEngine;
using TMPro;
using TowerDefense.Core;
using TowerDefense.Economy;

namespace TowerDefense.UI
{
    /// <summary>
    /// Manages the main gameplay HUD displaying currency, lives, and wave information.
    /// Subscribes to GameManager and EconomyManager events to update displays.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        public static HUDManager Instance { get; private set; }

        [Header("Currency Display")]
        [SerializeField] private TextMeshProUGUI _currencyText;
        [SerializeField] private string _currencyFormat = "{0}";

        [Header("Lives Display")]
        [SerializeField] private TextMeshProUGUI _livesText;
        [SerializeField] private string _livesFormat = "{0}";

        [Header("Wave Display")]
        [SerializeField] private TextMeshProUGUI _waveText;
        [SerializeField] private string _waveFormat = "Wave {0}";

        [Header("Animation Settings")]
        [SerializeField] private float _punchScale = 1.2f;
        [SerializeField] private float _punchDuration = 0.2f;

        [Header("Colors")]
        [SerializeField] private Color _currencyGainColor = Color.green;
        [SerializeField] private Color _currencyLossColor = Color.red;
        [SerializeField] private Color _livesLossColor = Color.red;

        private Color _currencyOriginalColor;
        private Color _livesOriginalColor;
        private Coroutine _currencyAnimationCoroutine;
        private Coroutine _livesAnimationCoroutine;
        private int _lastKnownLives;
        private int _lastKnownCurrency;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Cache original colors
            if (_currencyText != null)
            {
                _currencyOriginalColor = _currencyText.color;
            }
            if (_livesText != null)
            {
                _livesOriginalColor = _livesText.color;
            }
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();

            // Stop any running animation coroutines
            if (_currencyAnimationCoroutine != null)
            {
                StopCoroutine(_currencyAnimationCoroutine);
                _currencyAnimationCoroutine = null;
            }
            if (_livesAnimationCoroutine != null)
            {
                StopCoroutine(_livesAnimationCoroutine);
                _livesAnimationCoroutine = null;
            }

            // Reset visual states
            if (_currencyText != null)
            {
                _currencyText.transform.localScale = Vector3.one;
                _currencyText.color = _currencyOriginalColor;
            }
            if (_livesText != null)
            {
                _livesText.transform.localScale = Vector3.one;
                _livesText.color = _livesOriginalColor;
            }
        }

        private void Start()
        {
            InitializeDisplays();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void SubscribeToEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLivesChanged += HandleLivesChanged;
                GameManager.Instance.OnWaveChanged += HandleWaveChanged;
            }

            // Subscribe to EconomyManager for currency (has delta info)
            // Only fall back to GameManager if EconomyManager is not available
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnCurrencyChanged += HandleEconomyCurrencyChanged;
            }
            else if (GameManager.Instance != null)
            {
                // Fallback: use GameManager currency events when EconomyManager unavailable
                GameManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnLivesChanged -= HandleLivesChanged;
                GameManager.Instance.OnWaveChanged -= HandleWaveChanged;
                // Also unsubscribe from currency in case we used fallback mode
                GameManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            }

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnCurrencyChanged -= HandleEconomyCurrencyChanged;
            }
        }

        private void InitializeDisplays()
        {
            if (GameManager.Instance != null)
            {
                _lastKnownLives = GameManager.Instance.CurrentLives;
                _lastKnownCurrency = GameManager.Instance.CurrentCurrency;
                UpdateLivesDisplay(GameManager.Instance.CurrentLives);
                UpdateCurrencyDisplay(GameManager.Instance.CurrentCurrency);
                UpdateWaveDisplay(GameManager.Instance.CurrentWave);
            }
            else
            {
                // Set default values if GameManager is not available
                _lastKnownLives = 0;
                _lastKnownCurrency = 0;
                UpdateLivesDisplay(0);
                UpdateCurrencyDisplay(0);
                UpdateWaveDisplay(0);
            }
        }

        private void HandleLivesChanged(int lives)
        {
            UpdateLivesDisplay(lives);

            // Animate if lives decreased
            if (lives < _lastKnownLives)
            {
                AnimateLivesDamage();
            }

            _lastKnownLives = lives;
        }

        private void HandleCurrencyChanged(int amount)
        {
            int delta = amount - _lastKnownCurrency;
            UpdateCurrencyDisplay(amount);

            if (delta != 0)
            {
                AnimateCurrencyChange(delta > 0);
            }

            _lastKnownCurrency = amount;
        }

        private void HandleEconomyCurrencyChanged(int newAmount, int delta)
        {
            UpdateCurrencyDisplay(newAmount);

            if (delta != 0)
            {
                AnimateCurrencyChange(delta > 0);
            }

            // Keep _lastKnownCurrency in sync to prevent duplicate animations
            _lastKnownCurrency = newAmount;
        }

        private void HandleWaveChanged(int wave)
        {
            UpdateWaveDisplay(wave);
        }

        private void UpdateCurrencyDisplay(int amount)
        {
            if (_currencyText != null)
            {
                _currencyText.text = string.Format(_currencyFormat, amount);
            }
        }

        private void UpdateLivesDisplay(int lives)
        {
            if (_livesText != null)
            {
                _livesText.text = string.Format(_livesFormat, lives);
            }
        }

        private void UpdateWaveDisplay(int wave)
        {
            if (_waveText != null)
            {
                _waveText.text = string.Format(_waveFormat, wave);
            }
        }

        private void AnimateCurrencyChange(bool isGain)
        {
            if (_currencyText == null) return;

            if (_currencyAnimationCoroutine != null)
            {
                StopCoroutine(_currencyAnimationCoroutine);
                // Reset to original state
                _currencyText.transform.localScale = Vector3.one;
                _currencyText.color = _currencyOriginalColor;
            }

            Color flashColor = isGain ? _currencyGainColor : _currencyLossColor;
            _currencyAnimationCoroutine = StartCoroutine(PunchScaleCoroutine(
                _currencyText.transform,
                _currencyText,
                flashColor,
                _currencyOriginalColor
            ));
        }

        private void AnimateLivesDamage()
        {
            if (_livesText == null) return;

            if (_livesAnimationCoroutine != null)
            {
                StopCoroutine(_livesAnimationCoroutine);
                // Reset to original state
                _livesText.transform.localScale = Vector3.one;
                _livesText.color = _livesOriginalColor;
            }

            _livesAnimationCoroutine = StartCoroutine(PunchScaleCoroutine(
                _livesText.transform,
                _livesText,
                _livesLossColor,
                _livesOriginalColor
            ));
        }

        private IEnumerator PunchScaleCoroutine(
            Transform element,
            TextMeshProUGUI text,
            Color flashColor,
            Color originalColor)
        {
            float elapsed = 0f;

            while (elapsed < _punchDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _punchDuration;

                // Punch scale: grow to max at 50%, shrink back at 100%
                float scale;
                if (t < 0.5f)
                {
                    scale = Mathf.Lerp(1f, _punchScale, t * 2f);
                }
                else
                {
                    scale = Mathf.Lerp(_punchScale, 1f, (t - 0.5f) * 2f);
                }

                element.localScale = Vector3.one * scale;

                // Color flash: start with flash color, lerp back to original
                if (text != null)
                {
                    text.color = Color.Lerp(flashColor, originalColor, t);
                }

                yield return null;
            }

            // Ensure final state is correct
            element.localScale = Vector3.one;
            if (text != null)
            {
                text.color = originalColor;
            }
        }

        /// <summary>
        /// Displays a temporary message on the HUD.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="duration">How long to show the message.</param>
        public void ShowMessage(string message, float duration = 2f)
        {
            // Log message for now - can be extended to show UI popup
            UnityEngine.Debug.Log($"[HUD Message] {message}");
        }

        /// <summary>
        /// Forces a refresh of all HUD displays from current game state.
        /// </summary>
        public void RefreshAllDisplays()
        {
            InitializeDisplays();
        }
    }
}
