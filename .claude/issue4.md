## Context

Before implementing gameplay systems, we need a test environment where we can visualize and test all features. This issue creates a simple level blockout with a ground plane, visual path markers showing where enemies will walk, spawn point, and exit point. This serves as the foundation for NavMesh pathfinding (Issue 9) and provides immediate visual feedback during development.

**Builds upon:** Issues 1-3 (Project Setup, Game Manager, Camera)

## Detailed Implementation Instructions

### Level Layout Requirements

Create a simple test level with:
- **Ground Plane:** 40x40 units
- **Enemy Path:** S-curve or zigzag from spawn to exit
- **Spawn Point:** Where enemies enter
- **Exit Point:** Where enemies escape (player loses lives)
- **Buildable Areas:** Spaces alongside the path for tower placement

### Ground Plane Setup

1. Create a plane GameObject named "Ground"
2. Scale to 40x40 units (Plane default is 10x10, so scale (4, 1, 4))
3. Position at origin (0, 0, 0)
4. Assign to Layer 6 (Ground)
5. Create a simple ground material:
   - Create `Materials/M_Ground.mat` in `_Project/Art/Materials/`
   - Use URP Lit shader
   - Base color: Dark green or brown (game board appearance)

### Path Visualization

Create visual markers showing the enemy path:

**PathNode Prefab:**
1. Create empty GameObject named "PathNode"
2. Add child Cylinder (scale 0.5, 0.1, 0.5) - flat disc
3. Add child Sphere (scale 0.3) at Y=0.5 - marker ball
4. Create material `M_PathMarker.mat` with bright color (orange/yellow)
5. Save as prefab in `_Project/Prefabs/Level/PathNode.prefab`

**Path Layout in Scene:**
Create 8-12 PathNode instances forming an S-curve:
```
Example layout (top-down view, S = Spawn, E = Exit, * = PathNode):

     S
     |
     *---*
         |
     *---*
     |
     *---*
         |
         E
```

### Spawn Point Marker

Create a distinct spawn point indicator:

1. Create GameObject "SpawnPoint"
2. Add child Cube (scale 2, 0.1, 2) as base
3. Add child Cylinder pointing up (scale 0.5, 1, 0.5) as pillar
4. Create material `M_SpawnPoint.mat` - Green color
5. Add `SpawnPoint.cs` component (marker script):

```csharp
namespace TowerDefense.Core
{
    public class SpawnPoint : MonoBehaviour
    {
        public static SpawnPoint Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple SpawnPoints in scene!");
            }
            Instance = this;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 1f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
        }
    }
}
```

### Exit Point Marker

Create the exit/goal indicator:

1. Create GameObject "ExitPoint"
2. Add child Cube (scale 2, 0.1, 2) as base
3. Add child Cylinder pointing up (scale 0.5, 1, 0.5)
4. Create material `M_ExitPoint.mat` - Red color
5. Add `ExitPoint.cs` component:

```csharp
namespace TowerDefense.Core
{
    public class ExitPoint : MonoBehaviour
    {
        public static ExitPoint Instance { get; private set; }

        [SerializeField] private IntEventChannel _onEnemyReachedEnd;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple ExitPoints in scene!");
            }
            Instance = this;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                Debug.Log("Enemy reached exit!");
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }
    }
}
```

Add a BoxCollider trigger to ExitPoint (IsTrigger = true, size 2x2x2).

### Path Manager

Create `PathManager.cs` to manage waypoints:

```csharp
namespace TowerDefense.Core
{
    public class PathManager : MonoBehaviour
    {
        public static PathManager Instance { get; private set; }

        [SerializeField] private Transform[] _waypoints;

        public Transform[] Waypoints => _waypoints;
        public Transform SpawnPoint => _waypoints.Length > 0 ? _waypoints[0] : null;
        public Transform ExitPoint => _waypoints.Length > 0 ? _waypoints[_waypoints.Length - 1] : null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDrawGizmos()
        {
            if (_waypoints == null || _waypoints.Length < 2) return;

            Gizmos.color = Color.yellow;
            for (int i = 0; i < _waypoints.Length - 1; i++)
            {
                if (_waypoints[i] != null && _waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(_waypoints[i].position, _waypoints[i + 1].position);
                }
            }
        }
    }
}
```

### Scene Hierarchy Update

```
Main (Scene)
|-- --- MANAGEMENT ---
|   |-- GameManager
|   |-- GameInitializer
|   +-- PathManager
|-- --- CAMERAS ---
|   |-- CameraRig
|   |   +-- CameraTarget
|   |-- Main Camera
|   +-- IsometricVirtualCamera
|-- --- ENVIRONMENT ---
|   |-- Ground
|   |-- SpawnPoint
|   |-- ExitPoint
|   +-- Path
|       |-- PathNode_01
|       |-- PathNode_02
|       +-- ... (8-12 nodes)
|-- --- GAMEPLAY ---
|   |-- Towers
|   +-- Enemies
+-- --- UI ---
    +-- Canvas
```

### Camera Bounds Update

Update the `IsometricCameraController` bounds to match level size:
- Bounds Min: (-15, -15)
- Bounds Max: (15, 15)

### Tags Setup

Create the following tags in Project Settings > Tags and Layers:
- "Enemy" (for enemy detection at exit)
- "Tower" (for future use)
- "Waypoint" (for path nodes)

## Testing and Acceptance Criteria

### Manual Test Steps

1. Open Main.unity and view in Scene window
2. Verify ground plane is visible and properly colored
3. Verify spawn point is clearly marked (green)
4. Verify exit point is clearly marked (red)
5. Verify path nodes are visible and form clear path
6. Select PathManager and verify gizmo shows connected path
7. Enter Play mode:
   - Pan camera around entire level
   - Zoom in/out to see detail
   - Verify camera bounds keep view on level
8. Check SpawnPoint.Instance and ExitPoint.Instance are set (Debug.Log)

### Done When

- [ ] Ground plane is 40x40 units with appropriate material
- [ ] Spawn point visible with green material and gizmo
- [ ] Exit point visible with red material and gizmo
- [ ] 8-12 path nodes form clear S-curve or zigzag path
- [ ] PathManager shows connected line gizmos in editor
- [ ] All path waypoints assigned to PathManager
- [ ] Camera bounds adjusted to fit level
- [ ] Tags created: Enemy, Tower, Waypoint
- [ ] Singleton references accessible (SpawnPoint, ExitPoint, PathManager)
- [ ] Exit point has trigger collider configured
- [ ] Scene hierarchy follows specified structure

## Assets and Resources

- **ProBuilder** (optional) - Can use for more complex blockout geometry
- Use Unity primitives (Plane, Cube, Cylinder, Sphere) for all markers
- Create simple solid-color materials using URP Lit shader

### Material Colors (Suggested)
- Ground: RGB(60, 80, 60) - Dark muted green
- Path Markers: RGB(255, 180, 0) - Orange/Gold
- Spawn Point: RGB(50, 200, 50) - Bright green
- Exit Point: RGB(200, 50, 50) - Bright red

## Dependencies

- Issue 1: Project Setup (folder structure, materials location)
- Issue 2: Game Manager (event channels for enemy exit)
- Issue 3: Camera Controller (bounds configuration)

## Notes

- Path markers are visual guides for development; actual pathfinding will use NavMesh
- Consider adding height variation to path for visual interest (optional)
- The PathManager stores Transform references; for NavMesh, enemies will calculate path dynamically
- Keep path wide enough for multiple enemies to traverse without colliding
- This layout should provide good tower placement opportunities on both sides of the path
