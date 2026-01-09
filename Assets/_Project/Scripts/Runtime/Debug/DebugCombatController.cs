using UnityEngine;
using TowerDefense.Enemies;
using TowerDefense.Towers;
using TowerDefense.Core;

namespace TowerDefense.Debug
{
    /// <summary>
    /// Debug controller for testing combat systems.
    /// Provides keyboard shortcuts to spawn towers, enemies, and trigger combat events.
    /// </summary>
    public class DebugCombatController : MonoBehaviour
    {
        [Header("Test Data")]
        [SerializeField] private EnemyData _testEnemyData;
        [SerializeField] private TowerData _testTowerData;

        [Header("Spawn Settings")]
        [SerializeField] private int _waveSize = 5;
        [SerializeField] private float _spawnInterval = 0.5f;
        [SerializeField] private Vector2Int _towerGridPosition = new Vector2Int(10, 10);

        [Header("Key Bindings")]
        [SerializeField] private KeyCode _spawnEnemyKey = KeyCode.E;
        [SerializeField] private KeyCode _spawnWaveKey = KeyCode.W;
        [SerializeField] private KeyCode _killAllKey = KeyCode.K;
        [SerializeField] private KeyCode _spawnTowerKey = KeyCode.T;
        [SerializeField] private KeyCode _toggleDebugKey = KeyCode.F1;

        [Header("Debug Display")]
        [SerializeField] private bool _showDebugInfo = true;

        private bool _isSpawningWave;
        private int _enemiesSpawnedThisWave;
        private float _nextSpawnTime;
        private GUIStyle _headerStyle;

        private void Update()
        {
            HandleInput();
            UpdateWaveSpawning();
        }

        private void HandleInput()
        {
            // Toggle debug info display
            if (Input.GetKeyDown(_toggleDebugKey))
            {
                _showDebugInfo = !_showDebugInfo;
                UnityEngine.Debug.Log($"[DebugCombatController] Debug info display: {(_showDebugInfo ? "ON" : "OFF")}");
            }

            // Spawn single enemy
            if (Input.GetKeyDown(_spawnEnemyKey))
            {
                SpawnTestEnemy();
            }

            // Spawn wave of enemies
            if (Input.GetKeyDown(_spawnWaveKey))
            {
                StartWaveSpawn();
            }

            // Kill all enemies
            if (Input.GetKeyDown(_killAllKey))
            {
                KillAllEnemies();
            }

            // Spawn tower directly at configured grid position
            if (Input.GetKeyDown(_spawnTowerKey))
            {
                SpawnTestTower();
            }
        }

        private void UpdateWaveSpawning()
        {
            if (!_isSpawningWave) return;

            if (Time.time >= _nextSpawnTime && _enemiesSpawnedThisWave < _waveSize)
            {
                SpawnTestEnemy();
                _enemiesSpawnedThisWave++;
                _nextSpawnTime = Time.time + _spawnInterval;

                if (_enemiesSpawnedThisWave >= _waveSize)
                {
                    _isSpawningWave = false;
                    UnityEngine.Debug.Log($"[DebugCombatController] Wave spawn complete. Spawned {_waveSize} enemies.");
                }
            }
        }

        /// <summary>
        /// Spawns a single test enemy at the spawn point.
        /// </summary>
        public void SpawnTestEnemy()
        {
            if (_testEnemyData == null)
            {
                UnityEngine.Debug.LogWarning("[DebugCombatController] No test enemy data assigned!");
                return;
            }

            if (EnemySpawner.Instance == null)
            {
                UnityEngine.Debug.LogWarning("[DebugCombatController] EnemySpawner.Instance is null!");
                return;
            }

            Enemy enemy = EnemySpawner.Instance.SpawnEnemy(_testEnemyData);
            if (enemy != null)
            {
                UnityEngine.Debug.Log($"[DebugCombatController] Spawned enemy: {_testEnemyData.EnemyName}");
            }
        }

        /// <summary>
        /// Starts spawning a wave of enemies over time.
        /// </summary>
        public void StartWaveSpawn()
        {
            if (_testEnemyData == null)
            {
                UnityEngine.Debug.LogWarning("[DebugCombatController] No test enemy data assigned!");
                return;
            }

            if (_isSpawningWave)
            {
                UnityEngine.Debug.Log("[DebugCombatController] Wave already in progress.");
                return;
            }

            _isSpawningWave = true;
            _enemiesSpawnedThisWave = 0;
            _nextSpawnTime = Time.time;
            UnityEngine.Debug.Log($"[DebugCombatController] Starting wave spawn of {_waveSize} enemies.");
        }

        /// <summary>
        /// Kills all active enemies by dealing lethal damage.
        /// Note: Uses FindObjectsByType which is intentionally expensive for a debug tool.
        /// This approach is acceptable here since it's only used for manual debugging and testing.
        /// </summary>
        public void KillAllEnemies()
        {
            // Note: FindObjectsByType is slow with many enemies, but acceptable for debug purposes
            Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            int killedCount = 0;

            foreach (Enemy enemy in enemies)
            {
                if (enemy != null && enemy.IsValidTarget)
                {
                    // Deal massive damage to kill the enemy
                    enemy.TakeDamage(new DamageInfo(999999f, null, enemy.transform.position, false, DamageType.True));
                    killedCount++;
                }
            }

            UnityEngine.Debug.Log($"[DebugCombatController] Killed {killedCount} enemies.");
        }

        /// <summary>
        /// Spawns a test tower directly at the configured grid position.
        /// </summary>
        public void SpawnTestTower()
        {
            if (_testTowerData == null)
            {
                UnityEngine.Debug.LogWarning("[DebugCombatController] No test tower data assigned!");
                return;
            }

            if (GridManager.Instance == null)
            {
                UnityEngine.Debug.LogWarning("[DebugCombatController] GridManager.Instance is null!");
                return;
            }

            if (!GridManager.Instance.CanPlaceAt(_towerGridPosition))
            {
                UnityEngine.Debug.LogWarning($"[DebugCombatController] Cannot place tower at grid position {_towerGridPosition}");
                return;
            }

            Vector3 worldPos = GridManager.Instance.GridToWorldPosition(_towerGridPosition);
            GameObject tower = Instantiate(_testTowerData.Prefab, worldPos, Quaternion.identity);

            Tower towerComponent = tower.GetComponent<Tower>();
            if (towerComponent != null)
            {
                towerComponent.Initialize(_testTowerData, _towerGridPosition);
            }
            else
            {
                UnityEngine.Debug.LogError($"[DebugCombatController] Tower prefab does not have a Tower component!");
                Destroy(tower);
                return;
            }

            GridManager.Instance.TryOccupyCell(_towerGridPosition, tower);
            UnityEngine.Debug.Log($"[DebugCombatController] Spawned tower '{_testTowerData.TowerName}' at grid position {_towerGridPosition}");
        }

        private void OnGUI()
        {
            if (!_showDebugInfo) return;

            EnsureStyles();

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.BeginVertical("box");

            GUILayout.Label("<b>Combat Debug Controls</b>", _headerStyle);
            GUILayout.Label($"[{_spawnEnemyKey}] Spawn Enemy");
            GUILayout.Label($"[{_spawnWaveKey}] Spawn Wave ({_waveSize} enemies)");
            GUILayout.Label($"[{_killAllKey}] Kill All Enemies");
            GUILayout.Label($"[{_spawnTowerKey}] Spawn Tower at {_towerGridPosition}");
            GUILayout.Label($"[{_toggleDebugKey}] Toggle This Display");

            GUILayout.Space(10);

            // Show system status
            int activeEnemies = EnemySpawner.Instance != null ? EnemySpawner.Instance.ActiveEnemyCount : 0;
            GUILayout.Label($"Active Enemies: {activeEnemies}");

            if (_isSpawningWave)
            {
                GUILayout.Label($"Wave Progress: {_enemiesSpawnedThisWave}/{_waveSize}");
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    fontStyle = FontStyle.Bold,
                    fontSize = 12
                };
            }
        }
    }
}
