using UnityEngine;
using TMPro;

namespace TowerDefense.UI
{
    /// <summary>
    /// Floating text popup that displays currency rewards when enemies are killed.
    /// Billboards toward camera and fades out over its lifetime.
    /// </summary>
    [RequireComponent(typeof(TextMeshPro))]
    public class CurrencyPopup : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _floatSpeed = 1f;

        [Header("Fade")]
        [SerializeField] private float _fadeSpeed = 1f;
        [SerializeField] private float _lifetime = 1.5f;

        [Header("References")]
        [SerializeField] private TextMeshPro _textMesh;

        private UnityEngine.Camera _mainCamera;
        private Color _originalColor;
        private float _elapsedTime;
        private bool _isInitialized;

        private void Awake()
        {
            if (_textMesh == null)
            {
                _textMesh = GetComponent<TextMeshPro>();
            }

            if (_textMesh != null)
            {
                _originalColor = _textMesh.color;
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

            // Float upward
            transform.position += Vector3.up * _floatSpeed * Time.deltaTime;

            // Billboard toward camera
            BillboardToCamera();

            // Fade out over lifetime
            FadeOut();

            // Destroy when lifetime expires
            if (_elapsedTime >= _lifetime)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initializes the popup with the currency amount to display.
        /// </summary>
        /// <param name="amount">The currency amount to show.</param>
        public void Initialize(int amount)
        {
            if (_textMesh != null)
            {
                _textMesh.text = $"+{amount}";
                _originalColor = _textMesh.color;
            }

            _elapsedTime = 0f;
            _isInitialized = true;
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

        private void FadeOut()
        {
            if (_textMesh == null) return;

            // Calculate fade based on lifetime progress
            float fadeProgress = _elapsedTime / _lifetime;
            float alpha = Mathf.Lerp(1f, 0f, fadeProgress * _fadeSpeed);

            Color newColor = _originalColor;
            newColor.a = alpha;
            _textMesh.color = newColor;
        }
    }
}
