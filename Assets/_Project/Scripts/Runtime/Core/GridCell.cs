using UnityEngine;

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
