## Context

The isometric camera is the player's primary viewport into the game world. Before we can implement placement or combat systems, we need a camera that supports the isometric perspective typical of tower defense games, with smooth panning and zooming to navigate the map. This uses Cinemachine for camera management and the New Input System for controls.

**Builds upon:** Issues 1-2 (Project Setup, Game Manager)

## Detailed Implementation Instructions

### Camera Rig Setup

Create the following GameObject hierarchy in Main.unity under --- CAMERAS ---:

```
--- CAMERAS ---
|-- CameraRig (empty GameObject at origin)
|   +-- CameraTarget (empty, child of CameraRig)
|-- Main Camera (with CinemachineBrain)
+-- IsometricVirtualCamera (CinemachineVirtualCamera)
```

### Cinemachine Configuration

1. **Main Camera Setup:**
   - Add `CinemachineBrain` component
   - Set Update Method: Fixed Update
   - Set Blend Update Method: Fixed Update

2. **IsometricVirtualCamera Setup:**
   - Add `CinemachineVirtualCamera` component
   - Set Follow: CameraTarget
   - Set Look At: CameraTarget
   - Body: Transposer
     - Binding Mode: Lock To Target
     - Follow Offset: (0, 10, -10) for 45-degree isometric
   - Aim: Do Nothing
   - Lens: Orthographic
   - Orthographic Size: 10 (default, will be controlled by zoom)

### Isometric Camera Controller Script

Create `IsometricCameraController.cs` in `_Project/Scripts/Runtime/Camera/`:

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

namespace TowerDefense.Camera
{
    public class IsometricCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
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
            _targetZoom = _virtualCamera.m_Lens.OrthographicSize;
        }

        private void OnEnable()
        {
            _inputActions.Gameplay.Enable();
            _inputActions.Gameplay.CameraPan.performed += OnPan;
            _inputActions.Gameplay.CameraPan.canceled += OnPan;
            _inputActions.Gameplay.CameraZoom.performed += OnZoom;
            _inputActions.Gameplay.CameraZoom.canceled += OnZoom;
        }

        private void OnDisable()
        {
            _inputActions.Gameplay.CameraPan.performed -= OnPan;
            _inputActions.Gameplay.CameraPan.canceled -= OnPan;
            _inputActions.Gameplay.CameraZoom.performed -= OnZoom;
            _inputActions.Gameplay.CameraZoom.canceled -= OnZoom;
            _inputActions.Gameplay.Disable();
        }

        private void OnPan(InputAction.CallbackContext context)
        {
            _panInput = context.ReadValue<Vector2>();
        }

        private void OnZoom(InputAction.CallbackContext context)
        {
            _zoomInput = context.ReadValue<float>();
        }

        private void Update()
        {
            HandlePanning();
            HandleEdgePanning();
            HandleZoom();
        }

        private void HandlePanning()
        {
            if (_panInput.sqrMagnitude < 0.01f) return;

            Vector3 movement = new Vector3(_panInput.x, 0, _panInput.y);
            movement = Quaternion.Euler(0, 45, 0) * movement;

            Vector3 newPosition = _cameraTarget.position + movement * _panSpeed * Time.deltaTime;
            _cameraTarget.position = ClampToBounds(newPosition);
        }

        private void HandleEdgePanning()
        {
            if (!_enableEdgePan) return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 edgePan = Vector2.zero;

            if (mousePos.x < _edgePanThreshold) edgePan.x = -1;
            else if (mousePos.x > Screen.width - _edgePanThreshold) edgePan.x = 1;

            if (mousePos.y < _edgePanThreshold) edgePan.y = -1;
            else if (mousePos.y > Screen.height - _edgePanThreshold) edgePan.y = 1;

            if (edgePan.sqrMagnitude > 0)
            {
                Vector3 movement = new Vector3(edgePan.x, 0, edgePan.y);
                movement = Quaternion.Euler(0, 45, 0) * movement;
                Vector3 newPosition = _cameraTarget.position + movement * _panSpeed * Time.deltaTime;
                _cameraTarget.position = ClampToBounds(newPosition);
            }
        }

        private void HandleZoom()
        {
            if (Mathf.Abs(_zoomInput) > 0.01f)
            {
                _targetZoom -= _zoomInput * _zoomSpeed;
                _targetZoom = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);
            }

            float currentZoom = _virtualCamera.m_Lens.OrthographicSize;
            float newZoom = Mathf.SmoothDamp(currentZoom, _targetZoom, ref _zoomVelocity, _zoomSmoothTime);
            _virtualCamera.m_Lens.OrthographicSize = newZoom;
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

        public void SetCameraPosition(Vector3 worldPosition)
        {
            _cameraTarget.position = ClampToBounds(worldPosition);
        }

        public void SetZoom(float orthographicSize)
        {
            _targetZoom = Mathf.Clamp(orthographicSize, _minZoom, _maxZoom);
        }
    }
}
```

### Input Actions Configuration

Update `GameInputActions.inputactions` to include:

**Gameplay Action Map:**
- **CameraPan** (Value, Vector2)
  - Keyboard: WASD composite (W=up, S=down, A=left, D=right)
  - Keyboard: Arrow keys composite
- **CameraZoom** (Value, Axis)
  - Mouse: Scroll Y
  - Keyboard: +/- keys (optional)

### Scene Setup

1. Create the camera hierarchy as specified above
2. Add `IsometricCameraController` to the CameraRig GameObject
3. Wire up references:
   - Virtual Camera reference to IsometricVirtualCamera
   - Camera Target reference to CameraTarget transform
4. Set the CameraTarget initial position to center of your expected play area

### Camera Bounds Visualization (Editor Only)

Create `IsometricCameraControllerEditor.cs` in `_Project/Scripts/Editor/`:

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TowerDefense.Editor
{
    [CustomEditor(typeof(IsometricCameraController))]
    public class IsometricCameraControllerEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var boundsMin = serializedObject.FindProperty("_boundsMin").vector2Value;
            var boundsMax = serializedObject.FindProperty("_boundsMax").vector2Value;
            var useBounds = serializedObject.FindProperty("_useBounds").boolValue;

            if (!useBounds) return;

            Handles.color = Color.yellow;
            Vector3[] corners = new Vector3[]
            {
                new Vector3(boundsMin.x, 0, boundsMin.y),
                new Vector3(boundsMax.x, 0, boundsMin.y),
                new Vector3(boundsMax.x, 0, boundsMax.y),
                new Vector3(boundsMin.x, 0, boundsMax.y),
                new Vector3(boundsMin.x, 0, boundsMin.y)
            };
            Handles.DrawPolyLine(corners);
        }
    }
}
#endif
```

## Testing and Acceptance Criteria

### Manual Test Steps

1. Enter Play mode
2. **Pan Test (Keyboard):**
   - Press W/S/A/D - camera should pan smoothly in isometric directions
   - Press arrow keys - same behavior
3. **Pan Test (Edge):**
   - Move mouse to screen edges - camera should pan when near edges
   - Disable edge pan in Inspector, verify it stops
4. **Zoom Test:**
   - Scroll mouse wheel up - camera should zoom in (smaller ortho size)
   - Scroll mouse wheel down - camera should zoom out
   - Verify zoom stops at min/max limits
5. **Bounds Test:**
   - Pan to edges of bounds - camera should stop at boundary
   - Disable bounds, verify camera can move freely
6. **Smooth Motion:**
   - Verify all camera movements are smooth, not jerky
   - Zoom should ease in/out

### Edge Cases

- [ ] Camera does not move when game is paused (Time.timeScale = 0)
- [ ] Zoom works smoothly at both extremes
- [ ] Edge pan does not trigger when cursor is outside game window
- [ ] Multiple inputs combine correctly (WASD + edge pan)

### Done When

- [ ] Camera uses orthographic projection (isometric style)
- [ ] WASD and arrow keys pan the camera in world space
- [ ] Mouse wheel zooms in/out smoothly
- [ ] Edge panning works and can be toggled
- [ ] Camera movement bounded to configurable area
- [ ] All movements are smooth with appropriate easing
- [ ] Camera bounds visible in Scene view (Editor)
- [ ] Camera respects pause state
- [ ] No console errors during operation

## Assets and Resources

- **Cinemachine** package (already installed in Issue 1)
- **Input System** package (already installed in Issue 1)

## Dependencies

- Issue 1: Project Setup (packages, input actions)
- Issue 2: Game Manager (pause state checking)

## Notes

- The 45-degree rotation for isometric movement assumes a standard isometric setup; adjust if using different angles
- Consider adding camera shake support later (VFX milestone) via Cinemachine Impulse
- Edge panning threshold might need tuning based on display resolution
- For mobile support, add touch-based pan and pinch-to-zoom later
