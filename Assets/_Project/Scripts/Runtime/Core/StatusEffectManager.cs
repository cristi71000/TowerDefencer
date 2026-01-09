using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// MonoBehaviour component that manages status effects on an enemy.
    /// Handles adding, updating, and removing effects with proper stacking/refresh logic.
    /// Attach this to enemy prefabs alongside the Enemy component.
    /// </summary>
    public class StatusEffectManager : MonoBehaviour
    {
        private readonly List<StatusEffect> _activeEffects = new List<StatusEffect>();
        private TowerDefense.Enemies.Enemy _enemy;

        /// <summary>
        /// Event fired when a status effect is added.
        /// </summary>
        public event Action<StatusEffect> OnEffectAdded;

        /// <summary>
        /// Event fired when a status effect is removed.
        /// </summary>
        public event Action<StatusEffect> OnEffectRemoved;

        /// <summary>
        /// The number of currently active effects.
        /// </summary>
        public int ActiveEffectCount => _activeEffects.Count;

        private void Awake()
        {
            _enemy = GetComponent<TowerDefense.Enemies.Enemy>();

            if (_enemy == null)
            {
                UnityEngine.Debug.LogError($"StatusEffectManager requires an Enemy component on {gameObject.name}");
            }
        }

        private void OnEnable()
        {
            // Subscribe to enemy death to clear effects
            if (_enemy != null)
            {
                _enemy.OnDeath += HandleEnemyDeath;
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from enemy death
            if (_enemy != null)
            {
                _enemy.OnDeath -= HandleEnemyDeath;
            }

            // Clear all effects when disabled
            ClearAllEffects();
        }

        private void Update()
        {
            if (_enemy == null || _enemy.IsDead)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            // Update effects in reverse order so we can safely remove expired ones
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = _activeEffects[i];

                // Update the effect
                effect.Update(_enemy, deltaTime);

                // Remove expired effects
                if (effect.IsExpired)
                {
                    RemoveEffectAt(i);
                }
            }
        }

        /// <summary>
        /// Adds a status effect to this enemy.
        /// Handles stacking vs refresh logic based on the effect's CanStack property.
        /// </summary>
        /// <param name="effect">The effect to add.</param>
        public void AddEffect(StatusEffect effect)
        {
            if (effect == null)
            {
                UnityEngine.Debug.LogWarning($"StatusEffectManager.AddEffect: Cannot add null effect to {gameObject.name}");
                return;
            }

            if (_enemy == null || _enemy.IsDead)
            {
                return;
            }

            Type effectType = effect.GetType();

            if (effect.CanStack)
            {
                // Stackable effects are always added
                _activeEffects.Add(effect);
                effect.Apply(_enemy);
                OnEffectAdded?.Invoke(effect);
            }
            else
            {
                // Non-stackable effects refresh existing ones
                StatusEffect existingEffect = FindEffect(effectType);

                if (existingEffect != null)
                {
                    // Refresh the existing effect
                    existingEffect.Refresh(effect.Duration);

                    // For SlowEffect, also update the slow amount if stronger
                    if (existingEffect is SlowEffect existingSlow && effect is SlowEffect newSlow)
                    {
                        existingSlow.UpdateSlowParameters(newSlow.SlowAmount, newSlow.Duration);
                    }
                }
                else
                {
                    // No existing effect, add the new one
                    _activeEffects.Add(effect);
                    effect.Apply(_enemy);
                    OnEffectAdded?.Invoke(effect);
                }
            }
        }

        /// <summary>
        /// Removes all effects of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of effect to remove.</typeparam>
        public void RemoveEffect<T>() where T : StatusEffect
        {
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                if (_activeEffects[i] is T)
                {
                    RemoveEffectAt(i);
                }
            }
        }

        /// <summary>
        /// Removes a specific effect instance.
        /// </summary>
        /// <param name="effect">The effect instance to remove.</param>
        public void RemoveEffect(StatusEffect effect)
        {
            if (effect == null)
            {
                return;
            }

            int index = _activeEffects.IndexOf(effect);
            if (index >= 0)
            {
                RemoveEffectAt(index);
            }
        }

        /// <summary>
        /// Checks if the enemy has an effect of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of effect to check for.</typeparam>
        /// <returns>True if the enemy has at least one effect of the specified type.</returns>
        public bool HasEffect<T>() where T : StatusEffect
        {
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                if (_activeEffects[i] is T)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the first effect of the specified type, or null if none exists.
        /// </summary>
        /// <typeparam name="T">The type of effect to find.</typeparam>
        /// <returns>The first effect of the specified type, or null.</returns>
        public T GetEffect<T>() where T : StatusEffect
        {
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                if (_activeEffects[i] is T typedEffect)
                {
                    return typedEffect;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all effects of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of effects to find.</typeparam>
        /// <returns>A list of all effects of the specified type.</returns>
        public List<T> GetAllEffects<T>() where T : StatusEffect
        {
            List<T> results = new List<T>();
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                if (_activeEffects[i] is T typedEffect)
                {
                    results.Add(typedEffect);
                }
            }
            return results;
        }

        /// <summary>
        /// Clears all active effects from this enemy.
        /// Call this when the enemy dies or is returned to the pool.
        /// </summary>
        public void ClearAllEffects()
        {
            // Remove in reverse order to properly clean up
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = _activeEffects[i];
                effect.Remove(_enemy);
                OnEffectRemoved?.Invoke(effect);
            }

            _activeEffects.Clear();
        }

        /// <summary>
        /// Resets the manager for object pool reuse.
        /// Clears all effects and event subscribers to prevent stale references.
        /// </summary>
        public void ResetManager()
        {
            ClearAllEffects();

            // Clear event subscribers to avoid stale references when using object pooling
            OnEffectAdded = null;
            OnEffectRemoved = null;
        }

        /// <summary>
        /// Finds an existing effect of the specified type.
        /// </summary>
        /// <param name="effectType">The type of effect to find.</param>
        /// <returns>The first effect of the specified type, or null.</returns>
        private StatusEffect FindEffect(Type effectType)
        {
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                if (_activeEffects[i].GetType() == effectType)
                {
                    return _activeEffects[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Removes an effect at the specified index.
        /// </summary>
        /// <param name="index">The index of the effect to remove.</param>
        private void RemoveEffectAt(int index)
        {
            if (index < 0 || index >= _activeEffects.Count)
            {
                return;
            }

            StatusEffect effect = _activeEffects[index];
            effect.Remove(_enemy);
            _activeEffects.RemoveAt(index);
            OnEffectRemoved?.Invoke(effect);
        }

        /// <summary>
        /// Handles the enemy death event by clearing all effects.
        /// </summary>
        /// <param name="enemy">The enemy that died.</param>
        private void HandleEnemyDeath(TowerDefense.Enemies.Enemy enemy)
        {
            ClearAllEffects();
        }
    }
}
