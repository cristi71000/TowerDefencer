using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// World-space health bar that floats above enemies.
    /// Provides smooth animation, color gradient, and billboard behavior.
    /// </summary>
    public class EnemyHealthBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _fillImage;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Settings")]
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private bool _hideWhenFull = true;
        [SerializeField] private float _fadeSpeed = 3f;

        [Header("Colors")]
        [SerializeField] private Color _fullHealthColor = Color.green;
        [SerializeField] private Color _halfHealthColor = Color.yellow;
        [SerializeField] private Color _lowHealthColor = Color.red;

        private Transform _cameraTransform;
        private float _targetFillAmount = 1f;
        private float _targetAlpha = 0f;

        private void Awake()
        {
            // Cache camera transform for billboard behavior
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
            else
            {
                Debug.LogWarning("[EnemyHealthBar] Main camera not found. Billboard behavior disabled.");
            }
        }

        private void Start()
        {
            // Ensure initial state
            if (_hideWhenFull)
            {
                _targetAlpha = 0f;
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0f;
                }
            }
        }

        private void LateUpdate()
        {
            // Billboard: face the camera
            if (_cameraTransform != null)
            {
                transform.rotation = Quaternion.LookRotation(transform.position - _cameraTransform.position);
            }

            // Smooth fill animation
            if (_fillImage != null)
            {
                _fillImage.fillAmount = Mathf.Lerp(_fillImage.fillAmount, _targetFillAmount, Time.deltaTime * _smoothSpeed);

                // Update color based on current fill
                UpdateHealthColor(_fillImage.fillAmount);
            }

            // Smooth alpha fade
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, _targetAlpha, Time.deltaTime * _fadeSpeed);
            }
        }

        /// <summary>
        /// Sets the health bar to the specified normalized value (0-1) with smooth animation.
        /// </summary>
        /// <param name="normalizedHealth">Health percentage from 0 to 1.</param>
        public void SetHealth(float normalizedHealth)
        {
            _targetFillAmount = Mathf.Clamp01(normalizedHealth);

            // Show health bar when damaged
            if (_hideWhenFull)
            {
                _targetAlpha = normalizedHealth < 1f ? 1f : 0f;
            }
            else
            {
                _targetAlpha = 1f;
            }
        }

        /// <summary>
        /// Sets the health bar immediately without animation.
        /// </summary>
        /// <param name="normalizedHealth">Health percentage from 0 to 1.</param>
        public void SetHealthImmediate(float normalizedHealth)
        {
            _targetFillAmount = Mathf.Clamp01(normalizedHealth);

            if (_fillImage != null)
            {
                _fillImage.fillAmount = _targetFillAmount;
                UpdateHealthColor(_targetFillAmount);
            }

            // Update alpha immediately
            if (_hideWhenFull)
            {
                _targetAlpha = normalizedHealth < 1f ? 1f : 0f;
            }
            else
            {
                _targetAlpha = 1f;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = _targetAlpha;
            }
        }

        /// <summary>
        /// Resets the health bar for pool reuse.
        /// </summary>
        public void Reset()
        {
            _targetFillAmount = 1f;
            _targetAlpha = 0f;

            if (_fillImage != null)
            {
                _fillImage.fillAmount = 1f;
                UpdateHealthColor(1f);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// Updates the fill image color based on health percentage.
        /// Green (full) -> Yellow (half) -> Red (low)
        /// </summary>
        private void UpdateHealthColor(float normalizedHealth)
        {
            if (_fillImage == null) return;

            Color healthColor;

            if (normalizedHealth > 0.5f)
            {
                // Lerp from green to yellow (1.0 to 0.5)
                float t = (normalizedHealth - 0.5f) * 2f; // Remap 0.5-1.0 to 0-1
                healthColor = Color.Lerp(_halfHealthColor, _fullHealthColor, t);
            }
            else
            {
                // Lerp from yellow to red (0.5 to 0.0)
                float t = normalizedHealth * 2f; // Remap 0-0.5 to 0-1
                healthColor = Color.Lerp(_lowHealthColor, _halfHealthColor, t);
            }

            _fillImage.color = healthColor;
        }

        private void OnValidate()
        {
            // Re-cache camera if needed in editor
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }
    }
}
