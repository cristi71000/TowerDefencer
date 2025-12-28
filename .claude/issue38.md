## Context

Boss enemies are massive, high-health targets that serve as wave capstones. They may have special abilities and require focused fire from multiple towers.

**Builds upon:** Issue 9 (Enemy systems)

## Detailed Implementation Instructions

### Boss Enemy Data

Create `ED_BossEnemy.asset`:
```
EnemyName: "Golem"
Type: Boss
MaxHealth: 2000 (20x basic)
MoveSpeed: 1.0 (very slow)
Armor: 10
CurrencyReward: 200
LivesDamage: 10
IsFlying: false
IsImmuneToSlow: false
SlowResistance: 0.75
ModelScale: 3.0 (3x size)
```

### Boss Enemy Component

Create `BossEnemy.cs`:

```csharp
using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Enemies
{
    public class BossEnemy : MonoBehaviour
    {
        [Header("Boss Settings")]
        [SerializeField] private bool _showBossHealthBar = true;

        [Header("Abilities")]
        [SerializeField] private float _abilityInterval = 10f;
        [SerializeField] private float _healAmount = 0.1f; // 10% heal
        [SerializeField] private GameObject _abilityVFX;

        private Enemy _enemy;
        private float _abilityTimer;

        public event System.Action<BossEnemy> OnBossSpawned;
        public event System.Action<BossEnemy> OnBossDefeated;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
        }

        private void Start()
        {
            OnBossSpawned?.Invoke(this);

            // Notify UI to show boss health bar
            BossHealthBarUI.Instance?.ShowBoss(this);
        }

        private void OnDestroy()
        {
            if (_enemy.IsDead)
            {
                OnBossDefeated?.Invoke(this);
            }
        }

        private void Update()
        {
            if (_enemy.IsDead) return;

            _abilityTimer += Time.deltaTime;
            if (_abilityTimer >= _abilityInterval)
            {
                _abilityTimer = 0f;
                UseAbility();
            }
        }

        private void UseAbility()
        {
            // Example ability: Self-heal
            int healAmount = Mathf.RoundToInt(_enemy.MaxHealth * _healAmount);
            // Would need Heal method in Enemy.cs

            // VFX
            if (_abilityVFX != null)
            {
                Instantiate(_abilityVFX, transform.position, Quaternion.identity);
            }

            Debug.Log($"Boss used ability!");
        }
    }
}
```

### Boss Health Bar UI

Create `BossHealthBarUI.cs`:

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerDefense.UI
{
    public class BossHealthBarUI : MonoBehaviour
    {
        public static BossHealthBarUI Instance { get; private set; }

        [SerializeField] private GameObject _bossPanel;
        [SerializeField] private Image _healthFill;
        [SerializeField] private TextMeshProUGUI _bossNameText;
        [SerializeField] private TextMeshProUGUI _healthText;

        private BossEnemy _currentBoss;
        private Enemy _bossEnemy;

        private void Awake()
        {
            Instance = this;
            _bossPanel.SetActive(false);
        }

        public void ShowBoss(BossEnemy boss)
        {
            _currentBoss = boss;
            _bossEnemy = boss.GetComponent<Enemy>();

            _bossNameText.text = _bossEnemy.Data.EnemyName;
            _bossPanel.SetActive(true);

            _bossEnemy.OnHealthChanged += UpdateHealthBar;
            _bossEnemy.OnDeath += HideBoss;
        }

        private void UpdateHealthBar(Enemy enemy, int current, int max)
        {
            _healthFill.fillAmount = (float)current / max;
            _healthText.text = $"{current} / {max}";
        }

        private void HideBoss(Enemy enemy)
        {
            _bossEnemy.OnHealthChanged -= UpdateHealthBar;
            _bossEnemy.OnDeath -= HideBoss;
            _bossPanel.SetActive(false);
            _currentBoss = null;
        }
    }
}
```

### Boss Enemy Prefab

Create `BossEnemy` prefab:

```
BossEnemy (Enemy.cs, BossEnemy.cs, NavMeshAgent)
|-- Model
|   |-- Body (Large cube/humanoid shape)
|   |-- Details (armor, spikes, etc.)
|   +-- AbilityIndicator (for ability cast visual)
+-- HealthBarAnchor
```

### Boss Spawn Announcement

Create `BossAnnouncement.cs`:

```csharp
public class BossAnnouncement : MonoBehaviour
{
    [SerializeField] private WaveAnnouncement _announcement;

    private void Start()
    {
        EnemySpawner.Instance.OnEnemySpawned += CheckForBoss;
    }

    private void CheckForBoss(Enemy enemy)
    {
        if (enemy.Data.Type == EnemyType.Boss)
        {
            _announcement?.ShowAnnouncement("BOSS INCOMING!");
        }
    }
}
```

### Boss Material

Create `M_Enemy_Boss.mat`:
- Color: Dark red RGB(100, 20, 20)
- Metallic with glowing accents
- Consider emissive for intimidation

## Testing and Acceptance Criteria

### Done When

- [ ] BossEnemy data asset created
- [ ] BossEnemy prefab with imposing visual
- [ ] 3x scale makes boss visually distinct
- [ ] Boss health bar appears in UI
- [ ] High health requires sustained damage
- [ ] High slow resistance
- [ ] Ability system triggers periodically
- [ ] Boss announcement on spawn
- [ ] Large currency reward on defeat

## Dependencies

- Issue 9: Enemy systems
