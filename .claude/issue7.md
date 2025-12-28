## Context

Now that we have the grid system and tower prefab, we need the placement system that allows players to select a tower type, preview where it will be placed, validate the placement location, and instantiate the tower. This is the primary interaction system players use to build their defenses.

**Builds upon:** Issues 5-6 (Grid System, TowerData and Basic Tower Prefab)

## Detailed Implementation Instructions

### Tower Placement Manager

Create `TowerPlacementManager.cs` in `_Project/Scripts/Runtime/Towers/`:

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using TowerDefense.Core;

namespace TowerDefense.Towers
{
    public class TowerPlacementManager : MonoBehaviour
    {
        public static TowerPlacementManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private GridVisualizer _gridVisualizer;
        [SerializeField] private UnityEngine.Camera _mainCamera;
        [SerializeField] private LayerMask _groundLayer;

        [Header("Preview")]
        [SerializeField] private Material _validPreviewMaterial;
        [SerializeField] private Material _invalidPreviewMaterial;

        [Header("Events")]
        [SerializeField] private Vector3EventChannel _onTowerPlaced;

        private TowerData _selectedTowerData;
        private GameObject _previewInstance;
        private Vector2Int _currentGridPosition;
        private bool _isPlacementValid;
        private bool _isInPlacementMode;

        private GameInputActions _inputActions;

        public bool IsInPlacementMode => _isInPlacementMode;
        public TowerData SelectedTower => _selectedTowerData;

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
            _inputActions.Gameplay.Cancel.performed += OnCancelPerformed;
        }

        private void OnDisable()
        {
            _inputActions.Gameplay.Select.performed -= OnSelectPerformed;
            _inputActions.Gameplay.Cancel.performed -= OnCancelPerformed;
            _inputActions.Gameplay.Disable();
        }

        private void Update()
        {
            if (!_isInPlacementMode) return;

            UpdatePreviewPosition();
        }

        public void StartPlacement(TowerData towerData)
        {
            if (towerData == null || towerData.Prefab == null)
            {
                Debug.LogError("Invalid tower data for placement!");
                return;
            }

            // Check if player can afford
            if (GameManager.Instance.CurrentCurrency < towerData.PurchaseCost)
            {
                Debug.Log("Not enough currency to place this tower!");
                return;
            }

            CancelPlacement();

            _selectedTowerData = towerData;
            _isInPlacementMode = true;

            CreatePreview();
            _gridVisualizer?.SetGridVisible(true);
        }

        public void CancelPlacement()
        {
            if (_previewInstance != null)
            {
                Destroy(_previewInstance);
                _previewInstance = null;
            }

            _selectedTowerData = null;
            _isInPlacementMode = false;
            _gridVisualizer?.SetGridVisible(false);
            _gridVisualizer?.ClearHover();
        }

        private void CreatePreview()
        {
            _previewInstance = Instantiate(_selectedTowerData.Prefab);
            _previewInstance.name = "TowerPreview";

            // Disable tower functionality on preview
            Tower tower = _previewInstance.GetComponent<Tower>();
            if (tower != null) tower.enabled = false;

            // Disable colliders
            foreach (Collider col in _previewInstance.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            // Set preview material
            SetPreviewMaterial(_validPreviewMaterial);
        }

        private void UpdatePreviewPosition()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _groundLayer))
            {
                _currentGridPosition = _gridManager.WorldToGridPosition(hit.point);
                _isPlacementValid = _gridManager.CanPlaceAt(_currentGridPosition);

                // Snap preview to grid
                Vector3 snappedPosition = _gridManager.GridToWorldPosition(_currentGridPosition);
                _previewInstance.transform.position = snappedPosition;

                // Update visuals
                SetPreviewMaterial(_isPlacementValid ? _validPreviewMaterial : _invalidPreviewMaterial);
                _gridVisualizer?.SetHoverCell(_currentGridPosition, _isPlacementValid);
            }
        }

        private void SetPreviewMaterial(Material material)
        {
            foreach (Renderer renderer in _previewInstance.GetComponentsInChildren<Renderer>())
            {
                Material[] mats = new Material[renderer.materials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = material;
                }
                renderer.materials = mats;
            }
        }

        private void OnSelectPerformed(InputAction.CallbackContext context)
        {
            if (!_isInPlacementMode) return;

            TryPlaceTower();
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            if (_isInPlacementMode)
            {
                CancelPlacement();
            }
        }

        private void TryPlaceTower()
        {
            if (!_isPlacementValid)
            {
                Debug.Log("Cannot place tower here!");
                return;
            }

            // Check currency again
            if (!GameManager.Instance.TrySpendCurrency(_selectedTowerData.PurchaseCost))
            {
                Debug.Log("Not enough currency!");
                CancelPlacement();
                return;
            }

            // Place the tower
            PlaceTower(_currentGridPosition);
        }

        private void PlaceTower(Vector2Int gridPosition)
        {
            Vector3 worldPosition = _gridManager.GridToWorldPosition(gridPosition);

            // Instantiate actual tower
            GameObject towerObject = Instantiate(_selectedTowerData.Prefab, worldPosition, Quaternion.identity);
            towerObject.name = $"{_selectedTowerData.TowerName}_{gridPosition.x}_{gridPosition.y}";

            // Initialize tower
            Tower tower = towerObject.GetComponent<Tower>();
            if (tower != null)
            {
                tower.Initialize(_selectedTowerData, gridPosition);
            }

            // Mark cell as occupied
            _gridManager.TryOccupyCell(gridPosition, towerObject);

            // Update grid visual
            _gridVisualizer?.UpdateCellVisual(gridPosition);

            // Raise event
            _onTowerPlaced?.RaiseEvent(worldPosition);

            Debug.Log($"Placed {_selectedTowerData.TowerName} at {gridPosition}");

            // Continue placement mode or exit
            if (GameManager.Instance.CurrentCurrency >= _selectedTowerData.PurchaseCost)
            {
                // Can afford another, stay in placement mode
                UpdatePreviewPosition();
            }
            else
            {
                // Cannot afford, exit placement mode
                CancelPlacement();
            }
        }

        // Public method to sell a tower
        public void SellTower(Tower tower)
        {
            if (tower == null) return;

            Vector2Int gridPos = tower.GridPosition;
            int sellValue = tower.Data.SellValue;

            // Free the cell
            _gridManager.FreeCell(gridPos);
            _gridVisualizer?.UpdateCellVisual(gridPos);

            // Refund currency
            GameManager.Instance.ModifyCurrency(sellValue);

            // Destroy tower
            Destroy(tower.gameObject);

            Debug.Log($"Sold tower for {sellValue} currency");
        }
    }
}
```

### Input Actions Update

Add to `GameInputActions.inputactions` Gameplay action map:

- **Select** (Button)
  - Mouse: Left Button
- **Cancel** (Button)
  - Keyboard: Escape
  - Mouse: Right Button

### Preview Materials

Create in `_Project/Art/Materials/`:

- `M_Preview_Valid.mat`
  - URP Lit shader, Surface Type: Transparent
  - Base Color: RGBA(0, 255, 0, 128) - Semi-transparent green

- `M_Preview_Invalid.mat`
  - URP Lit shader, Surface Type: Transparent
  - Base Color: RGBA(255, 0, 0, 128) - Semi-transparent red

### Scene Setup

1. Create TowerPlacementManager GameObject under --- MANAGEMENT ---
2. Add TowerPlacementManager component
3. Wire up references:
   - Grid Manager
   - Grid Visualizer
   - Main Camera
   - Ground Layer (Layer 6)
   - Preview materials
   - OnTowerPlaced event channel

### Test Tower Selection (Temporary)

Create `TowerSelector.cs` for testing:

```csharp
using UnityEngine;
using TowerDefense.Towers;

namespace TowerDefense.Debug
{
    public class TowerSelector : MonoBehaviour
    {
        [SerializeField] private TowerData[] _availableTowers;
        [SerializeField] private KeyCode[] _towerKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

        private void Update()
        {
            for (int i = 0; i < _availableTowers.Length && i < _towerKeys.Length; i++)
            {
                if (Input.GetKeyDown(_towerKeys[i]))
                {
                    TowerPlacementManager.Instance?.StartPlacement(_availableTowers[i]);
                }
            }
        }
    }
}
```

## Testing and Acceptance Criteria

### Manual Test Steps

1. Enter Play mode
2. Press 1 to select Basic Tower for placement
3. Verify grid becomes visible
4. Move mouse over valid cells - preview should be green
5. Move mouse over blocked cells - preview should be red
6. Left-click on valid cell - tower should place
7. Verify currency decreases by tower cost
8. Verify cell becomes occupied (yellow)
9. Press Escape to cancel placement mode
10. Right-click should also cancel
11. Try to place when insufficient currency - should fail

### Done When

- [ ] TowerPlacementManager singleton created
- [ ] Placement mode activates with tower data
- [ ] Preview shows tower at mouse position
- [ ] Preview snaps to grid cells
- [ ] Valid/invalid positions show correct materials
- [ ] Grid visualizer shows during placement
- [ ] Left-click places tower at valid position
- [ ] Cancel (Escape/Right-click) exits placement mode
- [ ] Currency is spent on placement
- [ ] Cell marked as occupied after placement
- [ ] Event fires on tower placement
- [ ] SellTower properly refunds and removes tower

## Dependencies

- Issue 5: Grid System
- Issue 6: TowerData and Basic Tower Prefab
- Issue 2: Game Manager (currency)
