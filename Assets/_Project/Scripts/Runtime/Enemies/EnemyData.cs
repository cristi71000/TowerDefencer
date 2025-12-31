using UnityEngine;

namespace TowerDefense.Enemies
{
    public enum EnemyType
    {
        Basic,      // Standard ground enemy
        Fast,       // Quick but fragile
        Tank,       // Slow but tough
        Flying,     // Ignores ground obstacles
        Boss        // Powerful mini-boss
    }

    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "TD/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string EnemyName;
        [TextArea(2, 4)]
        public string Description;
        public Sprite Icon;
        public EnemyType Type;

        [Header("Prefab")]
        public GameObject Prefab;

        [Header("Stats")]
        [Min(1)]
        public int MaxHealth = 100;
        [Min(0.1f)]
        public float MoveSpeed = 3f;
        [Min(0)]
        public int Damage = 1;

        [Header("Economy")]
        [Min(0)]
        public int KillReward = 10;

        [Header("Audio/Visual")]
        public AudioClip DeathSound;
        public AudioClip HitSound;
        public GameObject DeathVFXPrefab;

        public bool IsFlying => Type == EnemyType.Flying;
        public bool IsBoss => Type == EnemyType.Boss;
    }
}
