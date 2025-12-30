using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Visual indicator that displays around a selected tower.
    /// Uses a LineRenderer to draw a pulsing circle.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class SelectionIndicator : MonoBehaviour
    {
        [Header("Circle Settings")]
        [SerializeField] private float _radius = 0.8f;
        [SerializeField] private int _segments = 32;
        [SerializeField] private float _lineWidth = 0.1f;
        [SerializeField] private float _heightOffset = 0.1f;

        [Header("Pulse Animation")]
        [SerializeField] private bool _enablePulse = true;
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _pulseMinScale = 0.9f;
        [SerializeField] private float _pulseMaxScale = 1.1f;

        [Header("Color")]
        [SerializeField] private Color _color = new Color(0f, 1f, 1f, 1f);
        [SerializeField] private Material _material;

        private LineRenderer _lineRenderer;
        private float _pulseTimer;
        private float _baseRadius;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _baseRadius = _radius;
            SetupLineRenderer();
            DrawCircle(_radius);
        }

        private void Update()
        {
            if (_enablePulse)
            {
                UpdatePulse();
            }
        }

        private void SetupLineRenderer()
        {
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.loop = true;
            _lineRenderer.positionCount = _segments;
            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _lineWidth;
            _lineRenderer.startColor = _color;
            _lineRenderer.endColor = _color;

            if (_material != null)
            {
                _lineRenderer.material = _material;
            }
        }

        private void DrawCircle(float radius)
        {
            if (_lineRenderer == null) return;

            float angleStep = 360f / _segments;
            for (int i = 0; i < _segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float z = Mathf.Sin(angle) * radius;
                _lineRenderer.SetPosition(i, new Vector3(x, _heightOffset, z));
            }
        }

        private void UpdatePulse()
        {
            _pulseTimer += Time.deltaTime * _pulseSpeed;

            // Use sine wave for smooth pulsing
            float pulseValue = (Mathf.Sin(_pulseTimer) + 1f) * 0.5f; // Normalize to 0-1
            float currentScale = Mathf.Lerp(_pulseMinScale, _pulseMaxScale, pulseValue);
            float currentRadius = _baseRadius * currentScale;

            DrawCircle(currentRadius);
        }

        /// <summary>
        /// Sets the base radius of the selection indicator.
        /// </summary>
        /// <param name="radius">The new radius.</param>
        public void SetRadius(float radius)
        {
            _baseRadius = radius;
            _radius = radius;
            DrawCircle(radius);
        }

        /// <summary>
        /// Sets the color of the selection indicator.
        /// </summary>
        /// <param name="color">The new color.</param>
        public void SetColor(Color color)
        {
            _color = color;
            if (_lineRenderer != null)
            {
                _lineRenderer.startColor = color;
                _lineRenderer.endColor = color;
            }
        }

        /// <summary>
        /// Enables or disables the pulse animation.
        /// </summary>
        /// <param name="enabled">True to enable pulsing, false to disable.</param>
        public void SetPulseEnabled(bool enabled)
        {
            _enablePulse = enabled;
            if (!enabled)
            {
                DrawCircle(_baseRadius);
            }
        }
    }
}
