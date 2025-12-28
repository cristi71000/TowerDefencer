using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Bootstraps the game by initializing the GameManager on Start.
    /// Attach this to a GameObject in the scene to ensure proper game initialization.
    /// </summary>
    public class GameInitializer : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManager;

        private void Start()
        {
            if (_gameManager != null)
            {
                _gameManager.InitializeGame();
            }
            else
            {
                Debug.LogError("GameManager reference missing on GameInitializer!");
            }
        }
    }
}
