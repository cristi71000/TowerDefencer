## Context

The economy system manages player currency (gold/coins) used to purchase and upgrade towers. This issue formalizes the economy with proper event handling, wave bonuses, and interest mechanics for strategic depth.

**Builds upon:** Issue 2 (Game Manager)

## Detailed Implementation Instructions

### Economy Manager

Create `EconomyManager.cs` in `_Project/Scripts/Runtime/Economy/`:

```csharp
using UnityEngine;
using TowerDefense.Core;
using TowerDefense.Core.Events;

namespace TowerDefense.Economy
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Starting Values")]
        [SerializeField] private int _startingCurrency = 200;

        [Header("Wave Bonuses")]
        [SerializeField] private int _baseWaveBonus = 50;
        [SerializeField] private int _waveBonusIncrement = 10;
        [SerializeField] private bool _enableInterest = false;
        [SerializeField] private float _interestRate = 0.05f;
        [SerializeField] private int _interestCap = 50;

        [Header("Events")]
        [SerializeField] private IntEventChannel _onCurrencyChanged;
        [SerializeField] private IntEventChannel _onWaveCompleted;

        private int _currentCurrency;
        private int _totalEarned;
        private int _totalSpent;

        public int CurrentCurrency => _currentCurrency;
        public int TotalEarned => _totalEarned;
        public int TotalSpent => _totalSpent;

        public event System.Action<int, int> OnCurrencyChanged; // newAmount, delta

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            if (_onWaveCompleted != null)
                _onWaveCompleted.OnEventRaised += HandleWaveCompleted;
        }

        private void OnDisable()
        {
            if (_onWaveCompleted != null)
                _onWaveCompleted.OnEventRaised -= HandleWaveCompleted;
        }

        public void Initialize()
        {
            _currentCurrency = _startingCurrency;
            _totalEarned = _startingCurrency;
            _totalSpent = 0;
            NotifyCurrencyChanged(0);
        }

        public void AddCurrency(int amount)
        {
            if (amount <= 0) return;

            _currentCurrency += amount;
            _totalEarned += amount;
            NotifyCurrencyChanged(amount);
        }

        public bool TrySpend(int amount)
        {
            if (amount <= 0) return true;
            if (_currentCurrency < amount) return false;

            _currentCurrency -= amount;
            _totalSpent += amount;
            NotifyCurrencyChanged(-amount);
            return true;
        }

        public bool CanAfford(int amount)
        {
            return _currentCurrency >= amount;
        }

        private void HandleWaveCompleted(int waveNumber)
        {
            // Wave completion bonus
            int waveBonus = _baseWaveBonus + (waveNumber * _waveBonusIncrement);
            AddCurrency(waveBonus);

            // Interest on saved currency
            if (_enableInterest)
            {
                int interest = Mathf.Min(
                    Mathf.FloorToInt(_currentCurrency * _interestRate),
                    _interestCap
                );
                if (interest > 0)
                {
                    AddCurrency(interest);
                    Debug.Log($"Interest earned: {interest}");
                }
            }

            Debug.Log($"Wave {waveNumber} bonus: {waveBonus}");
        }

        private void NotifyCurrencyChanged(int delta)
        {
            OnCurrencyChanged?.Invoke(_currentCurrency, delta);
            _onCurrencyChanged?.RaiseEvent(_currentCurrency);

            // Sync with GameManager
            if (GameManager.Instance != null)
            {
                // Use direct field sync or method
            }
        }

        public void Reset()
        {
            Initialize();
        }
    }
}
```

### Update GameManager Integration

GameManager should delegate to EconomyManager:

```csharp
// In GameManager.cs, update currency methods:
public int CurrentCurrency => EconomyManager.Instance?.CurrentCurrency ?? _currentCurrency;

public void ModifyCurrency(int delta)
{
    if (delta > 0)
        EconomyManager.Instance?.AddCurrency(delta);
    else if (delta < 0)
        EconomyManager.Instance?.TrySpend(-delta);
}

public bool TrySpendCurrency(int amount)
{
    return EconomyManager.Instance?.TrySpend(amount) ?? false;
}
```

### Economy Events for UI

Create currency changed event channel in ScriptableObjects/Events/ if not exists.

### Scene Setup

1. Create EconomyManager GameObject under --- MANAGEMENT ---
2. Configure starting currency and bonuses
3. Link event channels

## Testing and Acceptance Criteria

### Done When

- [ ] EconomyManager initializes with starting currency
- [ ] AddCurrency increases balance
- [ ] TrySpend deducts currency if affordable
- [ ] CanAfford check works correctly
- [ ] Wave completion grants bonus
- [ ] Interest calculated if enabled
- [ ] Events fire on currency changes
- [ ] Total earned/spent tracked

## Dependencies

- Issue 2: Game Manager
