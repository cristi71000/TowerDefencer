using UnityEngine;

namespace TowerDefense.Towers
{
    [RequireComponent(typeof(LineRenderer))]
    public class RangeIndicator : MonoBehaviour
    {
        [SerializeField] private int _segments = 64;
        [SerializeField] private float _lineWidth = 0.1f;
        [SerializeField] private Material _rangeMaterial;

        private LineRenderer _lineRenderer;

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.loop = true;
            _lineRenderer.positionCount = _segments;
            _lineRenderer.startWidth = _lineWidth;
            _lineRenderer.endWidth = _lineWidth;
            if (_rangeMaterial != null) _lineRenderer.material = _rangeMaterial;
            gameObject.SetActive(false);
        }

        public void SetRadius(float radius)
        {
            if (_lineRenderer == null) return;

            float angleStep = 360f / _segments;
            for (int i = 0; i < _segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                _lineRenderer.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0.1f, Mathf.Sin(angle) * radius));
            }
        }

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);
    }
}
