using UnityEngine;
using UnityEngine.InputSystem;
using TowerDefense.Core;
using TowerDefense.Core.Events;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Manages tower placement, including preview visualization, validation, and instantiation.
    /// Handles user input for selecting cells and placing towers on the grid.
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
        private Renderer[] _previewRenderers;

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
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    Debug.LogError("[TowerPlacementManager] No camera found! Please assign a camera or ensure one is tagged as MainCamera.");
                }
            }
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

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (!_isInPlacementMode || _previewInstance == null) return;

            UpdatePreviewPosition();
        }

        /// <summary>
        /// Initiates tower placement mode for the specified tower type.
        /// Creates a preview and enables grid visualization.
        /// </summary>
        /// <param name="towerData">The tower data to place</param>
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

            _previewRenderers = null;
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

            // Cache renderers to avoid repeated GetComponentsInChildren calls
            _previewRenderers = _previewInstance.GetComponentsInChildren<Renderer>();

            // Set initial preview material
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
            if (_previewRenderers == null || material == null) return;

            foreach (Renderer renderer in _previewRenderers)
            {
                if (renderer == null) continue;

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

        /// <summary>
        /// Sells a tower, refunding currency and freeing the grid cell.
        /// </summary>
        /// <param name="tower">The tower to sell</param>
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
