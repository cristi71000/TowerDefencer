using UnityEngine;
using TowerDefense.Towers;

namespace TowerDefense.Debug
{
    /// <summary>
    /// Debug utility for testing tower placement.
    /// Press number keys (1-3) to select towers for placement.
    /// </summary>
    public class TowerSelector : MonoBehaviour
    {
        [SerializeField] private TowerData[] _availableTowers;
        [SerializeField] private readonly KeyCode[] _towerKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3 };

        private void Update()
        {
            if (TowerPlacementManager.Instance == null) return;

            for (int i = 0; i < _availableTowers.Length && i < _towerKeys.Length; i++)
            {
                if (_availableTowers[i] == null) continue;

                if (Input.GetKeyDown(_towerKeys[i]))
                {
                    TowerPlacementManager.Instance.StartPlacement(_availableTowers[i]);
                    UnityEngine.Debug.Log($"[TowerSelector] Selected tower: {_availableTowers[i].TowerName} (Press {_towerKeys[i]})");
                }
            }
        }
    }
}
