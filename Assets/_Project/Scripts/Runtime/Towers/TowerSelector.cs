using UnityEngine;
using TowerDefense.Towers;

namespace TowerDefense.Testing
{
    /// <summary>
    /// Temporary test script to trigger tower placement via keyboard shortcuts.
    /// Press 1, 2, 3 to select different tower types for placement.
    /// Uses legacy Input for simplicity in debug scenarios.
    /// </summary>
    public class TowerSelector : MonoBehaviour
    {
        [SerializeField] private TowerData[] _availableTowers;
        [SerializeField] private KeyCode[] _towerKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

        private void Update()
        {
            if (TowerPlacementManager.Instance == null) return;

            for (int i = 0; i < _availableTowers.Length && i < _towerKeys.Length; i++)
            {
                if (_availableTowers[i] != null && Input.GetKeyDown(_towerKeys[i]))
                {
                    TowerPlacementManager.Instance.StartPlacement(_availableTowers[i]);
                }
            }
        }
    }
}
