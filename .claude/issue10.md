## Context

Enemies need a valid navigation mesh to pathfind from spawn to exit. This issue sets up NavMesh baking on the test level, configures NavMesh areas for the path, and ensures enemies can navigate correctly. This is required before enemies can move in-game.

**Builds upon:** Issues 4, 9 (Test Level, Enemy Prefab)

## Detailed Implementation Instructions

### NavMesh Surface Setup

1. **Install AI Navigation Package** (if not already):
   - Window > Package Manager
   - Search "AI Navigation"
   - Install latest version

2. **Add NavMesh Surface to Ground:**
   - Select the Ground plane
   - Add Component > NavMeshSurface
   - Configure settings:
     - Agent Type: Humanoid (default)
     - Collect Objects: Current Object Hierarchy or All
     - Include Layers: Ground layer only
     - Use Geometry: Render Meshes

### NavMesh Agent Settings

Create custom NavMesh Agent type for enemies:

1. Window > AI > Navigation
2. Select "Agents" tab
3. Add new agent type "Enemy":
   - Name: Enemy
   - Radius: 0.4
   - Height: 1.5
   - Step Height: 0.4
   - Max Slope: 45

### Path Walkable Area

Create a walkable path area:

1. Create empty GameObject "NavMeshPath"
2. Add NavMeshModifierVolume component:
   - Size: covers the enemy path width
   - Area Type: Walkable
3. Position and scale to cover the enemy walking path

Alternatively, use ProBuilder or primitive cubes to create a clear walking surface.

### Blocking Towers from Path (Optional NavMesh Obstacle)

For dynamic path blocking (if towers block paths):

```csharp
// Add to Tower.cs or create TowerNavMeshObstacle.cs
using UnityEngine;
using UnityEngine.AI;

namespace TowerDefense.Towers
{
    [RequireComponent(typeof(NavMeshObstacle))]
    public class TowerNavMeshObstacle : MonoBehaviour
    {
        private NavMeshObstacle _obstacle;

        private void Awake()
        {
            _obstacle = GetComponent<NavMeshObstacle>();
            _obstacle.carving = true;
            _obstacle.carveOnlyStationary = true;
        }
    }
}
```

Note: For this game, towers should NOT block the path (path is pre-defined). This is included for reference only.

### NavMesh Baking Script (Editor Utility)

Create `NavMeshBaker.cs` in `_Project/Scripts/Editor/`:

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Unity.AI.Navigation;

namespace TowerDefense.Editor
{
    public static class NavMeshBaker
    {
        [MenuItem("TD/Bake NavMesh")]
        public static void BakeNavMesh()
        {
            NavMeshSurface[] surfaces = Object.FindObjectsOfType<NavMeshSurface>();
            foreach (var surface in surfaces)
            {
                surface.BuildNavMesh();
            }
            Debug.Log($"Baked {surfaces.Length} NavMesh surface(s)");
        }

        [MenuItem("TD/Clear NavMesh")]
        public static void ClearNavMesh()
        {
            NavMeshSurface[] surfaces = Object.FindObjectsOfType<NavMeshSurface>();
            foreach (var surface in surfaces)
            {
                surface.RemoveData();
            }
            Debug.Log($"Cleared {surfaces.Length} NavMesh surface(s)");
        }
    }
}
#endif
```

### Level Setup for Navigation

Recommended level structure for clear navigation:

```
--- ENVIRONMENT ---
|-- Ground (NavMeshSurface)
|-- PathSurface (Plane/ProBuilder mesh for enemy walking)
|   +-- Material: M_Path
|-- SpawnPoint
|-- ExitPoint
+-- Obstacles (optional decorative objects)
```

**Path Surface:**
1. Create a plane or ProBuilder mesh
2. Position slightly above ground (Y = 0.01)
3. Scale/shape to match enemy path
4. This provides a clear walking surface

### NavMesh Visualization

To see the NavMesh in Scene view:
1. Window > AI > Navigation
2. Click "Show NavMesh" in Scene view toolbar
3. Blue overlay shows walkable areas

### Enemy NavMesh Configuration

Update BasicEnemy prefab:
1. Select BasicEnemy prefab
2. NavMeshAgent component:
   - Agent Type: Enemy (the custom one we created)
   - Base Offset: 0
   - Speed: 3 (will be overridden)
   - Auto Traverse Off Mesh Link: true
   - Auto Repath: true
   - Area Mask: Walkable

### Path Validation Component

Create `PathValidator.cs` to verify path exists:

```csharp
using UnityEngine;
using UnityEngine.AI;
using TowerDefense.Core;

namespace TowerDefense.Enemies
{
    public class PathValidator : MonoBehaviour
    {
        public static bool IsPathValid()
        {
            if (SpawnPoint.Instance == null || ExitPoint.Instance == null)
            {
                Debug.LogError("SpawnPoint or ExitPoint not found!");
                return false;
            }

            NavMeshPath path = new NavMeshPath();
            Vector3 start = SpawnPoint.Instance.transform.position;
            Vector3 end = ExitPoint.Instance.transform.position;

            if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
            {
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    Debug.Log("Path is valid and complete!");
                    return true;
                }
                else if (path.status == NavMeshPathStatus.PathPartial)
                {
                    Debug.LogWarning("Path is partial - enemies may not reach exit!");
                    return false;
                }
            }

            Debug.LogError("No path found from spawn to exit!");
            return false;
        }

        [ContextMenu("Validate Path")]
        public void ValidatePath()
        {
            IsPathValid();
        }

        private void OnDrawGizmos()
        {
            if (SpawnPoint.Instance == null || ExitPoint.Instance == null) return;

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(
                SpawnPoint.Instance.transform.position,
                ExitPoint.Instance.transform.position,
                NavMesh.AllAreas, path))
            {
                Gizmos.color = path.status == NavMeshPathStatus.PathComplete ? Color.green : Color.red;
                for (int i = 0; i < path.corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
                }
            }
        }
    }
}
```

### Scene Setup

1. Add NavMeshSurface to Ground object
2. Configure Ground layer in NavMeshSurface Include Layers
3. Bake NavMesh (TD > Bake NavMesh or button in NavMeshSurface inspector)
4. Add PathValidator to --- MANAGEMENT --- for debugging
5. Verify blue NavMesh overlay covers path from spawn to exit

## Testing and Acceptance Criteria

### Manual Test Steps

1. Open Main.unity scene
2. Select Ground, verify NavMeshSurface component present
3. Click "Bake" on NavMeshSurface or use TD > Bake NavMesh
4. Enable NavMesh visualization in Scene view
5. Verify blue overlay exists from spawn area to exit area
6. Right-click PathValidator > Validate Path
7. Verify console shows "Path is valid and complete!"
8. Place BasicEnemy at spawn point manually
9. Enter Play mode
10. Verify enemy moves along NavMesh toward exit
11. Verify enemy reaches exit point

### Edge Cases

- [ ] Rebake NavMesh after modifying level geometry
- [ ] Path validator shows red gizmo if path is invalid
- [ ] Enemy stops if no valid path exists

### Done When

- [ ] NavMeshSurface added to ground/level
- [ ] Custom "Enemy" agent type created
- [ ] NavMesh baked successfully (blue overlay visible)
- [ ] PathValidator confirms path from spawn to exit
- [ ] Enemy NavMeshAgent uses correct agent type
- [ ] Enemy navigates from spawn to exit in play mode
- [ ] Editor menu items for baking/clearing NavMesh
- [ ] Path visualized with gizmos in Scene view

## Assets and Resources

- Unity AI Navigation package
- No external assets required

## Dependencies

- Issue 4: Test Level (spawn/exit points, path layout)
- Issue 9: Enemy Prefab (NavMeshAgent configuration)

## Notes

- NavMesh should be rebaked whenever level geometry changes
- Consider using NavMesh Links for jumps/teleports in future levels
- Flying enemies will bypass NavMesh entirely
- The path surface ensures consistent navigation even with decorative ground variations
- Auto-repath handles dynamic obstacle changes (if implemented)
