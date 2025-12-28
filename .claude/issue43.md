## Context

When a tower is upgraded, there should be visual and audio feedback to make the action feel satisfying. This issue adds polish to the upgrade experience.

**Builds upon:** Issues 40-42 (Upgrade system and UI)

## Detailed Implementation Instructions

### Upgrade VFX

Create `VFX_TowerUpgrade.prefab`:

```csharp
// TowerUpgradeVFX.cs
using UnityEngine;

namespace TowerDefense.Towers
{
    public class TowerUpgradeVFX : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _burstParticles;
        [SerializeField] private ParticleSystem _glowParticles;
        [SerializeField] private float _duration = 1.5f;

        private void Start()
        {
            _burstParticles?.Play();
            _glowParticles?.Play();
            Destroy(gameObject, _duration);
        }
    }
}
```

Particle System setup:
- Burst: 30-50 particles, golden/yellow color, burst upward
- Glow: Ring expanding outward, fade over time
- Stars/sparkles rising

### Upgrade Audio

Create audio clips:
- `SFX_TowerUpgrade.wav` - Magical/mechanical upgrade sound
- Consider layered sounds: construction + magic chime

### Update TowerUpgradeManager

```csharp
// Add to TowerUpgradeManager.cs
[SerializeField] private GameObject _upgradeVFXPrefab;
[SerializeField] private AudioClip _upgradeSound;
[SerializeField] private AudioSource _audioSource;

private void PerformUpgrade(Tower tower, TowerData newData)
{
    Vector3 position = tower.transform.position;

    // Play effects at old tower position
    PlayUpgradeEffects(position);

    // ... existing upgrade code
}

private void PlayUpgradeEffects(Vector3 position)
{
    // VFX
    if (_upgradeVFXPrefab != null)
    {
        Instantiate(_upgradeVFXPrefab, position, Quaternion.identity);
    }

    // Audio
    if (_upgradeSound != null && _audioSource != null)
    {
        _audioSource.PlayOneShot(_upgradeSound);
    }
}
```

### Tower Flash Effect

Add temporary visual effect to upgraded tower:

```csharp
// TowerUpgradeFlash.cs
using UnityEngine;
using System.Collections;

namespace TowerDefense.Towers
{
    public class TowerUpgradeFlash : MonoBehaviour
    {
        [SerializeField] private float _flashDuration = 0.5f;
        [SerializeField] private Color _flashColor = Color.yellow;
        [SerializeField] private int _flashCount = 3;

        public void PlayFlash()
        {
            StartCoroutine(FlashCoroutine());
        }

        private IEnumerator FlashCoroutine()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            Material[] originalMaterials = new Material[renderers.Length];

            // Store original materials
            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].material;
            }

            float interval = _flashDuration / (_flashCount * 2);

            for (int f = 0; f < _flashCount; f++)
            {
                // Flash on
                foreach (var renderer in renderers)
                {
                    renderer.material.SetColor("_EmissionColor", _flashColor);
                    renderer.material.EnableKeyword("_EMISSION");
                }
                yield return new WaitForSeconds(interval);

                // Flash off
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].material = originalMaterials[i];
                }
                yield return new WaitForSeconds(interval);
            }
        }
    }
}
```

### UI Feedback

Add upgrade confirmation feedback:

```csharp
// In TowerUpgradePanel.cs after successful upgrade:
private void ShowUpgradeSuccess(TowerData newData)
{
    // Flash the panel
    // Show "+1" or tier indicator
    // Play UI sound
}
```

### Upgrade Notification

Create `UpgradeNotification.cs`:

```csharp
using UnityEngine;
using TMPro;

namespace TowerDefense.UI
{
    public class UpgradeNotification : MonoBehaviour
    {
        public static UpgradeNotification Instance { get; private set; }

        [SerializeField] private GameObject _notificationPrefab;
        [SerializeField] private Transform _notificationContainer;

        private void Awake()
        {
            Instance = this;
        }

        public void ShowNotification(string towerName, int newTier)
        {
            // Create floating notification
            // "Arrow Tower upgraded to Tier 2!"
        }
    }
}
```

## Testing and Acceptance Criteria

### Done When

- [ ] VFX plays when tower upgrades
- [ ] Audio plays when tower upgrades
- [ ] Upgraded tower flashes briefly
- [ ] VFX auto-destroys after duration
- [ ] Effects do not obstruct gameplay
- [ ] UI provides feedback on success
- [ ] Feels satisfying and polished

## Dependencies

- Issues 40-42: Upgrade system
