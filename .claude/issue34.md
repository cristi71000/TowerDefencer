## Context

With multiple tower types available, the selection UI needs improvements: tooltips showing tower stats, visual distinction between tower types, and better affordability feedback.

**Builds upon:** Issue 22 (Tower Selection Panel)

## Detailed Implementation Instructions

### Tower Tooltip Component

Create `TowerTooltip.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefense.Towers;

namespace TowerDefense.UI
{
    public class TowerTooltip : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _rectTransform;

        [Header("Content")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _statsText;
        [SerializeField] private Image _typeIcon;

        [Header("Type Icons")]
        [SerializeField] private Sprite _basicIcon;
        [SerializeField] private Sprite _aoeIcon;
        [SerializeField] private Sprite _slowIcon;
        [SerializeField] private Sprite _sniperIcon;
        [SerializeField] private Sprite _supportIcon;

        [Header("Settings")]
        [SerializeField] private Vector2 _offset = new Vector2(10, 10);
        [SerializeField] private float _showDelay = 0.5f;

        private TowerData _currentData;
        private float _hoverTimer;
        private bool _isShowing;

        private void Awake()
        {
            Hide();
        }

        private void Update()
        {
            if (_isShowing)
            {
                FollowMouse();
            }
        }

        public void ShowForTower(TowerData data)
        {
            _currentData = data;
            PopulateContent();
            _canvasGroup.alpha = 1f;
            _isShowing = true;
            FollowMouse();
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0f;
            _isShowing = false;
            _currentData = null;
        }

        private void PopulateContent()
        {
            if (_currentData == null) return;

            _nameText.text = _currentData.TowerName;
            _descriptionText.text = _currentData.Description;

            // Build stats string
            string stats = "";
            stats += $"Cost: <color=yellow>{_currentData.PurchaseCost}</color>\n";

            if (_currentData.Damage > 0)
                stats += $"Damage: {_currentData.Damage}\n";

            if (_currentData.AttackSpeed > 0)
                stats += $"Attack Speed: {_currentData.AttackSpeed}/s\n";

            if (_currentData.Range > 0)
                stats += $"Range: {_currentData.Range}\n";

            if (_currentData.AOERadius > 0)
                stats += $"AOE Radius: {_currentData.AOERadius}\n";

            if (_currentData.SlowAmount > 0)
                stats += $"Slow: {_currentData.SlowAmount * 100}%\n";

            if (_currentData.BuffAmount > 0)
                stats += $"Buff: +{_currentData.BuffAmount * 100}% damage\n";

            _statsText.text = stats;

            // Set type icon
            _typeIcon.sprite = GetTypeIcon(_currentData.Type);
        }

        private Sprite GetTypeIcon(TowerType type)
        {
            return type switch
            {
                TowerType.Basic => _basicIcon,
                TowerType.AOE => _aoeIcon,
                TowerType.Slow => _slowIcon,
                TowerType.Sniper => _sniperIcon,
                TowerType.Support => _supportIcon,
                _ => _basicIcon
            };
        }

        private void FollowMouse()
        {
            Vector2 mousePos = Input.mousePosition;

            // Clamp to screen bounds
            float width = _rectTransform.rect.width;
            float height = _rectTransform.rect.height;

            float x = mousePos.x + _offset.x;
            float y = mousePos.y + _offset.y;

            // Flip if near screen edge
            if (x + width > Screen.width)
                x = mousePos.x - width - _offset.x;

            if (y + height > Screen.height)
                y = mousePos.y - height - _offset.y;

            _rectTransform.position = new Vector3(x, y, 0);
        }
    }
}
```

### Update Tower Button for Tooltip

```csharp
// Add to TowerButton.cs
using UnityEngine.EventSystems;

public class TowerButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TowerTooltip _tooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _tooltip?.ShowForTower(_towerData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltip?.Hide();
    }
}
```

### Tower Button Visual Improvements

Update TowerButton prefab:

```
TowerButton
|-- Background (Image with rounded corners)
|-- TypeIndicator (Colored bar at top indicating type)
|-- Icon (Tower icon)
|-- CostPanel
|   |-- CoinIcon
|   +-- CostText
|-- HotkeyBadge (corner badge showing 1, 2, 3...)
|-- SelectionHighlight (hidden by default)
+-- CooldownOverlay (for future upgrade cooldowns)
```

### Type Color Coding

```csharp
public static class TowerTypeColors
{
    public static Color GetColor(TowerType type)
    {
        return type switch
        {
            TowerType.Basic => new Color(0.7f, 0.7f, 0.7f),    // Gray
            TowerType.AOE => new Color(1f, 0.5f, 0.2f),        // Orange
            TowerType.Slow => new Color(0.3f, 0.7f, 1f),       // Blue
            TowerType.Sniper => new Color(0.8f, 0.2f, 0.2f),   // Red
            TowerType.Support => new Color(0.3f, 1f, 0.3f),    // Green
            _ => Color.white
        };
    }
}
```

### Tooltip Prefab Setup

Create `TowerTooltip` prefab:

```
TowerTooltip (TowerTooltip.cs, CanvasGroup)
|-- Background (Panel with dark semi-transparent)
|-- Header
|   |-- TypeIcon (Image)
|   +-- NameText (TMP Bold)
|-- DescriptionText (TMP Italic)
+-- StatsText (TMP)
```

## Testing and Acceptance Criteria

### Done When

- [ ] Tooltip appears on button hover
- [ ] Tooltip shows tower name and description
- [ ] Tooltip shows relevant stats
- [ ] Type icon displays correctly
- [ ] Tooltip follows mouse
- [ ] Tooltip stays on screen
- [ ] Type color indicators on buttons
- [ ] Hotkey badges visible

## Dependencies

- Issue 22: Tower Selection Panel
- Issues 30-33: Tower variety
