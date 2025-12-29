using UnityEngine;

namespace TowerDefense.Core
{
    public class GridVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager _gridManager;

        [Header("Cell Prefab")]
        [SerializeField] private GameObject _cellPrefab;

        [Header("Materials")]
        [SerializeField] private Material _emptyMaterial;
        [SerializeField] private Material _blockedMaterial;
        [SerializeField] private Material _occupiedMaterial;
        [SerializeField] private Material _validHoverMaterial;
        [SerializeField] private Material _invalidHoverMaterial;

        [Header("Settings")]
        [SerializeField] private bool _showGridOnStart = false;
        [SerializeField] private float _cellHeight = 0.05f;

        private GameObject[,] _cellVisuals;
        private Renderer[,] _cellRenderers;
        private bool _isVisible = false;
        private Vector2Int _currentHoverCell = new Vector2Int(-1, -1);

        public bool IsVisible => _isVisible;

        private void Start()
        {
            if (_gridManager == null)
            {
                _gridManager = GridManager.Instance;
            }

            if (_gridManager == null)
            {
                Debug.LogError("[GridVisualizer] No GridManager found!");
                return;
            }

            if (_cellPrefab == null)
            {
                Debug.LogError("[GridVisualizer] Cell prefab is not assigned!");
                return;
            }

            CreateCellVisuals();
            SetGridVisible(_showGridOnStart);
        }

        private void CreateCellVisuals()
        {
            int width = _gridManager.Width;
            int height = _gridManager.Height;
            _cellVisuals = new GameObject[width, height];
            _cellRenderers = new Renderer[width, height];

            Transform container = new GameObject("GridVisuals").transform;
            container.SetParent(transform);

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    Vector2Int gridPos = new Vector2Int(x, z);
                    GridCell cell = _gridManager.GetCell(gridPos);

                    GameObject visual = Instantiate(_cellPrefab, container);
                    visual.transform.position = cell.WorldPosition + Vector3.up * _cellHeight;
                    visual.transform.localScale = new Vector3(
                        _gridManager.CellSize * 0.95f,
                        0.1f,
                        _gridManager.CellSize * 0.95f
                    );
                    visual.name = $"Cell_{x}_{z}";

                    _cellVisuals[x, z] = visual;
                    _cellRenderers[x, z] = visual.GetComponent<Renderer>();
                    UpdateCellVisual(gridPos);
                }
            }
        }

        public void UpdateCellVisual(Vector2Int gridPos)
        {
            if (_gridManager == null || !_gridManager.IsValidGridPosition(gridPos)) return;
            if (_cellRenderers == null) return;

            GridCell cell = _gridManager.GetCell(gridPos);
            Renderer renderer = _cellRenderers[gridPos.x, gridPos.y];

            if (renderer == null) return;

            Material mat = cell.Type switch
            {
                CellType.Empty => _emptyMaterial,
                CellType.Blocked => _blockedMaterial,
                CellType.Occupied => _occupiedMaterial,
                _ => _emptyMaterial
            };

            renderer.sharedMaterial = mat;
        }

        public void SetGridVisible(bool visible)
        {
            _isVisible = visible;
            if (_cellVisuals == null) return;

            foreach (var visual in _cellVisuals)
            {
                if (visual != null)
                    visual.SetActive(visible);
            }
        }

        public void ToggleGridVisible()
        {
            SetGridVisible(!_isVisible);
        }

        public void SetHoverCell(Vector2Int gridPos, bool isValid)
        {
            // Reset previous hover cell
            if (_currentHoverCell.x >= 0 && _gridManager.IsValidGridPosition(_currentHoverCell))
            {
                UpdateCellVisual(_currentHoverCell);
            }

            _currentHoverCell = gridPos;

            // Set new hover cell
            if (_gridManager.IsValidGridPosition(gridPos) && _cellRenderers != null)
            {
                Renderer renderer = _cellRenderers[gridPos.x, gridPos.y];
                if (renderer != null)
                {
                    renderer.sharedMaterial = isValid ? _validHoverMaterial : _invalidHoverMaterial;
                }
            }
        }

        public void ClearHover()
        {
            if (_currentHoverCell.x >= 0 && _gridManager != null && _gridManager.IsValidGridPosition(_currentHoverCell))
            {
                UpdateCellVisual(_currentHoverCell);
            }
            _currentHoverCell = new Vector2Int(-1, -1);
        }

        public void RefreshAllVisuals()
        {
            if (_gridManager == null) return;

            for (int x = 0; x < _gridManager.Width; x++)
            {
                for (int z = 0; z < _gridManager.Height; z++)
                {
                    UpdateCellVisual(new Vector2Int(x, z));
                }
            }
        }
    }
}
