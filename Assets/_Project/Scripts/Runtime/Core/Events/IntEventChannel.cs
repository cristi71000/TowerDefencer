using System;
using UnityEngine;

namespace TowerDefense.Core.Events
{
    /// <summary>
    /// A ScriptableObject-based event channel for int parameter events.
    /// Enables decoupled communication between systems without direct references.
    /// </summary>
    [CreateAssetMenu(fileName = "IntEventChannel", menuName = "TD/Events/Int Event Channel")]
    public class IntEventChannel : ScriptableObject
    {
        public event Action<int> OnEventRaised;

        public void RaiseEvent(int value)
        {
            OnEventRaised?.Invoke(value);
        }
    }
}
