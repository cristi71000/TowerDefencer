using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using TowerDefense.Core;
using TowerDefense;

namespace TowerDefense.Camera
{
    /// <summary>
    /// Controls the isometric camera with pan and zoom functionality.
    /// Supports keyboard panning (WASD/Arrows), edge panning, and mouse wheel zoom.
    /// </summary>
    public class IsometricCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineCamera _virtualCamera;
        [SerializeField] private Transform _cameraTarget;

        [Header("Pan Settings")]
        [SerializeField] private float _panSpeed = 20f;
        [SerializeField] private float _edgePanThreshold = 20f;
        [SerializeField] private bool _enableEdgePan = true;

        [Header("Zoom Settings")]
        [SerializeField] private float _minZoom = 5f;
        [SerializeField] private float _maxZoom = 20f;
        [SerializeField] private float _zoomSpeed = 2f;
        [SerializeField] private float _zoomSmoothTime = 0.1f;

        [Header("Bounds")]
        [SerializeField] private bool _useBounds = true;
        [SerializeField] private Vector2 _boundsMin = new Vector2(-50, -50);
        [SerializeField] private Vector2 _boundsMax = new Vector2(50, 50);

        private GameInputActions _inputActions;
        private Vector2 _panInput;
        private float _zoomInput;
        private float _targetZoom;
        private float _zoomVelocity;

        private void Awake()
        {
            _inputActions = new GameInputActions();

            if (_virtualCamera != null)
            {
                _targetZoom = _virtualCamera.Lens.OrthographicSize;
            }
            else
            {
                _targetZoom = 10f;
                Debug.LogWarning("[CameraController] No virtual camera assigned!");
            }

            if (_cameraTarget == null)
            {
                Debug.LogWarning("[CameraController] No camera target assigned!");
            }
        }

        private void OnEnable()
        {
            _inputActions.Gameplay.Enable();
            _inputActions.Gameplay.CameraMove.performed += OnPan;
            _inputActions.Gameplay.CameraMove.canceled += OnPan;
            _inputActions.Gameplay.CameraZoom.performed += OnZoom;
            _inputActions.Gameplay.CameraZoom.canceled += OnZoom;
        }

        private void OnDisable()
        {
            _inputActions.Gameplay.CameraMove.performed -= OnPan;
            _inputActions.Gameplay.CameraMove.canceled -= OnPan;
            _inputActions.Gameplay.CameraZoom.performed -= OnZoom;
            _inputActions.Gameplay.CameraZoom.canceled -= OnZoom;
            _inputActions.Gameplay.Disable();
        }

        private void OnDestroy()
        {
            _inputActions?.Dispose();
        }

        private void OnPan(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _panInput = Vector2.zero;
            }
            else
            {
                _panInput = context.ReadValue<Vector2>();
            }
        }

        private void OnZoom(InputAction.CallbackContext context)
        {
            _zoomInput = context.ReadValue<float>();
        }

        private void Update()
        {
            // Respect pause state - don't move camera when game is paused
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Paused)
            {
                return;
            }

            HandlePanning();
            HandleEdgePanning();
            HandleZoom();
        }

        private void HandlePanning()
        {
            if (_cameraTarget == null) return;
            if (_panInput.sqrMagnitude < 0.01f) return;

            Vector3 movement = new Vector3(_panInput.x, 0, _panInput.y);
            // Rotate movement by 45 degrees for isometric alignment
            movement = Quaternion.Euler(0, 45, 0) * movement;

            Vector3 newPosition = _cameraTarget.position + movement * _panSpeed * Time.unscaledDeltaTime;
            _cameraTarget.position = ClampToBounds(newPosition);
        }

        private void HandleEdgePanning()
        {
            if (_cameraTarget == null) return;
            if (!_enableEdgePan) return;

            // Check if mouse is available
            if (Mouse.current == null) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();

            // Don't edge pan if mouse is outside the game window
            if (mousePos.x < 0 || mousePos.x > Screen.width ||
                mousePos.y < 0 || mousePos.y > Screen.height)
            {
                return;
            }

            Vector2 edgePan = Vector2.zero;

            if (mousePos.x < _edgePanThreshold)
            {
                edgePan.x = -1;
            }
            else if (mousePos.x > Screen.width - _edgePanThreshold)
            {
                edgePan.x = 1;
            }

            if (mousePos.y < _edgePanThreshold)
            {
                edgePan.y = -1;
            }
            else if (mousePos.y > Screen.height - _edgePanThreshold)
            {
                edgePan.y = 1;
            }

            if (edgePan.sqrMagnitude > 0)
            {
                Vector3 movement = new Vector3(edgePan.x, 0, edgePan.y);
                // Rotate movement by 45 degrees for isometric alignment
                movement = Quaternion.Euler(0, 45, 0) * movement;
                Vector3 newPosition = _cameraTarget.position + movement * _panSpeed * Time.unscaledDeltaTime;
                _cameraTarget.position = ClampToBounds(newPosition);
            }
        }

        private void HandleZoom()
        {
            if (_virtualCamera == null) return;

            if (Mathf.Abs(_zoomInput) > 0.01f)
            {
                // Normalize scroll input (scroll values can be large like 120)
                float normalizedZoom = Mathf.Sign(_zoomInput);
                _targetZoom -= normalizedZoom * _zoomSpeed;
                _targetZoom = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);
                _zoomInput = 0f;
            }

            float currentZoom = _virtualCamera.Lens.OrthographicSize;
            float newZoom = Mathf.SmoothDamp(currentZoom, _targetZoom, ref _zoomVelocity, _zoomSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);

            var lens = _virtualCamera.Lens;
            lens.OrthographicSize = newZoom;
            _virtualCamera.Lens = lens;
        }

        private Vector3 ClampToBounds(Vector3 position)
        {
            if (!_useBounds) return position;

            return new Vector3(
                Mathf.Clamp(position.x, _boundsMin.x, _boundsMax.x),
                position.y,
                Mathf.Clamp(position.z, _boundsMin.y, _boundsMax.y)
            );
        }

        /// <summary>
        /// Sets the camera target position, clamped to bounds if enabled.
        /// </summary>
        /// <param name="worldPosition">The world position to move the camera to.</param>
        public void SetCameraPosition(Vector3 worldPosition)
        {
            if (_cameraTarget != null)
            {
                _cameraTarget.position = ClampToBounds(worldPosition);
            }
        }

        /// <summary>
        /// Sets the target zoom level, clamped to min/max values.
        /// </summary>
        /// <param name="orthographicSize">The target orthographic size.</param>
        public void SetZoom(float orthographicSize)
        {
            _targetZoom = Mathf.Clamp(orthographicSize, _minZoom, _maxZoom);
        }

        // Properties exposed for editor visualization
        internal bool UseBounds => _useBounds;
        internal Vector2 BoundsMin => _boundsMin;
        internal Vector2 BoundsMax => _boundsMax;
    }
}
