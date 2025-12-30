using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Handles keyboard shortcuts for tower context actions like selling
    /// and cycling targeting priority.
    /// </summary>
    public class TowerContextActions : MonoBehaviour
    {
        [Header("Key Bindings")]
        [SerializeField] private Key _sellKey = Key.X;
        [SerializeField] private Key _cyclePriorityKey = Key.Tab;

        private void Update()
        {
            if (Keyboard.current == null) return;

            // Check for sell key (X)
            if (Keyboard.current[_sellKey].wasPressedThisFrame)
            {
                OnSellPressed();
            }

            // Check for cycle priority key (Tab)
            if (Keyboard.current[_cyclePriorityKey].wasPressedThisFrame)
            {
                OnCyclePriorityPressed();
            }
        }

        private void OnSellPressed()
        {
            // Skip if in placement mode
            if (TowerPlacementManager.Instance != null && TowerPlacementManager.Instance.IsInPlacementMode)
            {
                return;
            }

            if (TowerSelectionManager.Instance != null)
            {
                TowerSelectionManager.Instance.SellSelectedTower();
            }
        }

        private void OnCyclePriorityPressed()
        {
            // Skip if in placement mode
            if (TowerPlacementManager.Instance != null && TowerPlacementManager.Instance.IsInPlacementMode)
            {
                return;
            }

            if (TowerSelectionManager.Instance != null)
            {
                TowerSelectionManager.Instance.CycleTargetingPriority();
            }
        }
    }
}
