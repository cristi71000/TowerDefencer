## Context

Enemies need visual feedback for their health state. This issue implements a world-space health bar that floats above each enemy, showing current health percentage. The health bar should face the camera and update smoothly as damage is taken.

**Builds upon:** Issue 9, 11 (Enemy Prefab, Spawner)

## Detailed Implementation Instructions

### Enemy Health Bar Component

Create `EnemyHealthBar.cs` in `_Project/Scripts/Runtime/Enemies/`:

```csharp
using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense.Enemies
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Settings")]
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private bool _hideWhenFull = true;
        [SerializeField] private Gradient _healthGradient;

        private Transform _cameraTransform;
        private float _targetFill;
        private float _currentFill;

        private void Awake()
        {
            _cameraTransform = Camera.main.transform;
            _currentFill = 1f;
            _targetFill = 1f;

            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void LateUpdate()
        {
            // Face camera (billboard)
            transform.rotation = Quaternion.LookRotation(transform.position - _cameraTransform.position);

            // Smooth fill
            if (Mathf.Abs(_currentFill - _targetFill) > 0.001f)
            {
                _currentFill = Mathf.Lerp(_currentFill, _targetFill, Time.deltaTime * _smoothSpeed);
                UpdateVisuals();
            }
        }

        public void SetHealth(float normalizedHealth)
        {
            _targetFill = Mathf.Clamp01(normalizedHealth);

            // Visibility
            if (_hideWhenFull && _canvasGroup != null)
            {
                _canvasGroup.alpha = normalizedHealth < 1f ? 1f : 0f;
            }
        }

        public void SetHealthImmediate(float normalizedHealth)
        {
            _targetFill = Mathf.Clamp01(normalizedHealth);
            _currentFill = _targetFill;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_fillImage != null)
            {
                _fillImage.fillAmount = _currentFill;

                if (_healthGradient != null)
                    _fillImage.color = _healthGradient.Evaluate(_currentFill);
            }
        }

        public void Reset()
        {
            _currentFill = 1f;
            _targetFill = 1f;
            UpdateVisuals();

            if (_hideWhenFull && _canvasGroup != null)
                _canvasGroup.alpha = 0f;
        }
    }
}
```

### Health Bar Prefab Structure

```
EnemyHealthBar (Canvas - World Space)
|-- Background (Image - dark/black)
+-- Fill (Image - health color, filled)
```

### Prefab Setup

1. Create new UI > Canvas
2. Set Canvas:
   - Render Mode: World Space
   - Width: 100, Height: 15
   - Scale: (0.01, 0.01, 0.01)
3. Add CanvasGroup component
4. Add EnemyHealthBar script

**Background Image:**
- Anchor: Stretch
- Color: RGB(40, 40, 40)
- Raycast Target: false

**Fill Image:**
- Anchor: Stretch
- Image Type: Filled
- Fill Method: Horizontal
- Fill Origin: Left
- Raycast Target: false

Create gradient for health color (green to yellow to red).

Save as `_Project/Prefabs/UI/EnemyHealthBar.prefab`

### Update Enemy Prefab

Add health bar to BasicEnemy:

```
BasicEnemy
|-- Model
|-- TargetPoint
+-- HealthBarAnchor
    +-- EnemyHealthBar (instance of prefab)
```

### Update Enemy.cs

Add health bar integration:

```csharp
// Add to Enemy.cs
[SerializeField] private EnemyHealthBar _healthBar;

private void Start()
{
    if (_healthBar != null)
        _healthBar.SetHealthImmediate(1f);
}

// In TakeDamage after health changes:
_healthBar?.SetHealth(HealthPercent);

// In ResetEnemy:
_healthBar?.Reset();
```

## Testing and Acceptance Criteria

### Done When

- [ ] Health bar prefab created with world-space canvas
- [ ] Health bar faces camera (billboard)
- [ ] Fill smoothly animates on damage
- [ ] Color changes based on health (gradient)
- [ ] Health bar hidden when at full health
- [ ] Health bar resets when enemy returns to pool
- [ ] No errors in play mode

## Dependencies

- Issue 9: Enemy Prefab
- Issue 11: Spawner (pool reset)
