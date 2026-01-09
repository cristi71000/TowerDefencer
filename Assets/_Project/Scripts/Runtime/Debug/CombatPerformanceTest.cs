using UnityEngine;
using TowerDefense.Enemies;
using TowerDefense.Towers;

namespace TowerDefense.Debug
{
    /// <summary>
    /// Monitors FPS and combat system performance.
    /// Displays active enemy/projectile counts and warns if performance drops below threshold.
    /// </summary>
    public class CombatPerformanceTest : MonoBehaviour
    {
        [Header("Performance Thresholds")]
        [SerializeField] private float _targetFPS = 60f;
        [SerializeField] private float _warningThreshold = 0.8f; // 80% of target FPS

        [Header("Display Settings")]
        [SerializeField] private bool _showPerformanceUI = true;
        [SerializeField] private KeyCode _toggleKey = KeyCode.F2;
        [SerializeField] private Color _goodColor = Color.green;
        [SerializeField] private Color _warningColor = Color.yellow;
        [SerializeField] private Color _badColor = Color.red;

        [Header("FPS Calculation")]
        [SerializeField] private float _fpsUpdateInterval = 0.5f;

        private float _currentFPS;
        private float _minFPS;
        private float _maxFPS;
        private float _avgFPS;
        private float _fpsAccumulator;
        private int _fpsFrameCount;
        private float _lastFPSUpdateTime;

        private float _warningFPS;
        private bool _isInWarningState;

        private GUIStyle _labelStyle;
        private GUIStyle _warningStyle;

        private void Awake()
        {
            _warningFPS = _targetFPS * _warningThreshold;
            ResetStats();
        }

        private void Update()
        {
            UpdateFPS();
            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                _showPerformanceUI = !_showPerformanceUI;
                UnityEngine.Debug.Log($"[CombatPerformanceTest] Performance UI: {(_showPerformanceUI ? "ON" : "OFF")}");
            }
        }

        private void UpdateFPS()
        {
            // Accumulate frame time
            _fpsAccumulator += Time.unscaledDeltaTime;
            _fpsFrameCount++;

            // Update FPS display at interval
            if (Time.unscaledTime - _lastFPSUpdateTime >= _fpsUpdateInterval)
            {
                _currentFPS = _fpsFrameCount / _fpsAccumulator;

                // Track min/max
                if (_currentFPS < _minFPS || _minFPS <= 0)
                {
                    _minFPS = _currentFPS;
                }
                if (_currentFPS > _maxFPS)
                {
                    _maxFPS = _currentFPS;
                }

                // Update rolling average (simple approximation)
                _avgFPS = (_avgFPS + _currentFPS) / 2f;
                if (_avgFPS <= 0) _avgFPS = _currentFPS;

                // Check for performance warning
                bool wasInWarningState = _isInWarningState;
                _isInWarningState = _currentFPS < _warningFPS;

                // Log warning when entering warning state
                if (_isInWarningState && !wasInWarningState)
                {
                    int activeEnemies = GetActiveEnemyCount();
                    int activeProjectiles = GetActiveProjectileCount();
                    UnityEngine.Debug.LogWarning(
                        $"[CombatPerformanceTest] FPS dropped below {_warningFPS:F0}! " +
                        $"Current: {_currentFPS:F1} FPS, " +
                        $"Enemies: {activeEnemies}, Projectiles: {activeProjectiles}");
                }

                // Reset accumulators
                _fpsAccumulator = 0f;
                _fpsFrameCount = 0;
                _lastFPSUpdateTime = Time.unscaledTime;
            }
        }

        /// <summary>
        /// Resets all performance statistics.
        /// </summary>
        public void ResetStats()
        {
            _currentFPS = 0f;
            _minFPS = 0f;
            _maxFPS = 0f;
            _avgFPS = 0f;
            _fpsAccumulator = 0f;
            _fpsFrameCount = 0;
            _lastFPSUpdateTime = Time.unscaledTime;
            _isInWarningState = false;

            UnityEngine.Debug.Log("[CombatPerformanceTest] Performance stats reset.");
        }

        private int GetActiveEnemyCount()
        {
            if (EnemyPoolManager.Instance != null)
            {
                return EnemyPoolManager.Instance.TotalActiveEnemies;
            }
            if (EnemySpawner.Instance != null)
            {
                return EnemySpawner.Instance.ActiveEnemyCount;
            }
            return 0;
        }

        private int GetActiveProjectileCount()
        {
            if (ProjectilePoolManager.Instance != null)
            {
                return ProjectilePoolManager.Instance.TotalActiveProjectiles;
            }
            return 0;
        }

        private Color GetFPSColor()
        {
            if (_currentFPS >= _targetFPS)
            {
                return _goodColor;
            }
            else if (_currentFPS >= _warningFPS)
            {
                return _warningColor;
            }
            else
            {
                return _badColor;
            }
        }

        private void OnGUI()
        {
            if (!_showPerformanceUI) return;

            EnsureStyles();

            float boxWidth = 220f;
            float boxHeight = 160f;
            float xPos = Screen.width - boxWidth - 10;
            float yPos = 10;

            GUILayout.BeginArea(new Rect(xPos, yPos, boxWidth, boxHeight));
            GUILayout.BeginVertical("box");

            GUILayout.Label("<b>Performance Monitor</b>", _labelStyle);
            GUILayout.Space(5);

            // FPS display with color coding
            Color originalColor = GUI.color;
            GUI.color = GetFPSColor();
            GUILayout.Label($"FPS: {_currentFPS:F1}", _labelStyle);
            GUI.color = originalColor;

            GUILayout.Label($"Min: {_minFPS:F1} / Max: {_maxFPS:F1}", _labelStyle);
            GUILayout.Label($"Avg: {_avgFPS:F1}", _labelStyle);

            GUILayout.Space(5);

            // Combat stats
            int activeEnemies = GetActiveEnemyCount();
            int activeProjectiles = GetActiveProjectileCount();
            GUILayout.Label($"Active Enemies: {activeEnemies}", _labelStyle);
            GUILayout.Label($"Active Projectiles: {activeProjectiles}", _labelStyle);

            // Warning indicator
            if (_isInWarningState)
            {
                GUILayout.Space(5);
                GUILayout.Label($"WARNING: FPS < {_warningFPS:F0}!", _warningStyle);
            }

            GUILayout.Space(5);
            GUILayout.Label($"[{_toggleKey}] Toggle Display", _labelStyle);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    fontSize = 12
                };
            }

            if (_warningStyle == null)
            {
                _warningStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    fontSize = 12,
                    normal = { textColor = _badColor },
                    fontStyle = FontStyle.Bold
                };
            }
        }
    }
}
