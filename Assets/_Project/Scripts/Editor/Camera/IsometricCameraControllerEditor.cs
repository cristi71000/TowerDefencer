#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TowerDefense.Camera;

namespace TowerDefense.Editor
{
    /// <summary>
    /// Custom editor for IsometricCameraController that draws camera bounds in the Scene view.
    /// </summary>
    [CustomEditor(typeof(IsometricCameraController))]
    public class IsometricCameraControllerEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            var controller = (IsometricCameraController)target;

            if (!controller.UseBounds) return;

            Vector2 boundsMin = controller.BoundsMin;
            Vector2 boundsMax = controller.BoundsMax;

            Handles.color = Color.yellow;

            Vector3[] corners = new Vector3[]
            {
                new Vector3(boundsMin.x, 0, boundsMin.y),
                new Vector3(boundsMax.x, 0, boundsMin.y),
                new Vector3(boundsMax.x, 0, boundsMax.y),
                new Vector3(boundsMin.x, 0, boundsMax.y),
                new Vector3(boundsMin.x, 0, boundsMin.y)
            };

            Handles.DrawPolyLine(corners);

            // Draw corner handles for visual clarity
            float handleSize = HandleUtility.GetHandleSize(corners[0]) * 0.1f;
            for (int i = 0; i < 4; i++)
            {
                Handles.DrawSolidDisc(corners[i], Vector3.up, handleSize);
            }
        }
    }
}
#endif
