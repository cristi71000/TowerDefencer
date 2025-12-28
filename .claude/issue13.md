## Context

To complete the core enemy milestone, enemies need death effects and proper cleanup. This issue adds simple death VFX, ensures enemies are properly removed from the game state, and provides visual feedback when enemies are killed.

**Builds upon:** Issues 9-12 (Enemy systems)

## Detailed Implementation Instructions

### Death Effect Component

Create `EnemyDeathEffect.cs` in `_Project/Scripts/Runtime/Enemies/`:

```csharp
using UnityEngine;

namespace TowerDefense.Enemies
{
    public class EnemyDeathEffect : MonoBehaviour
    {
        [SerializeField] private GameObject _deathVFXPrefab;
        [SerializeField] private float _vfxDuration = 2f;
        [SerializeField] private bool _scaleWithEnemy = true;

        public void PlayDeathEffect(Vector3 position, float scale = 1f)
        {
            if (_deathVFXPrefab == null) return;

            GameObject vfx = Instantiate(_deathVFXPrefab, position, Quaternion.identity);

            if (_scaleWithEnemy)
                vfx.transform.localScale = Vector3.one * scale;

            Destroy(vfx, _vfxDuration);
        }
    }
}
```

### Simple Death VFX Prefab

Create using Unity Particle System:

1. Create new Particle System
2. Configure:
   - Duration: 0.5
   - Looping: false
   - Start Lifetime: 0.3-0.5
   - Start Speed: 3-5
   - Start Size: 0.2-0.4
   - Start Color: Red/Orange
   - Gravity Modifier: 0.5
   - Emission: Burst of 15-20 particles
   - Shape: Sphere, Radius 0.3
   - Color over Lifetime: Fade out
   - Size over Lifetime: Shrink

Save as `_Project/Prefabs/VFX/VFX_EnemyDeath.prefab`

### Update Enemy.cs for Death Effects

```csharp
// Add to Enemy.cs
[SerializeField] private EnemyDeathEffect _deathEffect;

private void Die()
{
    if (_isDead) return;
    _isDead = true;
    _navAgent.isStopped = true;

    // Play death effect
    _deathEffect?.PlayDeathEffect(transform.position, _enemyData.ModelScale);

    OnDeath?.Invoke(this);
}
```

### Currency Popup (Optional Visual)

Create `CurrencyPopup.cs` for floating text:

```csharp
using UnityEngine;
using TMPro;

namespace TowerDefense.UI
{
    public class CurrencyPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private float _floatSpeed = 1f;
        [SerializeField] private float _fadeSpeed = 2f;
        [SerializeField] private float _lifetime = 1f;

        private float _timer;
        private Color _originalColor;

        private void Awake()
        {
            _originalColor = _text.color;
        }

        public void Initialize(int amount)
        {
            _text.text = $"+{amount}";
            _timer = 0f;
            _text.color = _originalColor;
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            // Float up
            transform.position += Vector3.up * _floatSpeed * Time.deltaTime;

            // Fade out
            Color c = _text.color;
            c.a = Mathf.Lerp(1f, 0f, _timer / _lifetime);
            _text.color = c;

            if (_timer >= _lifetime)
                Destroy(gameObject);
        }
    }
}
```

### Integration with Spawner

Update EnemySpawner to show currency popup on death:

```csharp
[SerializeField] private CurrencyPopup _currencyPopupPrefab;

private void HandleDeath(Enemy enemy)
{
    // Show currency popup
    if (_currencyPopupPrefab != null)
    {
        var popup = Instantiate(_currencyPopupPrefab, enemy.transform.position + Vector3.up, Quaternion.identity);
        popup.Initialize(enemy.Data.CurrencyReward);
    }

    // ... rest of death handling
}
```

## Testing and Acceptance Criteria

### Done When

- [ ] Death VFX particle system created
- [ ] VFX plays at enemy position on death
- [ ] VFX auto-destroys after duration
- [ ] Currency popup shows reward amount
- [ ] Popup floats up and fades out
- [ ] No lingering objects after death
- [ ] Pool properly recycles enemy

## Dependencies

- Issues 9-12: Enemy systems

## Notes

- Consider pooling VFX for performance in later optimization pass
- Death effects should be quick and not obstruct gameplay
- Currency popup is optional but improves game feel
