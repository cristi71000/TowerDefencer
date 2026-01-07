using UnityEngine;
using TMPro;

namespace TowerDefense.UI
{
    /// <summary>
    /// Floating text that displays damage numbers.
    /// Rises upward, fades out over lifetime, and auto-destroys.
    /// Critical hits are displayed larger and in a different color.
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class FloatingDamageNumber : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _floatSpeed = 1.5f;
        [SerializeField] private float _floatRandomness = 0.3f;

        [Header("Fade")]
        [SerializeField] private float _fadeSpeed = 1f;
        [SerializeField] private float _lifetime = 1.2f;

        [Header("Scale")]
        [SerializeField] private float _normalScale = 1f;
        [SerializeField] private float _criticalScale = 1.5f;
        [SerializeField] private float _scaleAnimationSpeed = 5f;

        [Header("Colors")]
        [SerializeField] private Color _normalColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color _criticalColor = new Color(1f, 0.8f, 0f, 1f);

        [Header("References")]
        [SerializeField] private TextMeshPro _textMesh;

        private UnityEngine.Camera _mainCamera;
        private Color _currentColor;
        private float _elapsedTime;
        private bool _isInitialized;
        private bool _isCritical;
        private Vector3 _floatDirection;
        private float _targetScale;
        private float _currentScale;

        private void Awake()
        {
            if (_textMesh == null)
            {
                _textMesh = GetComponent<TextMeshPro>();
            }
        }

        private void Start()
        {
            _mainCamera = UnityEngine.Camera.main;
        }

        private void Update()
        {
            if (!_isInitialized) return;

            _elapsedTime += Time.deltaTime;

            // Float upward with slight randomness
            transform.position += _floatDirection * _floatSpeed * Time.deltaTime;

            // Billboard toward camera
            BillboardToCamera();

            // Animate scale
            AnimateScale();

            // Fade out over lifetime
            FadeOut();

            // Destroy when lifetime expires
            if (_elapsedTime >= _lifetime)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initializes the floating damage number with the specified damage value.
        /// </summary>
        /// <param name="damage">The damage amount to display.</param>
        /// <param name="isCritical">Whether this was a critical hit.</param>
        public void Initialize(float damage, bool isCritical = false)
        {
            _isCritical = isCritical;
            _elapsedTime = 0f;
            _isInitialized = true;

            // Set text
            if (_textMesh != null)
            {
                int displayDamage = Mathf.RoundToInt(damage);
                _textMesh.text = isCritical ? $"{displayDamage}!" : displayDamage.ToString();

                // Set color based on critical status
                _currentColor = isCritical ? _criticalColor : _normalColor;
                _textMesh.color = _currentColor;
            }

            // Set scale
            _targetScale = isCritical ? _criticalScale : _normalScale;
            _currentScale = _targetScale * 0.5f; // Start smaller for pop effect
            transform.localScale = Vector3.one * _currentScale;

            // Calculate float direction with slight randomness
            float randomX = Random.Range(-_floatRandomness, _floatRandomness);
            float randomZ = Random.Range(-_floatRandomness, _floatRandomness);
            _floatDirection = new Vector3(randomX, 1f, randomZ).normalized;
        }

        /// <summary>
        /// Initializes the floating damage number at a specific position.
        /// </summary>
        /// <param name="position">The world position to spawn at.</param>
        /// <param name="damage">The damage amount to display.</param>
        /// <param name="isCritical">Whether this was a critical hit.</param>
        public void Initialize(Vector3 position, float damage, bool isCritical = false)
        {
            transform.position = position;
            Initialize(damage, isCritical);
        }

        private void BillboardToCamera()
        {
            if (_mainCamera == null)
            {
                _mainCamera = UnityEngine.Camera.main;
                if (_mainCamera == null) return;
            }

            // Face the camera
            Vector3 directionToCamera = _mainCamera.transform.position - transform.position;
            directionToCamera.y = 0f; // Keep upright

            if (directionToCamera.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(-directionToCamera);
            }
        }

        private void AnimateScale()
        {
            if (Mathf.Approximately(_currentScale, _targetScale)) return;

            _currentScale = Mathf.Lerp(_currentScale, _targetScale, Time.deltaTime * _scaleAnimationSpeed);

            // Snap when close enough
            if (Mathf.Abs(_currentScale - _targetScale) < 0.01f)
            {
                _currentScale = _targetScale;
            }

            transform.localScale = Vector3.one * _currentScale;
        }

        private void FadeOut()
        {
            if (_textMesh == null) return;

            // Calculate fade based on lifetime progress
            float fadeProgress = _elapsedTime / _lifetime;
            // Start fading at 50% lifetime
            float fadeStart = 0.5f;
            float alpha = 1f;

            if (fadeProgress > fadeStart)
            {
                float fadeTime = (fadeProgress - fadeStart) / (1f - fadeStart);
                alpha = Mathf.Lerp(1f, 0f, fadeTime * _fadeSpeed);
            }

            Color newColor = _currentColor;
            newColor.a = alpha;
            _textMesh.color = newColor;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure valid values
            _floatSpeed = Mathf.Max(0f, _floatSpeed);
            _lifetime = Mathf.Max(0.1f, _lifetime);
            _normalScale = Mathf.Max(0.1f, _normalScale);
            _criticalScale = Mathf.Max(_normalScale, _criticalScale);
        }
#endif
    }
}
