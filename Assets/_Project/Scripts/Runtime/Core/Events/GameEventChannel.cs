using System;
using UnityEngine;

namespace TowerDefense.Core.Events
{
    /// <summary>
    /// A ScriptableObject-based event channel for parameterless events.
    /// Enables decoupled communication between systems without direct references.
    /// </summary>
    [CreateAssetMenu(fileName = "GameEventChannel", menuName = "TD/Events/Game Event Channel")]
    public class GameEventChannel : ScriptableObject
    {
        public event Action OnEventRaised;

        public void RaiseEvent()
        {
            OnEventRaised?.Invoke();
        }
    }
}
