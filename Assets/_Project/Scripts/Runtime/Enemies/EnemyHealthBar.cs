using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// World-space health bar UI that billboards toward the camera.
    /// Provides smooth fill animation with color gradient based on health percentage.
    /// </summary>
    public class EnemyHealthBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _fillImage;

        [Header("Settings")]
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private bool _hideWhenFull = true;
        [SerializeField] private float _fadeSpeed = 3f;

        [Header("Color Gradient")]
        [SerializeField] private Color _fullHealthColor = Color.green;
        [SerializeField] private Color _midHealthColor = Color.yellow;
        [SerializeField] private Color _lowHealthColor = Color.red;
        [SerializeField] private float _midHealthThreshold = 0.5f;
        [SerializeField] private float _lowHealthThreshold = 0.25f;

        private Transform _cameraTransform;
        private float _targetFillAmount = 1f;
        private float _currentFillAmount = 1f;
        private float _targetAlpha = 1f;
        private bool _isInitialized;
        private bool _cameraCacheFailed;

        public bool HideWhenFull
        {
            get => _hideWhenFull;
            set => _hideWhenFull = value;
        }

        private void Awake()
        {
            CacheMainCamera();
        }

        private void OnEnable()
        {
            // Reset the cache failed flag when re-enabled (camera may be available now)
            _cameraCacheFailed = false;
            CacheMainCamera();
        }

        private void LateUpdate()
        {
            if (!_isInitialized) return;

            UpdateBillboard();
            UpdateFillAnimation();
            UpdateAlphaAnimation();
        }

        /// <summary>
        /// Initializes the health bar with the specified health percentage.
        /// </summary>
        /// <param name="healthPercent">Health percentage from 0 to 1.</param>
        public void Initialize(float healthPercent)
        {
            _isInitialized = true;
            _cameraCacheFailed = false;
            _targetFillAmount = Mathf.Clamp01(healthPercent);
            _currentFillAmount = _targetFillAmount;

            if (_fillImage != null)
            {
                _fillImage.fillAmount = _currentFillAmount;
                UpdateFillColor(_currentFillAmount);
            }

            UpdateVisibility();
            CacheMainCamera();
        }

        /// <summary>
        /// Updates the health bar to display the new health percentage.
        /// </summary>
        /// <param name="healthPercent">Health percentage from 0 to 1.</param>
        public void UpdateHealth(float healthPercent)
        {
            _targetFillAmount = Mathf.Clamp01(healthPercent);
            UpdateVisibility();
        }

        /// <summary>
        /// Resets the health bar state for object pool reuse.
        /// </summary>
        public void ResetHealthBar()
        {
            _targetFillAmount = 1f;
            _currentFillAmount = 1f;
            _targetAlpha = 1f;
            _isInitialized = false;
            _cameraCacheFailed = false;

            if (_fillImage != null)
            {
                _fillImage.fillAmount = 1f;
                UpdateFillColor(1f);
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Shows the health bar.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            _targetAlpha = 1f;
        }

        /// <summary>
        /// Hides the health bar.
        /// </summary>
        public void Hide()
        {
            _targetAlpha = 0f;
        }

        private void CacheMainCamera()
        {
            if (_cameraCacheFailed) return;

            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
            else
            {
                _cameraCacheFailed = true;
            }
        }

        private void UpdateBillboard()
        {
            if (_cameraTransform == null)
            {
                // Only try to cache if we haven't already failed
                if (!_cameraCacheFailed)
                {
                    CacheMainCamera();
                }
                if (_cameraTransform == null) return;
            }

            // Billboard: face the camera
            Vector3 directionToCamera = _cameraTransform.position - transform.position;
            directionToCamera.y = 0f; // Keep upright by ignoring vertical component

            if (directionToCamera.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(-directionToCamera, Vector3.up);
            }
        }

        private void UpdateFillAnimation()
        {
            if (_fillImage == null) return;

            if (!Mathf.Approximately(_currentFillAmount, _targetFillAmount))
            {
                _currentFillAmount = Mathf.Lerp(_currentFillAmount, _targetFillAmount, Time.deltaTime * _smoothSpeed);

                // Snap to target if very close
                if (Mathf.Abs(_currentFillAmount - _targetFillAmount) < 0.001f)
                {
                    _currentFillAmount = _targetFillAmount;
                }

                _fillImage.fillAmount = _currentFillAmount;
                UpdateFillColor(_currentFillAmount);
            }
        }

        private void UpdateAlphaAnimation()
        {
            if (_canvasGroup == null) return;

            if (!Mathf.Approximately(_canvasGroup.alpha, _targetAlpha))
            {
                _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, _targetAlpha, Time.deltaTime * _fadeSpeed);

                // Snap to target if very close
                if (Mathf.Abs(_canvasGroup.alpha - _targetAlpha) < 0.01f)
                {
                    _canvasGroup.alpha = _targetAlpha;
                }

                // Disable gameObject when fully faded out
                if (_targetAlpha <= 0f && _canvasGroup.alpha <= 0.01f)
                {
                    gameObject.SetActive(false);
                }
            }
        }

        private void UpdateFillColor(float fillPercent)
        {
            if (_fillImage == null) return;

            Color targetColor;

            if (fillPercent <= _lowHealthThreshold)
            {
                // Low health: full red
                targetColor = _lowHealthColor;
            }
            else if (fillPercent <= _midHealthThreshold)
            {
                // Interpolate between red and yellow
                float range = _midHealthThreshold - _lowHealthThreshold;
                float t = 0f;
                if (!Mathf.Approximately(range, 0f))
                {
                    t = (fillPercent - _lowHealthThreshold) / range;
                }
                targetColor = Color.Lerp(_lowHealthColor, _midHealthColor, t);
            }
            else
            {
                // Interpolate between yellow and green
                float range = 1f - _midHealthThreshold;
                float t = 0f;
                if (!Mathf.Approximately(range, 0f))
                {
                    t = (fillPercent - _midHealthThreshold) / range;
                }
                targetColor = Color.Lerp(_midHealthColor, _fullHealthColor, t);
            }

            _fillImage.color = targetColor;
        }

        private void UpdateVisibility()
        {
            if (_hideWhenFull && _targetFillAmount >= 1f)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure thresholds are valid
            _lowHealthThreshold = Mathf.Clamp01(_lowHealthThreshold);
            _midHealthThreshold = Mathf.Clamp(_midHealthThreshold, _lowHealthThreshold, 1f);
        }
#endif
    }
}
