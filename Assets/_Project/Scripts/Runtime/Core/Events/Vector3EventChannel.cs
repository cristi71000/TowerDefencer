using System;
using UnityEngine;

namespace TowerDefense.Core.Events
{
    /// <summary>
    /// A ScriptableObject-based event channel for Vector3 parameter events.
    /// Enables decoupled communication between systems without direct references.
    /// </summary>
    [CreateAssetMenu(fileName = "Vector3EventChannel", menuName = "TD/Events/Vector3 Event Channel")]
    public class Vector3EventChannel : ScriptableObject
    {
        public event Action<Vector3> OnEventRaised;

        public void RaiseEvent(Vector3 value)
        {
            OnEventRaised?.Invoke(value);
        }
    }
}
