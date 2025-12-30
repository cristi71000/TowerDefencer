using UnityEngine;
using UnityEngine.InputSystem;
using TowerDefense.Core;
using TowerDefense.Core.Events;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Manages tower placement including preview, validation, and instantiation.
    /// Allows players to select a tower type, preview placement, and place towers on valid grid cells.
    /// </summary>
    public class TowerPlacementManager : MonoBehaviour
    {
        public static TowerPlacementManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private GridVisualizer _gridVisualizer;
        [SerializeField] private Camera _mainCamera;
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
                _mainCamera = Camera.main;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            _inputActions?.Dispose();
        }

        private void OnEnable()
        {
            if (_inputActions == null) return;

            _inputActions.Gameplay.Enable();
            _inputActions.Gameplay.Select.performed += OnSelectPerformed;
            _inputActions.Gameplay.Cancel.performed += OnCancelPerformed;
        }

        private void OnDisable()
        {
            if (_inputActions == null) return;

            _inputActions.Gameplay.Select.performed -= OnSelectPerformed;
            _inputActions.Gameplay.Cancel.performed -= OnCancelPerformed;
            _inputActions.Gameplay.Disable();
        }

        private void Start()
        {
            if (_gridManager == null)
            {
                _gridManager = GridManager.Instance;
            }

            if (_gridManager == null)
            {
                Debug.LogError("[TowerPlacementManager] No GridManager found!");
            }
        }

        private void Update()
        {
            if (!_isInPlacementMode) return;

            UpdatePreviewPosition();
        }

        /// <summary>
        /// Starts placement mode for the specified tower type.
        /// </summary>
        /// <param name="towerData">The tower data to place.</param>
        public void StartPlacement(TowerData towerData)
        {
            if (towerData == null || towerData.Prefab == null)
            {
                Debug.LogError("[TowerPlacementManager] Invalid tower data for placement!");
                return;
            }

            // Check if player can afford
            if (GameManager.Instance == null)
            {
                Debug.LogError("[TowerPlacementManager] GameManager not found!");
                return;
            }

            if (GameManager.Instance.CurrentCurrency < towerData.PurchaseCost)
            {
                Debug.Log($"[TowerPlacementManager] Not enough currency to place {towerData.TowerName}! " +
                         $"Need {towerData.PurchaseCost}, have {GameManager.Instance.CurrentCurrency}");
                return;
            }

            CancelPlacement();

            _selectedTowerData = towerData;
            _isInPlacementMode = true;

            CreatePreview();
            _gridVisualizer?.SetGridVisible(true);

            Debug.Log($"[TowerPlacementManager] Started placement mode for {towerData.TowerName}");
        }

        /// <summary>
        /// Cancels the current placement mode and cleans up the preview.
        /// </summary>
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

            // Disable colliders to prevent physics interactions
            foreach (Collider col in _previewInstance.GetComponentsInChildren<Collider>())
            {
                col.enabled = false;
            }

            // Set initial preview material
            SetPreviewMaterial(_validPreviewMaterial);
        }

        private void UpdatePreviewPosition()
        {
            if (_previewInstance == null) return;
            if (_gridManager == null) return;
            if (Mouse.current == null) return;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _groundLayer))
            {
                _currentGridPosition = _gridManager.WorldToGridPosition(hit.point);
                _isPlacementValid = _gridManager.CanPlaceAt(_currentGridPosition);

                // Snap preview to grid
                Vector3 snappedPosition = _gridManager.GridToWorldPosition(_currentGridPosition);
                _previewInstance.transform.position = snappedPosition;

                // Update visuals based on validity
                SetPreviewMaterial(_isPlacementValid ? _validPreviewMaterial : _invalidPreviewMaterial);
                _gridVisualizer?.SetHoverCell(_currentGridPosition, _isPlacementValid);
            }
            else
            {
                // Mouse is not over valid ground
                _isPlacementValid = false;
                SetPreviewMaterial(_invalidPreviewMaterial);
            }
        }

        private void SetPreviewMaterial(Material material)
        {
            if (_previewInstance == null || material == null) return;

            foreach (Renderer renderer in _previewInstance.GetComponentsInChildren<Renderer>())
            {
                var sharedMats = renderer.sharedMaterials;
                for (int i = 0; i < sharedMats.Length; i++)
                {
                    sharedMats[i] = material;
                }
                renderer.sharedMaterials = sharedMats;
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
                Debug.Log("[TowerPlacementManager] Placement cancelled");
            }
        }

        private void TryPlaceTower()
        {
            if (!_isPlacementValid)
            {
                Debug.Log("[TowerPlacementManager] Cannot place tower here!");
                return;
            }

            if (GameManager.Instance == null)
            {
                Debug.LogError("[TowerPlacementManager] GameManager not found!");
                CancelPlacement();
                return;
            }

            // Check currency again (in case it changed)
            if (!GameManager.Instance.TrySpendCurrency(_selectedTowerData.PurchaseCost))
            {
                Debug.Log("[TowerPlacementManager] Not enough currency!");
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
            if (!_gridManager.TryOccupyCell(gridPosition, towerObject))
            {
                Debug.LogError($"[TowerPlacementManager] Failed to occupy cell at {gridPosition}!");
                Destroy(towerObject);
                return;
            }

            // Update grid visual
            _gridVisualizer?.UpdateCellVisual(gridPosition);

            // Raise event
            _onTowerPlaced?.RaiseEvent(worldPosition);

            Debug.Log($"[TowerPlacementManager] Placed {_selectedTowerData.TowerName} at {gridPosition}");

            // Continue placement mode or exit based on remaining currency
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentCurrency >= _selectedTowerData.PurchaseCost)
            {
                // Can afford another, stay in placement mode
                UpdatePreviewPosition();
            }
            else
            {
                // Cannot afford or GameManager unavailable, exit placement mode
                Debug.Log("[TowerPlacementManager] Not enough currency for another tower, exiting placement mode");
                CancelPlacement();
            }
        }

        /// <summary>
        /// Sells a tower, refunding currency and freeing the grid cell.
        /// </summary>
        /// <param name="tower">The tower to sell.</param>
        public void SellTower(Tower tower)
        {
            if (tower == null) return;

            Vector2Int gridPos = tower.GridPosition;

            // Get sell value with null check
            int sellValue = 0;
            if (tower.Data != null)
            {
                sellValue = tower.Data.SellValue;
            }
            else
            {
                Debug.LogWarning("[TowerPlacementManager] Tower data is null, cannot determine sell value!");
            }

            // Free the cell
            if (_gridManager != null)
            {
                _gridManager.FreeCell(gridPos);
            }
            else
            {
                Debug.LogWarning("[TowerPlacementManager] GridManager is null, cannot free cell!");
            }
            _gridVisualizer?.UpdateCellVisual(gridPos);

            // Refund currency
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ModifyCurrency(sellValue);
            }
            else
            {
                Debug.LogWarning("[TowerPlacementManager] GameManager not found, cannot refund currency!");
            }

            // Destroy tower
            Destroy(tower.gameObject);

            Debug.Log($"[TowerPlacementManager] Sold tower for {sellValue} currency");
        }
    }
}
