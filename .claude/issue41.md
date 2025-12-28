## Context

Players need a UI to view and select tower upgrades. When a tower is selected, available upgrades should be displayed with costs and stat comparisons.

**Builds upon:** Issues 8, 40 (Tower Selection, Upgrade System)

## Detailed Implementation Instructions

### Tower Upgrade Panel

Create `TowerUpgradePanel.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefense.Towers;
using TowerDefense.Economy;

namespace TowerDefense.UI
{
    public class TowerUpgradePanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private RectTransform _upgradeButtonContainer;

        [Header("Current Tower Info")]
        [SerializeField] private TextMeshProUGUI _towerNameText;
        [SerializeField] private TextMeshProUGUI _towerStatsText;
        [SerializeField] private Image _towerIcon;

        [Header("Buttons")]
        [SerializeField] private Button _sellButton;
        [SerializeField] private TextMeshProUGUI _sellValueText;
        [SerializeField] private UpgradeButton _upgradeButtonPrefab;

        [Header("Priority")]
        [SerializeField] private Button _priorityButton;
        [SerializeField] private TextMeshProUGUI _priorityText;

        private Tower _selectedTower;
        private System.Collections.Generic.List<UpgradeButton> _upgradeButtons = new System.Collections.Generic.List<UpgradeButton>();

        private void Start()
        {
            _sellButton?.onClick.AddListener(OnSellClicked);
            _priorityButton?.onClick.AddListener(OnPriorityClicked);

            TowerSelectionManager.Instance.OnTowerSelected += ShowForTower;
            TowerSelectionManager.Instance.OnTowerDeselected += Hide;

            Hide();
        }

        private void OnDestroy()
        {
            _sellButton?.onClick.RemoveListener(OnSellClicked);
            _priorityButton?.onClick.RemoveListener(OnPriorityClicked);

            if (TowerSelectionManager.Instance != null)
            {
                TowerSelectionManager.Instance.OnTowerSelected -= ShowForTower;
                TowerSelectionManager.Instance.OnTowerDeselected -= Hide;
            }
        }

        public void ShowForTower(Tower tower)
        {
            _selectedTower = tower;
            _panel.SetActive(true);

            UpdateTowerInfo();
            UpdateSellButton();
            UpdatePriorityButton();
            CreateUpgradeButtons();
        }

        public void Hide()
        {
            _panel.SetActive(false);
            _selectedTower = null;
            ClearUpgradeButtons();
        }

        private void UpdateTowerInfo()
        {
            if (_selectedTower == null) return;

            var data = _selectedTower.Data;
            _towerNameText.text = data.TowerName;

            if (data.Icon != null)
                _towerIcon.sprite = data.Icon;

            string stats = $"Damage: {data.Damage}\n";
            stats += $"Attack Speed: {data.AttackSpeed}/s\n";
            stats += $"Range: {data.Range}";
            _towerStatsText.text = stats;
        }

        private void UpdateSellButton()
        {
            if (_selectedTower == null) return;
            _sellValueText.text = $"Sell (${_selectedTower.Data.SellValue})";
        }

        private void UpdatePriorityButton()
        {
            if (_selectedTower == null) return;
            _priorityText.text = $"Priority: {_selectedTower.CurrentPriority}";
        }

        private void CreateUpgradeButtons()
        {
            ClearUpgradeButtons();

            var upgrades = TowerUpgradeManager.Instance?.GetAvailableUpgrades(_selectedTower);
            if (upgrades == null || upgrades.Length == 0) return;

            foreach (var upgrade in upgrades)
            {
                var button = Instantiate(_upgradeButtonPrefab, _upgradeButtonContainer);
                button.Initialize(upgrade, _selectedTower);
                button.OnClicked += HandleUpgradeClicked;
                _upgradeButtons.Add(button);
            }
        }

        private void ClearUpgradeButtons()
        {
            foreach (var button in _upgradeButtons)
            {
                button.OnClicked -= HandleUpgradeClicked;
                Destroy(button.gameObject);
            }
            _upgradeButtons.Clear();
        }

        private void HandleUpgradeClicked(TowerData upgradeData)
        {
            if (TowerUpgradeManager.Instance.TryUpgrade(_selectedTower, upgradeData))
            {
                // Find new tower at same position and select it
                // Or just hide panel
                Hide();
            }
        }

        private void OnSellClicked()
        {
            TowerSelectionManager.Instance?.SellSelectedTower();
        }

        private void OnPriorityClicked()
        {
            TowerSelectionManager.Instance?.CycleTargetingPriority();
            UpdatePriorityButton();
        }
    }
}
```

### Upgrade Button Component

Create `UpgradeButton.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefense.Towers;
using TowerDefense.Economy;

namespace TowerDefense.UI
{
    public class UpgradeButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private TextMeshProUGUI _statsText;
        [SerializeField] private GameObject _unaffordableOverlay;

        private TowerData _upgradeData;
        private Tower _currentTower;

        public event System.Action<TowerData> OnClicked;

        private void Awake()
        {
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HandleClick);
        }

        public void Initialize(TowerData upgradeData, Tower currentTower)
        {
            _upgradeData = upgradeData;
            _currentTower = currentTower;

            _nameText.text = upgradeData.TowerName;
            if (upgradeData.Icon != null)
                _icon.sprite = upgradeData.Icon;

            int cost = TowerUpgradeManager.Instance.GetUpgradeCost(currentTower, upgradeData);
            _costText.text = $"${cost}";

            // Show stat improvements
            ShowStatComparison();
            UpdateAffordability();
        }

        private void Update()
        {
            UpdateAffordability();
        }

        private void ShowStatComparison()
        {
            if (_currentTower == null || _upgradeData == null) return;

            var current = _currentTower.Data;
            var upgrade = _upgradeData;

            string stats = "";
            if (upgrade.Damage != current.Damage)
                stats += $"Damage: {current.Damage} -> <color=green>{upgrade.Damage}</color>\n";
            if (upgrade.AttackSpeed != current.AttackSpeed)
                stats += $"Speed: {current.AttackSpeed} -> <color=green>{upgrade.AttackSpeed}</color>\n";
            if (upgrade.Range != current.Range)
                stats += $"Range: {current.Range} -> <color=green>{upgrade.Range}</color>";

            _statsText.text = stats;
        }

        private void UpdateAffordability()
        {
            bool canAfford = TowerUpgradeManager.Instance?.CanUpgrade(_currentTower, _upgradeData) ?? false;
            _button.interactable = canAfford;
            _unaffordableOverlay.SetActive(!canAfford);
        }

        private void HandleClick()
        {
            OnClicked?.Invoke(_upgradeData);
        }
    }
}
```

### Panel Layout

```
TowerUpgradePanel
|-- Background
|-- Header
|   |-- TowerIcon
|   +-- TowerNameText
|-- StatsSection
|   +-- TowerStatsText
|-- UpgradeSection
|   |-- "Upgrades:" label
|   +-- UpgradeButtonContainer (Vertical Layout)
|-- ActionButtons
|   |-- SellButton
|   +-- PriorityButton
```

## Testing and Acceptance Criteria

### Done When

- [ ] Panel appears on tower selection
- [ ] Current tower stats displayed
- [ ] Available upgrades shown as buttons
- [ ] Upgrade cost displayed
- [ ] Stat comparison shows improvements
- [ ] Unaffordable upgrades grayed out
- [ ] Clicking upgrade performs upgrade
- [ ] Sell button works
- [ ] Priority button cycles priority

## Dependencies

- Issue 8: Tower Selection
- Issue 40: Upgrade System
