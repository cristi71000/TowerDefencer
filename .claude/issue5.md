## Context

Tower placement is a core mechanic in tower defense games. Before players can place towers, we need a grid system that divides the play area into discrete cells. This grid system will be used for placement validation, visual feedback, and ensuring towers snap to consistent positions. The grid should be visible during placement mode and clearly indicate valid/invalid cells.

**Builds upon:** Issue 4 (Test Level Blockout)

## Detailed Implementation Instructions

### Grid System Architecture

The grid system consists of:
1. **GridManager** - Manages grid state, cell data, and queries
2. **GridCell** - Data structure representing a single cell
3. **GridVisualizer** - Handles visual representation of the grid

### Grid Cell Data Structure

Create `GridCell.cs` in `_Project/Scripts/Runtime/Core/`:

```csharp
namespace TowerDefense.Core
{
    public enum CellType
    {
        Empty,      // Can place towers
        Blocked,    // Cannot place (path, obstacle)
        Occupied    // Has a tower placed
    }

    [System.Serializable]
    public class GridCell
    {
        public Vector2Int GridPosition;
        public Vector3 WorldPosition;
        public CellType Type;
        public GameObject OccupyingObject;

        public bool IsPlaceable => Type == CellType.Empty;

        public GridCell(Vector2Int gridPos, Vector3 worldPos, CellType type = CellType.Empty)
        {
            GridPosition = gridPos;
            WorldPosition = worldPos;
            Type = type;
            OccupyingObject = null;
        }
    }
}
```

### Grid Manager

Create `GridManager.cs` in `_Project/Scripts/Runtime/Core/`:

```csharp
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

            cell.Type = CellType.Empty;
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
```

### Grid Visualizer

Create `GridVisualizer.cs` in `_Project/Scripts/Runtime/Core/`:

```csharp
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
        private bool _isVisible = false;
        private Vector2Int _currentHoverCell = new Vector2Int(-1, -1);

        private void Start()
        {
            CreateCellVisuals();
            SetGridVisible(_showGridOnStart);
        }

        private void CreateCellVisuals()
        {
            int width = _gridManager.Width;
            int height = _gridManager.Height;
            _cellVisuals = new GameObject[width, height];

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
                    UpdateCellVisual(gridPos);
                }
            }
        }

        public void UpdateCellVisual(Vector2Int gridPos)
        {
            if (!_gridManager.IsValidGridPosition(gridPos)) return;

            GridCell cell = _gridManager.GetCell(gridPos);
            GameObject visual = _cellVisuals[gridPos.x, gridPos.y];
            Renderer renderer = visual.GetComponent<Renderer>();

            Material mat = cell.Type switch
            {
                CellType.Empty => _emptyMaterial,
                CellType.Blocked => _blockedMaterial,
                CellType.Occupied => _occupiedMaterial,
                _ => _emptyMaterial
            };

            renderer.material = mat;
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

        public void SetHoverCell(Vector2Int gridPos, bool isValid)
        {
            // Reset previous hover cell
            if (_currentHoverCell.x >= 0 && _gridManager.IsValidGridPosition(_currentHoverCell))
            {
                UpdateCellVisual(_currentHoverCell);
            }

            _currentHoverCell = gridPos;

            // Set new hover cell
            if (_gridManager.IsValidGridPosition(gridPos))
            {
                Renderer renderer = _cellVisuals[gridPos.x, gridPos.y].GetComponent<Renderer>();
                renderer.material = isValid ? _validHoverMaterial : _invalidHoverMaterial;
            }
        }

        public void ClearHover()
        {
            if (_currentHoverCell.x >= 0 && _gridManager.IsValidGridPosition(_currentHoverCell))
            {
                UpdateCellVisual(_currentHoverCell);
            }
            _currentHoverCell = new Vector2Int(-1, -1);
        }

        public void RefreshAllVisuals()
        {
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
```

### Cell Prefab Setup

Create a simple cell prefab:

1. Create a Cube primitive
2. Scale to (1, 0.1, 1)
3. Remove BoxCollider (visuals only)
4. Save as `_Project/Prefabs/Level/GridCell.prefab`

### Materials for Grid Cells

Create the following materials in `_Project/Art/Materials/Grid/`:

- `M_Cell_Empty.mat` - Semi-transparent light blue, RGBA(100, 200, 255, 100)
- `M_Cell_Blocked.mat` - Semi-transparent red, RGBA(255, 100, 100, 100)
- `M_Cell_Occupied.mat` - Semi-transparent yellow, RGBA(255, 255, 100, 100)
- `M_Cell_ValidHover.mat` - Solid green, RGBA(100, 255, 100, 200)
- `M_Cell_InvalidHover.mat` - Solid red, RGBA(255, 100, 100, 200)

For transparency, use URP Lit shader with Surface Type: Transparent

### Scene Setup

1. Create GridManager GameObject under --- MANAGEMENT ---
2. Add GridManager component with settings:
   - Grid Width: 20
   - Grid Height: 20
   - Cell Size: 2
   - Grid Origin: (-20, 0, -20)
   - Blocked Layers: include path/obstacle layers
3. Add GridVisualizer as child of GridManager
4. Wire up all material references

### Blocked Area Setup

Add colliders to path markers to mark them as blocked:
1. Add BoxCollider to each PathNode
2. Place PathNodes on a layer included in GridManager's Blocked Layers
3. Alternatively, create invisible blocking volumes along the path

## Testing and Acceptance Criteria

### Manual Test Steps

1. Enter Play mode
2. Select GridManager - verify gizmos show grid bounds and lines
3. Verify grid cells generated without errors
4. Toggle grid visibility via inspector or public method
5. Verify path areas show as blocked (red cells)
6. Verify empty areas show as placeable (blue cells)
7. Call SetHoverCell with valid position - verify green highlight
8. Call SetHoverCell with blocked position - verify red highlight
9. Test WorldToGridPosition and GridToWorldPosition conversions

### Done When

- [ ] GridManager creates NxM grid of cells
- [ ] Cells correctly detect blocked areas (path)
- [ ] GridToWorldPosition returns center of cell
- [ ] WorldToGridPosition correctly maps world coords to grid
- [ ] IsValidGridPosition validates bounds correctly
- [ ] CanPlaceAt returns true for empty cells, false otherwise
- [ ] TryOccupyCell marks cell as occupied
- [ ] FreeCell returns cell to empty state
- [ ] GridVisualizer creates visual for each cell
- [ ] Grid can be toggled visible/invisible
- [ ] Hover states display correct materials
- [ ] Grid gizmos visible in Scene view when selected
- [ ] No console errors during play mode

## Assets and Resources

- Unity primitive Cube for cell visual
- URP Lit shader with transparency for materials
- Colors specified above for material creation

## Dependencies

- Issue 4: Test Level Blockout (path markers for blocked detection)

## Notes

- Cell size of 2 units works well for typical tower sizes
- Grid origin should be positioned so grid covers the playable area
- Consider performance for very large grids (object pooling cell visuals)
- The blocked layer detection runs once at start; call RefreshBlockedCells() if level changes dynamically
- Semi-transparent materials require proper render queue setup for correct layering
