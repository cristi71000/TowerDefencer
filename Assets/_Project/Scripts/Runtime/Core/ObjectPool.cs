using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Generic object pool for efficient object reuse.
    /// Supports prewarming and automatic expansion.
    /// </summary>
    /// <typeparam name="T">The component type to pool</typeparam>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _inactive;
        private readonly List<T> _active;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onReturn;

        /// <summary>
        /// Number of objects currently in the pool (inactive).
        /// </summary>
        public int InactiveCount => _inactive.Count;

        /// <summary>
        /// Number of objects currently in use (active).
        /// </summary>
        public int ActiveCount => _active.Count;

        /// <summary>
        /// Total number of objects managed by this pool.
        /// </summary>
        public int TotalCount => _inactive.Count + _active.Count;

        /// <summary>
        /// Creates a new object pool.
        /// </summary>
        /// <param name="prefab">The prefab to instantiate</param>
        /// <param name="parent">Optional parent transform for pooled objects</param>
        /// <param name="initialSize">Number of objects to prewarm</param>
        /// <param name="onGet">Optional callback when object is retrieved from pool</param>
        /// <param name="onReturn">Optional callback when object is returned to pool</param>
        public ObjectPool(T prefab, Transform parent = null, int initialSize = 0, 
            Action<T> onGet = null, Action<T> onReturn = null)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab), "ObjectPool requires a non-null prefab.");
            }

            _prefab = prefab;
            _parent = parent;
            _inactive = new Stack<T>(initialSize > 0 ? initialSize : 8);
            _active = new List<T>(initialSize > 0 ? initialSize : 8);
            _onGet = onGet;
            _onReturn = onReturn;

            // Prewarm the pool
            Prewarm(initialSize);
        }

        /// <summary>
        /// Prewarms the pool by creating inactive instances.
        /// </summary>
        /// <param name="count">Number of instances to create</param>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T instance = CreateInstance();
                instance.gameObject.SetActive(false);
                _inactive.Push(instance);
            }
        }

        /// <summary>
        /// Gets an object from the pool, creating a new one if necessary.
        /// </summary>
        /// <returns>An active pooled object</returns>
        public T Get()
        {
            T instance;

            if (_inactive.Count > 0)
            {
                instance = _inactive.Pop();
            }
            else
            {
                instance = CreateInstance();
            }

            instance.gameObject.SetActive(true);
            _active.Add(instance);
            _onGet?.Invoke(instance);

            return instance;
        }

        /// <summary>
        /// Gets an object from the pool and positions it.
        /// </summary>
        /// <param name="position">World position</param>
        /// <param name="rotation">World rotation</param>
        /// <returns>An active pooled object</returns>
        public T Get(Vector3 position, Quaternion rotation)
        {
            T instance = Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        /// <param name="instance">The object to return</param>
        public void Return(T instance)
        {
            if (instance == null)
            {
                Debug.LogWarning("ObjectPool.Return called with null instance.");
                return;
            }

            if (!_active.Remove(instance))
            {
                Debug.LogWarning($"ObjectPool.Return: Instance {instance.name} was not tracked as active.");
            }

            _onReturn?.Invoke(instance);
            instance.gameObject.SetActive(false);
            
            if (_parent != null)
            {
                instance.transform.SetParent(_parent);
            }

            _inactive.Push(instance);
        }

        /// <summary>
        /// Returns all active objects to the pool.
        /// </summary>
        public void ReturnAll()
        {
            // Iterate backwards to avoid issues with list modification
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                T instance = _active[i];
                _onReturn?.Invoke(instance);
                instance.gameObject.SetActive(false);
                
                if (_parent != null)
                {
                    instance.transform.SetParent(_parent);
                }

                _inactive.Push(instance);
            }

            _active.Clear();
        }

        /// <summary>
        /// Destroys all pooled objects and clears the pool.
        /// </summary>
        public void Clear()
        {
            foreach (T instance in _active)
            {
                if (instance != null)
                {
                    UnityEngine.Object.Destroy(instance.gameObject);
                }
            }

            foreach (T instance in _inactive)
            {
                if (instance != null)
                {
                    UnityEngine.Object.Destroy(instance.gameObject);
                }
            }

            _active.Clear();
            _inactive.Clear();
        }

        private T CreateInstance()
        {
            T instance = UnityEngine.Object.Instantiate(_prefab, _parent);
            instance.name = $"{_prefab.name}_Pooled";
            return instance;
        }
    }
}
