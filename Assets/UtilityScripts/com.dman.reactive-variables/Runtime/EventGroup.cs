using System;
using UnityEngine;

namespace Dman.ReactiveVariables
{
    [CreateAssetMenu(fileName = "EventGroup", menuName = "DmanUtilities/ReactiveVariables/EventGroup", order = 10)]
    public class EventGroup : ScriptableObject
    {
        public event Action OnEvent;
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif


        public void TriggerEvent()
        {
            Debug.Log($"${name} triggered");
            OnEvent?.Invoke();
        }
    }
}
