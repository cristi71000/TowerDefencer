## Context

Players need visual and input feedback when interacting with placed towers. This includes selecting towers to view their stats, showing range indicators, and providing options to sell or upgrade. This completes the core placement milestone by making towers interactive after placement.

**Builds upon:** Issues 5-7 (Grid System, Tower Prefab, Placement System)

## Detailed Implementation Instructions

### Tower Selection Manager

Create `TowerSelectionManager.cs` in `_Project/Scripts/Runtime/Towers/`:

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using TowerDefense.Core;

namespace TowerDefense.Towers
{
    public class TowerSelectionManager : MonoBehaviour
    {
        public static TowerSelectionManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private UnityEngine.Camera _mainCamera;
        [SerializeField] private LayerMask _towerLayer;
        [SerializeField] private LayerMask _groundLayer;

        [Header("Selection Visual")]
        [SerializeField] private GameObject _selectionIndicatorPrefab;

        private Tower _selectedTower;
        private GameObject _selectionIndicator;
        private RangeIndicator _rangeIndicator;
        private GameInputActions _inputActions;

        public Tower SelectedTower => _selectedTower;

        public event System.Action<Tower> OnTowerSelected;
        public event System.Action OnTowerDeselected;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _inputActions = new GameInputActions();

            if (_mainCamera == null)
                _mainCamera = UnityEngine.Camera.main;
        }

        private void OnEnable()
        {
            _inputActions.Gameplay.Enable();
            _inputActions.Gameplay.Select.performed += OnSelectPerformed;
        }

        private void OnDisable()
        {
            _inputActions.Gameplay.Select.performed -= OnSelectPerformed;
            _inputActions.Gameplay.Disable();
        }

        private void OnSelectPerformed(InputAction.CallbackContext context)
        {
            // Skip if in placement mode
            if (TowerPlacementManager.Instance != null && TowerPlacementManager.Instance.IsInPlacementMode)
                return;

            TrySelectTower();
        }

        private void TrySelectTower()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

            // First check for tower hit
            if (Physics.Raycast(ray, out RaycastHit towerHit, 100f, _towerLayer))
            {
                Tower tower = towerHit.collider.GetComponentInParent<Tower>();
                if (tower != null)
                {
                    SelectTower(tower);
                    return;
                }
            }

            // If clicked on ground, deselect
            if (Physics.Raycast(ray, out RaycastHit groundHit, 100f, _groundLayer))
            {
                DeselectTower();
            }
        }

        public void SelectTower(Tower tower)
        {
            if (_selectedTower == tower) return;

            DeselectTower();

            _selectedTower = tower;

            // Show selection indicator
            ShowSelectionIndicator(tower);

            // Show range indicator
            ShowRangeIndicator(tower);

            OnTowerSelected?.Invoke(tower);
            Debug.Log($"Selected tower: {tower.Data.TowerName}");
        }

        public void DeselectTower()
        {
            if (_selectedTower == null) return;

            HideSelectionIndicator();
            HideRangeIndicator();

            _selectedTower = null;
            OnTowerDeselected?.Invoke();
        }

        private void ShowSelectionIndicator(Tower tower)
        {
            if (_selectionIndicatorPrefab == null) return;

            if (_selectionIndicator == null)
            {
                _selectionIndicator = Instantiate(_selectionIndicatorPrefab);
            }

            _selectionIndicator.SetActive(true);
            _selectionIndicator.transform.position = tower.transform.position;
        }

        private void HideSelectionIndicator()
        {
            if (_selectionIndicator != null)
            {
                _selectionIndicator.SetActive(false);
            }
        }

        private void ShowRangeIndicator(Tower tower)
        {
            _rangeIndicator = tower.GetComponentInChildren<RangeIndicator>();
            if (_rangeIndicator != null)
            {
                _rangeIndicator.SetRadius(tower.Data.Range);
                _rangeIndicator.Show();
            }
        }

        private void HideRangeIndicator()
        {
            if (_rangeIndicator != null)
            {
                _rangeIndicator.Hide();
                _rangeIndicator = null;
            }
        }

        public void SellSelectedTower()
        {
            if (_selectedTower == null) return;

            Tower towerToSell = _selectedTower;
            DeselectTower();
            TowerPlacementManager.Instance?.SellTower(towerToSell);
        }

        public void CycleTargetingPriority()
        {
            if (_selectedTower == null) return;

            int current = (int)_selectedTower.CurrentPriority;
            int next = (current + 1) % System.Enum.GetValues(typeof(TargetingPriority)).Length;
            _selectedTower.CurrentPriority = (TargetingPriority)next;

            Debug.Log($"Targeting priority changed to: {_selectedTower.CurrentPriority}");
        }
    }
}
```

### Selection Indicator Prefab

Create a visual selection ring:

1. Create empty GameObject "SelectionIndicator"
2. Add child with LineRenderer (circle shape)
3. Or use a simple projected decal/quad

```csharp
// Alternative: Create procedurally
public class SelectionIndicator : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private int _segments = 32;
    [SerializeField] private float _radius = 1.2f;
    [SerializeField] private float _pulseSpeed = 2f;
    [SerializeField] private float _pulseAmount = 0.1f;

    private float _baseRadius;

    private void Awake()
    {
        _baseRadius = _radius;
        CreateCircle();
    }

    private void Update()
    {
        // Pulse animation
        float pulse = Mathf.Sin(Time.time * _pulseSpeed) * _pulseAmount;
        _radius = _baseRadius + pulse;
        CreateCircle();
    }

    private void CreateCircle()
    {
        _lineRenderer.positionCount = _segments + 1;
        _lineRenderer.loop = false;

        float angleStep = 360f / _segments;
        for (int i = 0; i <= _segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * _radius, 0.1f, Mathf.Sin(angle) * _radius);
            _lineRenderer.SetPosition(i, pos);
        }
    }
}
```

Save as `_Project/Prefabs/UI/SelectionIndicator.prefab`

### Tower Context Actions

Create `TowerContextActions.cs` for keyboard shortcuts:

```csharp
using UnityEngine;

namespace TowerDefense.Towers
{
    public class TowerContextActions : MonoBehaviour
    {
        [SerializeField] private KeyCode _sellKey = KeyCode.X;
        [SerializeField] private KeyCode _priorityKey = KeyCode.Tab;

        private void Update()
        {
            if (TowerSelectionManager.Instance?.SelectedTower == null) return;

            if (Input.GetKeyDown(_sellKey))
            {
                TowerSelectionManager.Instance.SellSelectedTower();
            }

            if (Input.GetKeyDown(_priorityKey))
            {
                TowerSelectionManager.Instance.CycleTargetingPriority();
            }
        }
    }
}
```

### ISelectable Interface (Optional)

For extensible selection system:

```csharp
namespace TowerDefense.Core
{
    public interface ISelectable
    {
        void OnSelected();
        void OnDeselected();
        Transform Transform { get; }
    }
}
```

### Scene Setup

1. Create TowerSelectionManager GameObject under --- MANAGEMENT ---
2. Add TowerSelectionManager component
3. Configure:
   - Main Camera reference
   - Tower Layer: Layer 7
   - Ground Layer: Layer 6
   - Selection Indicator Prefab
4. Add TowerContextActions component to same object

### Materials for Selection Indicator

Create `M_SelectionRing.mat`:
- URP Unlit shader
- Color: Bright cyan or white
- Consider additive blending for glow effect

## Testing and Acceptance Criteria

### Manual Test Steps

1. Place a tower using the placement system
2. Exit placement mode
3. Click on the placed tower - verify selection indicator appears
4. Verify range indicator circle shows around tower
5. Click on ground - verify tower deselects
6. Select tower, press X - verify tower sells and currency refunds
7. Select tower, press Tab - verify targeting priority cycles
8. Try selecting while in placement mode - should not select

### Done When

- [ ] TowerSelectionManager singleton created
- [ ] Clicking tower selects it
- [ ] Selection indicator appears around selected tower
- [ ] Range indicator shows tower attack range
- [ ] Clicking ground deselects tower
- [ ] Sell hotkey (X) sells selected tower
- [ ] Priority hotkey (Tab) cycles targeting priority
- [ ] Selection disabled during placement mode
- [ ] Events fire on select/deselect
- [ ] Selection indicator has visual polish (pulse animation)

## Dependencies

- Issue 5: Grid System
- Issue 6: Tower Prefab (RangeIndicator)
- Issue 7: Placement System
