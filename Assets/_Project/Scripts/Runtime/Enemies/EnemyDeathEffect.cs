using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// Component to spawn death VFX when an enemy dies.
    /// Attach to enemy prefabs to enable death visual effects.
    /// </summary>
    public class EnemyDeathEffect : MonoBehaviour
    {
        [Header("VFX Settings")]
        [SerializeField] private GameObject _deathVFXPrefab;
        [SerializeField] private float _vfxDuration = 2f;
        [SerializeField] private bool _scaleWithEnemy = true;

        /// <summary>
        /// The VFX prefab to spawn on death.
        /// </summary>
        public GameObject DeathVFXPrefab => _deathVFXPrefab;

        /// <summary>
        /// How long the VFX lasts before being destroyed.
        /// </summary>
        public float VFXDuration => _vfxDuration;

        /// <summary>
        /// Whether to scale the VFX with the enemy's size.
        /// </summary>
        public bool ScaleWithEnemy => _scaleWithEnemy;

        /// <summary>
        /// Plays the death effect at the specified position with optional scale.
        /// </summary>
        /// <param name="position">World position to spawn the VFX.</param>
        /// <param name="scale">Scale multiplier for the VFX (used if ScaleWithEnemy is true).</param>
        public void PlayDeathEffect(Vector3 position, float scale = 1f)
        {
            if (_deathVFXPrefab == null)
            {
                return;
            }

            GameObject vfxInstance = Instantiate(_deathVFXPrefab, position, Quaternion.identity);

            if (_scaleWithEnemy && scale != 1f)
            {
                vfxInstance.transform.localScale *= scale;
            }

            // Destroy the VFX after the specified duration
            Destroy(vfxInstance, _vfxDuration);
        }
    }
}
