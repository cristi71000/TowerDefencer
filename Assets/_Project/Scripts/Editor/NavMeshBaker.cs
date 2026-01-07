using UnityEditor;
using UnityEngine;
using Unity.AI.Navigation;

namespace TowerDefense.Editor
{
    /// <summary>
    /// Editor utility for baking and managing NavMesh in the Tower Defense project.
    ///
    /// SETUP INSTRUCTIONS:
    /// 1. Add a NavMeshSurface component to your Ground object in the scene
    /// 2. Configure the NavMeshSurface settings:
    ///    - Agent Type: Humanoid (or custom agent for enemies)
    ///    - Collect Objects: Volume or Children (depending on level structure)
    ///    - Include Layers: Ground, Path
    ///    - Use Geometry: Render Meshes
    /// 3. Use TD/Bake NavMesh menu item to bake all surfaces in the scene
    /// </summary>
    public static class NavMeshBaker
    {
        /// <summary>
        /// Bakes all NavMeshSurface components found in the current scene.
        /// </summary>
        [MenuItem("TD/Bake NavMesh", priority = 100)]
        public static void BakeNavMesh()
        {
            NavMeshSurface[] surfaces = Object.FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);

            if (surfaces == null || surfaces.Length == 0)
            {
                UnityEngine.Debug.LogWarning("[NavMeshBaker] No NavMeshSurface components found in the scene. " +
                    "Please add a NavMeshSurface component to your Ground object.");
                EditorUtility.DisplayDialog(
                    "NavMesh Baker",
                    "No NavMeshSurface components found in the scene.\n\n" +
                    "Please add a NavMeshSurface component to your Ground object first.",
                    "OK");
                return;
            }

            int bakedCount = 0;
            foreach (NavMeshSurface surface in surfaces)
            {
                UnityEngine.Debug.Log($"[NavMeshBaker] Baking NavMesh for: {surface.gameObject.name}");
                surface.BuildNavMesh();
                bakedCount++;
                EditorUtility.SetDirty(surface);
            }

            UnityEngine.Debug.Log($"[NavMeshBaker] Successfully baked {bakedCount} NavMesh surface(s).");
            EditorUtility.DisplayDialog(
                "NavMesh Baker",
                $"Successfully baked {bakedCount} NavMesh surface(s).",
                "OK");
        }

        /// <summary>
        /// Clears all NavMesh data from NavMeshSurface components in the current scene.
        /// </summary>
        [MenuItem("TD/Clear NavMesh", priority = 101)]
        public static void ClearNavMesh()
        {
            NavMeshSurface[] surfaces = Object.FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);

            if (surfaces == null || surfaces.Length == 0)
            {
                UnityEngine.Debug.LogWarning("[NavMeshBaker] No NavMeshSurface components found in the scene.");
                EditorUtility.DisplayDialog(
                    "NavMesh Baker",
                    "No NavMeshSurface components found in the scene.",
                    "OK");
                return;
            }

            int clearedCount = 0;
            foreach (NavMeshSurface surface in surfaces)
            {
                UnityEngine.Debug.Log($"[NavMeshBaker] Clearing NavMesh for: {surface.gameObject.name}");
                surface.RemoveData();
                clearedCount++;
                EditorUtility.SetDirty(surface);
            }

            UnityEngine.Debug.Log($"[NavMeshBaker] Successfully cleared {clearedCount} NavMesh surface(s).");
            EditorUtility.DisplayDialog(
                "NavMesh Baker",
                $"Successfully cleared {clearedCount} NavMesh surface(s).",
                "OK");
        }

        /// <summary>
        /// Validates that NavMesh is properly set up in the scene.
        /// </summary>
        [MenuItem("TD/Validate NavMesh Setup", priority = 102)]
        public static void ValidateNavMeshSetup()
        {
            NavMeshSurface[] surfaces = Object.FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);
            bool hasIssues = false;
            string report = "NavMesh Setup Validation Report:\n\n";

            // Check for NavMeshSurface components
            if (surfaces == null || surfaces.Length == 0)
            {
                report += "- WARNING: No NavMeshSurface components found.\n";
                report += "  Add a NavMeshSurface to your Ground object.\n\n";
                hasIssues = true;
            }
            else
            {
                report += $"- Found {surfaces.Length} NavMeshSurface component(s).\n";
                foreach (NavMeshSurface surface in surfaces)
                {
                    report += $"  - {surface.gameObject.name}\n";
                }
                report += "\n";
            }

            // Check for SpawnPoint
            var spawnPoints = Object.FindObjectsByType<TowerDefense.Core.SpawnPoint>(FindObjectsSortMode.None);
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                report += "- WARNING: No SpawnPoint found in scene.\n";
                hasIssues = true;
            }
            else
            {
                report += $"- SpawnPoint found: {spawnPoints[0].gameObject.name}\n";
            }

            // Check for ExitPoint
            var exitPoints = Object.FindObjectsByType<TowerDefense.Core.ExitPoint>(FindObjectsSortMode.None);
            if (exitPoints == null || exitPoints.Length == 0)
            {
                report += "- WARNING: No ExitPoint found in scene.\n";
                hasIssues = true;
            }
            else
            {
                report += $"- ExitPoint found: {exitPoints[0].gameObject.name}\n";
            }

            // Check for PathValidator
            var pathValidators = Object.FindObjectsByType<TowerDefense.Enemies.PathValidator>(FindObjectsSortMode.None);
            if (pathValidators == null || pathValidators.Length == 0)
            {
                report += "\n- INFO: No PathValidator found. Consider adding one for path visualization.\n";
            }
            else
            {
                report += $"\n- PathValidator found: {pathValidators[0].gameObject.name}\n";
            }

            if (hasIssues)
            {
                report += "\n--- Setup incomplete. Please address the warnings above. ---";
                UnityEngine.Debug.LogWarning("[NavMeshBaker] " + report);
            }
            else
            {
                report += "\n--- All checks passed! ---";
                UnityEngine.Debug.Log("[NavMeshBaker] " + report);
            }

            EditorUtility.DisplayDialog("NavMesh Setup Validation", report, "OK");
        }
    }
}
