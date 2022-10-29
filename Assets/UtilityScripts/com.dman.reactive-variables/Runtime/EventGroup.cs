using System;
using UnityEngine;
using UnityEngine.Events;

namespace Dman.ReactiveVariables
{
    [CreateAssetMenu(fileName = "EventGroup", menuName = "DmanUtilities/ReactiveVariables/EventGroup", order = 10)]
    public class EventGroup : ScriptableObject
    {
        public event Action OnEvent;
        [SerializeField] UnityEvent Triggered;
        public bool LogEvents = true;
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif


        public void TriggerEvent()
        {
            if(LogEvents)
                Debug.Log($"${name} triggered {Time.frameCount}");
            OnEvent?.Invoke();
            Triggered?.Invoke();
        }
    }
}
