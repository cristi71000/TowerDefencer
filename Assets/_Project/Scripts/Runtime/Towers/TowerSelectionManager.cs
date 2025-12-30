using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Manages tower selection, allowing players to click on placed towers to select them,
    /// view selection and range indicators, and perform context actions like selling.
    /// </summary>
    public class TowerSelectionManager : MonoBehaviour
    {
        public static TowerSelectionManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private UnityEngine.Camera _mainCamera;
        [SerializeField] private LayerMask _towerLayer;
        [SerializeField] private LayerMask _groundLayer;

        [Header("Indicators")]
        [SerializeField] private GameObject _selectionIndicatorPrefab;
        [SerializeField] private RangeIndicator _rangeIndicatorPrefab;

        private Tower _selectedTower;
        private GameObject _selectionIndicatorInstance;
        private RangeIndicator _rangeIndicatorInstance;

        private GameInputActions _inputActions;

        /// <summary>
        /// The currently selected tower, or null if no tower is selected.
        /// </summary>
        public Tower SelectedTower => _selectedTower;

        /// <summary>
        /// Returns true if a tower is currently selected.
        /// </summary>
        public bool HasSelection => _selectedTower != null;

        /// <summary>
        /// Event raised when a tower is selected.
        /// </summary>
        public event Action<Tower> OnTowerSelected;

        /// <summary>
        /// Event raised when a tower is deselected.
        /// </summary>
        public event Action<Tower> OnTowerDeselected;

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
                _mainCamera = UnityEngine.Camera.main;
            }

            CreateIndicators();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            _inputActions?.Dispose();

            if (_selectionIndicatorInstance != null)
            {
                Destroy(_selectionIndicatorInstance);
            }

            if (_rangeIndicatorInstance != null)
            {
                Destroy(_rangeIndicatorInstance.gameObject);
            }
        }

        private void OnEnable()
        {
            if (_inputActions == null) return;

            _inputActions.Gameplay.Enable();
            _inputActions.Gameplay.Select.performed += OnSelectPerformed;
        }

        private void OnDisable()
        {
            if (_inputActions == null) return;

            _inputActions.Gameplay.Select.performed -= OnSelectPerformed;
            _inputActions.Gameplay.Disable();
        }

        private void CreateIndicators()
        {
            // Create selection indicator instance
            if (_selectionIndicatorPrefab != null)
            {
                _selectionIndicatorInstance = Instantiate(_selectionIndicatorPrefab);
                _selectionIndicatorInstance.name = "SelectionIndicator";
                _selectionIndicatorInstance.SetActive(false);
            }

            // Create range indicator instance
            if (_rangeIndicatorPrefab != null)
            {
                _rangeIndicatorInstance = Instantiate(_rangeIndicatorPrefab);
                _rangeIndicatorInstance.name = "SelectionRangeIndicator";
                _rangeIndicatorInstance.Hide();
            }
        }

        private void OnValidate()
        {
            if (_selectionIndicatorPrefab == null)
                UnityEngine.Debug.LogWarning("[TowerSelectionManager] Selection Indicator Prefab not assigned!");
            if (_rangeIndicatorPrefab == null)
                UnityEngine.Debug.LogWarning("[TowerSelectionManager] Range Indicator Prefab not assigned!");
        }

        private void OnSelectPerformed(InputAction.CallbackContext context)
        {
            // Skip selection if in placement mode
            if (TowerPlacementManager.Instance != null && TowerPlacementManager.Instance.IsInPlacementMode)
            {
                return;
            }

            TrySelectTower();
        }

        private void TrySelectTower()
        {
            if (_mainCamera == null) return;
            if (Mouse.current == null) return;

            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

            // First, try to hit a tower
            if (Physics.Raycast(ray, out RaycastHit towerHit, 100f, _towerLayer))
            {
                Tower tower = towerHit.collider.GetComponentInParent<Tower>();
                if (tower != null)
                {
                    SelectTower(tower);
                    return;
                }
            }

            // If we hit ground instead, deselect current tower
            if (Physics.Raycast(ray, out RaycastHit groundHit, 100f, _groundLayer))
            {
                DeselectTower();
            }
        }

        /// <summary>
        /// Selects the specified tower, showing selection and range indicators.
        /// </summary>
        /// <param name="tower">The tower to select.</param>
        public void SelectTower(Tower tower)
        {
            if (tower == null) return;

            // Deselect previous tower if different
            if (_selectedTower != null && _selectedTower != tower)
            {
                Tower previousTower = _selectedTower;
                _selectedTower = null;
                OnTowerDeselected?.Invoke(previousTower);
            }

            _selectedTower = tower;

            ShowSelectionIndicator(tower);
            ShowRangeIndicator(tower);

            OnTowerSelected?.Invoke(tower);

            UnityEngine.Debug.Log($"[TowerSelectionManager] Selected tower: {tower.name}");
        }

        /// <summary>
        /// Deselects the currently selected tower.
        /// </summary>
        public void DeselectTower()
        {
            if (_selectedTower == null) return;

            Tower previousTower = _selectedTower;
            _selectedTower = null;

            HideSelectionIndicator();
            HideRangeIndicator();

            OnTowerDeselected?.Invoke(previousTower);

            UnityEngine.Debug.Log($"[TowerSelectionManager] Deselected tower: {previousTower.name}");
        }

        /// <summary>
        /// Sells the currently selected tower, refunding currency and freeing the grid cell.
        /// </summary>
        public void SellSelectedTower()
        {
            if (_selectedTower == null)
            {
                UnityEngine.Debug.Log("[TowerSelectionManager] No tower selected to sell.");
                return;
            }

            // Check if tower was already destroyed externally
            if (_selectedTower.gameObject == null)
            {
                UnityEngine.Debug.LogWarning("[TowerSelectionManager] Selected tower was already destroyed!");
                _selectedTower = null;
                HideSelectionIndicator();
                HideRangeIndicator();
                OnTowerDeselected?.Invoke(null);
                return;
            }

            Tower towerToSell = _selectedTower;
            DeselectTower();

            if (TowerPlacementManager.Instance != null)
            {
                TowerPlacementManager.Instance.SellTower(towerToSell);
            }
            else
            {
                UnityEngine.Debug.LogWarning("[TowerSelectionManager] TowerPlacementManager not found, cannot sell tower.");
            }
        }

        /// <summary>
        /// Cycles the targeting priority of the currently selected tower.
        /// </summary>
        public void CycleTargetingPriority()
        {
            if (_selectedTower == null || _selectedTower.Data == null)
            {
                UnityEngine.Debug.LogWarning("[TowerSelectionManager] Cannot cycle priority - no valid tower selected");
                return;
            }

            // Get all priority values
            TargetingPriority[] priorities = (TargetingPriority[])Enum.GetValues(typeof(TargetingPriority));
            int currentIndex = Array.IndexOf(priorities, _selectedTower.CurrentPriority);
            int nextIndex = (currentIndex + 1) % priorities.Length;

            _selectedTower.CurrentPriority = priorities[nextIndex];

            UnityEngine.Debug.Log($"[TowerSelectionManager] Changed {_selectedTower.name} targeting priority to: {_selectedTower.CurrentPriority}");
        }

        #region Indicator Helper Methods

        private void ShowSelectionIndicator(Tower tower)
        {
            if (_selectionIndicatorInstance == null)
            {
                UnityEngine.Debug.LogWarning("[TowerSelectionManager] Selection indicator was destroyed! Recreating...");
                CreateIndicators();
            }

            if (_selectionIndicatorInstance != null)
            {
                _selectionIndicatorInstance.transform.position = tower.transform.position;
                _selectionIndicatorInstance.transform.SetParent(tower.transform);
                _selectionIndicatorInstance.SetActive(true);
            }
        }

        private void ShowRangeIndicator(Tower tower)
        {
            if (_rangeIndicatorInstance == null)
            {
                UnityEngine.Debug.LogWarning("[TowerSelectionManager] Range indicator was destroyed! Recreating...");
                CreateIndicators();
            }

            if (_rangeIndicatorInstance != null && tower.Data != null)
            {
                _rangeIndicatorInstance.transform.position = tower.transform.position;
                _rangeIndicatorInstance.transform.SetParent(tower.transform);
                _rangeIndicatorInstance.SetRadius(tower.Data.Range);
                _rangeIndicatorInstance.Show();
            }
        }

        private void HideSelectionIndicator()
        {
            if (_selectionIndicatorInstance != null)
            {
                _selectionIndicatorInstance.transform.SetParent(null);
                _selectionIndicatorInstance.SetActive(false);
            }
        }

        private void HideRangeIndicator()
        {
            if (_rangeIndicatorInstance != null)
            {
                _rangeIndicatorInstance.transform.SetParent(null);
                _rangeIndicatorInstance.Hide();
            }
        }

        #endregion
    }
}
