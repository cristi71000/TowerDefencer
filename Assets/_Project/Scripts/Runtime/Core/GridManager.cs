using UnityEngine;

namespace TowerDefense.Core
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Grid Settings")]
        [SerializeField] private int _gridWidth = 20;
        [SerializeField] private int _gridHeight = 20;
        [SerializeField] private float _cellSize = 2f;
        [SerializeField] private Vector3 _gridOrigin = new Vector3(-20, 0, -20);

        [Header("Blocked Area Detection")]
        [SerializeField] private LayerMask _blockedLayers;
        [SerializeField] private float _blockCheckRadius = 0.5f;

        private GridCell[,] _grid;

        public int Width => _gridWidth;
        public int Height => _gridHeight;
        public float CellSize => _cellSize;
        public Vector3 Origin => _gridOrigin;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializeGrid();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void InitializeGrid()
        {
            _grid = new GridCell[_gridWidth, _gridHeight];

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int z = 0; z < _gridHeight; z++)
                {
                    Vector2Int gridPos = new Vector2Int(x, z);
                    Vector3 worldPos = GridToWorldPosition(gridPos);
                    CellType cellType = DetermineCellType(worldPos);
                    _grid[x, z] = new GridCell(gridPos, worldPos, cellType);
                }
            }
        }

        private CellType DetermineCellType(Vector3 worldPosition)
        {
            // Check for obstacles using overlap sphere
            Collider[] hits = Physics.OverlapSphere(
                worldPosition + Vector3.up * 0.5f,
                _blockCheckRadius,
                _blockedLayers
            );

            return hits.Length > 0 ? CellType.Blocked : CellType.Empty;
        }

        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            return new Vector3(
                _gridOrigin.x + gridPosition.x * _cellSize + _cellSize / 2f,
                _gridOrigin.y,
                _gridOrigin.z + gridPosition.y * _cellSize + _cellSize / 2f
            );
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            int x = Mathf.FloorToInt((worldPosition.x - _gridOrigin.x) / _cellSize);
            int z = Mathf.FloorToInt((worldPosition.z - _gridOrigin.z) / _cellSize);
            return new Vector2Int(x, z);
        }

        public bool IsValidGridPosition(Vector2Int gridPosition)
        {
            return gridPosition.x >= 0 && gridPosition.x < _gridWidth &&
                   gridPosition.y >= 0 && gridPosition.y < _gridHeight;
        }

        public GridCell GetCell(Vector2Int gridPosition)
        {
            if (!IsValidGridPosition(gridPosition)) return null;
            return _grid[gridPosition.x, gridPosition.y];
        }

        public GridCell GetCellAtWorldPosition(Vector3 worldPosition)
        {
            Vector2Int gridPos = WorldToGridPosition(worldPosition);
            return GetCell(gridPos);
        }

        public bool CanPlaceAt(Vector2Int gridPosition)
        {
            GridCell cell = GetCell(gridPosition);
            return cell != null && cell.IsPlaceable;
        }

        public bool TryOccupyCell(Vector2Int gridPosition, GameObject occupant)
        {
            GridCell cell = GetCell(gridPosition);
            if (cell == null || !cell.IsPlaceable) return false;

            cell.Type = CellType.Occupied;
            cell.OccupyingObject = occupant;
            return true;
        }

        public void FreeCell(Vector2Int gridPosition)
        {
            GridCell cell = GetCell(gridPosition);
            if (cell == null) return;

            // Re-check if the cell should be blocked (preserves original state)
            cell.Type = DetermineCellType(cell.WorldPosition);
            cell.OccupyingObject = null;
        }

        public void RefreshBlockedCells()
        {
            // Re-scan for blocked areas (useful after level changes)
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int z = 0; z < _gridHeight; z++)
                {
                    if (_grid[x, z].Type != CellType.Occupied)
                    {
                        _grid[x, z].Type = DetermineCellType(_grid[x, z].WorldPosition);
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw grid bounds
            Gizmos.color = Color.cyan;
            Vector3 size = new Vector3(_gridWidth * _cellSize, 0.1f, _gridHeight * _cellSize);
            Vector3 center = _gridOrigin + size / 2f;
            Gizmos.DrawWireCube(center, size);

            // Draw grid lines
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            for (int x = 0; x <= _gridWidth; x++)
            {
                Vector3 start = _gridOrigin + new Vector3(x * _cellSize, 0, 0);
                Vector3 end = start + new Vector3(0, 0, _gridHeight * _cellSize);
                Gizmos.DrawLine(start, end);
            }
            for (int z = 0; z <= _gridHeight; z++)
            {
                Vector3 start = _gridOrigin + new Vector3(0, 0, z * _cellSize);
                Vector3 end = start + new Vector3(_gridWidth * _cellSize, 0, 0);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
