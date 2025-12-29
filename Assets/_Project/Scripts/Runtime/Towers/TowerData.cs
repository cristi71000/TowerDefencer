using UnityEngine;

namespace TowerDefense.Towers
{
    public enum TargetingPriority
    {
        First,      // Closest to exit
        Nearest,    // Closest to tower
        Strongest,  // Highest current health
        Weakest,    // Lowest current health
        Fastest     // Highest speed
    }

    public enum TowerType
    {
        Basic,      // Single target, balanced
        AOE,        // Area damage
        Slow,       // Applies slow effect
        Sniper,     // High damage, slow fire rate
        Support     // Buffs nearby towers
    }

    [CreateAssetMenu(fileName = "NewTowerData", menuName = "TD/Tower Data")]
    public class TowerData : ScriptableObject
    {
        [Header("Identity")]
        public string TowerName;
        public string Description;
        public Sprite Icon;
        public TowerType Type;

        [Header("Prefab")]
        public GameObject Prefab;

        [Header("Economy")]
        public int PurchaseCost;
        public int SellValue;

        [Header("Combat Stats")]
        public float Range = 5f;
        public float AttackSpeed = 1f;
        public int Damage = 10;
        public TargetingPriority DefaultPriority = TargetingPriority.First;

        [Header("Projectile")]
        public GameObject ProjectilePrefab;
        public float ProjectileSpeed = 10f;

        [Header("Special Effects")]
        public float SlowAmount = 0f;
        public float SlowDuration = 0f;
        public float AOERadius = 0f;
        public float BuffRadius = 0f;
        public float BuffAmount = 0f;

        [Header("Upgrades")]
        public TowerData[] UpgradesTo;
        public TowerData UpgradesFrom;

        [Header("Audio/Visual")]
        public AudioClip AttackSound;
        public GameObject MuzzleFlashPrefab;

        public float AttackInterval => 1f / AttackSpeed;
        public bool IsAOE => AOERadius > 0;
        public bool HasSlowEffect => SlowAmount > 0 && SlowDuration > 0;
        public bool IsSupportTower => Type == TowerType.Support;
    }
}
