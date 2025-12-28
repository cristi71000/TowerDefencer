## Context

Game feel ("juice") makes actions satisfying. This issue adds screen shake, hit pause, UI animations, and other micro-feedback that makes the game feel responsive and polished.

**Builds upon:** Issues 44-45 (VFX, Audio)

## Detailed Implementation Instructions

### Game Feel Manager

Create `GameFeelManager.cs`:

```csharp
using UnityEngine;
using Cinemachine;

namespace TowerDefense.Core
{
    public class GameFeelManager : MonoBehaviour
    {
        public static GameFeelManager Instance { get; private set; }

        [Header("Camera Shake")]
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        [Header("Hit Pause")]
        [SerializeField] private float _hitPauseDuration = 0.05f;
        [SerializeField] private float _hitPauseTimeScale = 0.1f;

        [Header("Slow Motion")]
        [SerializeField] private float _slowMoDuration = 0.3f;
        [SerializeField] private float _slowMoTimeScale = 0.3f;

        private Coroutine _hitPauseCoroutine;
        private Coroutine _slowMoCoroutine;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void ShakeCamera(float intensity = 1f)
        {
            _impulseSource?.GenerateImpulse(intensity);
        }

        public void DoHitPause()
        {
            if (_hitPauseCoroutine != null)
                StopCoroutine(_hitPauseCoroutine);
            _hitPauseCoroutine = StartCoroutine(HitPauseCoroutine());
        }

        private System.Collections.IEnumerator HitPauseCoroutine()
        {
            Time.timeScale = _hitPauseTimeScale;
            yield return new WaitForSecondsRealtime(_hitPauseDuration);
            Time.timeScale = 1f;
        }

        public void DoSlowMo()
        {
            if (_slowMoCoroutine != null)
                StopCoroutine(_slowMoCoroutine);
            _slowMoCoroutine = StartCoroutine(SlowMoCoroutine());
        }

        private System.Collections.IEnumerator SlowMoCoroutine()
        {
            Time.timeScale = _slowMoTimeScale;
            float elapsed = 0f;
            while (elapsed < _slowMoDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                Time.timeScale = Mathf.Lerp(_slowMoTimeScale, 1f, elapsed / _slowMoDuration);
                yield return null;
            }
            Time.timeScale = 1f;
        }
    }
}
```

### UI Tweening

Create `UITween.cs`:

```csharp
using UnityEngine;
using System.Collections;

namespace TowerDefense.UI
{
    public static class UITween
    {
        public static IEnumerator ScalePunch(Transform target, float intensity = 1.2f, float duration = 0.2f)
        {
            Vector3 original = target.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float scale = t < 0.5f
                    ? Mathf.Lerp(1f, intensity, t * 2f)
                    : Mathf.Lerp(intensity, 1f, (t - 0.5f) * 2f);
                target.localScale = original * scale;
                yield return null;
            }
            target.localScale = original;
        }

        public static IEnumerator Shake(Transform target, float intensity = 5f, float duration = 0.3f)
        {
            Vector3 original = target.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float strength = intensity * (1f - elapsed / duration);
                target.localPosition = original + (Vector3)Random.insideUnitCircle * strength;
                yield return null;
            }
            target.localPosition = original;
        }

        public static IEnumerator FadeIn(CanvasGroup group, float duration = 0.3f)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = elapsed / duration;
                yield return null;
            }
            group.alpha = 1f;
        }

        public static IEnumerator FadeOut(CanvasGroup group, float duration = 0.3f)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = 1f - elapsed / duration;
                yield return null;
            }
            group.alpha = 0f;
        }
    }
}
```

### Integration Examples

**On Critical Hit:**
```csharp
GameFeelManager.Instance.ShakeCamera(0.3f);
GameFeelManager.Instance.DoHitPause();
```

**On Boss Death:**
```csharp
GameFeelManager.Instance.ShakeCamera(1f);
GameFeelManager.Instance.DoSlowMo();
```

**On Currency Gain:**
```csharp
StartCoroutine(UITween.ScalePunch(currencyText.transform));
```

### Number Pop Effect

Create `NumberPop.cs` for satisfying number displays:

```csharp
using UnityEngine;
using TMPro;

namespace TowerDefense.UI
{
    public class NumberPop : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private AnimationCurve _scaleCurve;
        [SerializeField] private float _animDuration = 0.3f;

        public void Pop(int value, Color color)
        {
            _text.text = value.ToString();
            _text.color = color;
            StartCoroutine(PopAnimation());
        }

        private System.Collections.IEnumerator PopAnimation()
        {
            float elapsed = 0f;
            while (elapsed < _animDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / _animDuration;
                float scale = _scaleCurve.Evaluate(t);
                transform.localScale = Vector3.one * scale;
                yield return null;
            }
        }
    }
}
```

## Testing and Acceptance Criteria

### Done When

- [ ] Camera shake on impacts
- [ ] Hit pause on big hits
- [ ] Slow-mo on boss death
- [ ] UI elements punch on interaction
- [ ] Currency text animates on change
- [ ] Panel fade in/out
- [ ] Feels responsive and satisfying
- [ ] Not overwhelming or distracting

## Dependencies

- Issues 44-45: VFX, Audio
