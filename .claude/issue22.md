## Context

Players need a UI to select which tower type to build. This issue implements the tower selection panel with buttons showing tower icons, costs, and handling affordability states.

**Builds upon:** Issues 6-7, 20-21 (Tower systems, Economy, HUD)

## Detailed Implementation Instructions

### Tower Button Component

Create `TowerButton.cs` in `_Project/Scripts/Runtime/UI/`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefense.Towers;
using TowerDefense.Economy;

namespace TowerDefense.UI
{
    public class TowerButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private GameObject _lockedOverlay;

        [Header("Colors")]
        [SerializeField] private Color _affordableColor = Color.white;
        [SerializeField] private Color _unaffordableColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color _selectedColor = new Color(0.3f, 0.8f, 0.3f, 1f);

        private TowerData _towerData;
        private bool _isSelected;

        public TowerData TowerData => _towerData;

        public event System.Action<TowerData> OnClicked;

        private void Awake()
        {
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClick);
        }

        public void Initialize(TowerData data)
        {
            _towerData = data;

            if (_iconImage != null && data.Icon != null)
                _iconImage.sprite = data.Icon;

            if (_costText != null)
                _costText.text = data.PurchaseCost.ToString();

            UpdateAffordability();
        }

        private void Update()
        {
            UpdateAffordability();
        }

        public void UpdateAffordability()
        {
            if (_towerData == null) return;

            bool canAfford = EconomyManager.Instance?.CanAfford(_towerData.PurchaseCost) ?? false;

            _button.interactable = canAfford;

            Color targetColor = _isSelected ? _selectedColor :
                               (canAfford ? _affordableColor : _unaffordableColor);

            if (_backgroundImage != null)
                _backgroundImage.color = targetColor;

            if (_costText != null)
                _costText.color = canAfford ? Color.white : Color.red;

            if (_lockedOverlay != null)
                _lockedOverlay.SetActive(!canAfford);
        }

        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateAffordability();
        }

        private void HandleClick()
        {
            OnClicked?.Invoke(_towerData);
        }
    }
}
```

### Tower Selection Panel

Create `TowerSelectionPanel.cs`:

```csharp
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Towers;

namespace TowerDefense.UI
{
    public class TowerSelectionPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private TowerButton _buttonPrefab;

        [Header("Tower Data")]
        [SerializeField] private TowerData[] _availableTowers;

        [Header("Hotkeys")]
        [SerializeField] private KeyCode[] _towerHotkeys = {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
            KeyCode.Alpha4, KeyCode.Alpha5
        };

        private List<TowerButton> _buttons = new List<TowerButton>();
        private TowerButton _selectedButton;

        private void Start()
        {
            CreateButtons();
        }

        private void Update()
        {
            HandleHotkeys();
        }

        private void CreateButtons()
        {
            foreach (var tower in _availableTowers)
            {
                if (tower == null) continue;

                TowerButton button = Instantiate(_buttonPrefab, _buttonContainer);
                button.Initialize(tower);
                button.OnClicked += HandleTowerSelected;
                _buttons.Add(button);
            }
        }

        private void HandleTowerSelected(TowerData data)
        {
            // Deselect previous
            if (_selectedButton != null)
                _selectedButton.SetSelected(false);

            // Find and select new button
            _selectedButton = _buttons.Find(b => b.TowerData == data);
            if (_selectedButton != null)
                _selectedButton.SetSelected(true);

            // Start placement
            TowerPlacementManager.Instance?.StartPlacement(data);
        }

        private void HandleHotkeys()
        {
            for (int i = 0; i < _towerHotkeys.Length && i < _buttons.Count; i++)
            {
                if (Input.GetKeyDown(_towerHotkeys[i]))
                {
                    if (_buttons[i].TowerData != null)
                        HandleTowerSelected(_buttons[i].TowerData);
                }
            }
        }

        public void CancelSelection()
        {
            if (_selectedButton != null)
            {
                _selectedButton.SetSelected(false);
                _selectedButton = null;
            }
        }

        public void RefreshButtons()
        {
            foreach (var button in _buttons)
            {
                button.UpdateAffordability();
            }
        }
    }
}
```

### Tower Button Prefab Setup

Create prefab structure:

```
TowerButton (TowerButton.cs, Button)
|-- Background (Image)
|-- Icon (Image)
|-- CostPanel
|   |-- CoinIcon (Image)
|   +-- CostText (TMP)
|-- HotkeyLabel (TMP, e.g., "1")
+-- LockedOverlay (Image, semi-transparent)
```

### UI Layout

Add to HUD Canvas:

```
--- UI ---
+-- HUDCanvas
    |-- TopPanel (existing)
    +-- BottomPanel
        +-- TowerSelectionPanel (TowerSelectionPanel.cs)
            |-- ButtonContainer (Horizontal Layout Group)
            |   |-- TowerButton_1 (instance)
            |   |-- TowerButton_2 (instance)
            |   +-- ... more buttons
            +-- Background (Image)
```

### Panel Positioning

- Anchor: Bottom-center
- Position: 20 pixels from bottom
- Use Horizontal Layout Group with spacing

## Testing and Acceptance Criteria

### Done When

- [ ] Tower buttons display for each available tower
- [ ] Buttons show correct icon and cost
- [ ] Unaffordable towers grayed out
- [ ] Clicking button starts placement mode
- [ ] Selected button highlighted
- [ ] Hotkeys 1-5 select towers
- [ ] Buttons update when currency changes

## Dependencies

- Issues 6-7: Tower systems
- Issues 20-21: Economy, HUD
