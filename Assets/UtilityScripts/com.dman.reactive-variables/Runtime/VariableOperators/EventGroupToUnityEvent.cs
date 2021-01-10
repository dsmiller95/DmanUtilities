using UnityEngine;
using UnityEngine.Events;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class EventGroupToUnityEvent : MonoBehaviour
    {
        public EventGroup eventGroup;
        public bool OnlyTriggerIfActive = true;

        public UnityEvent OnChanged;
        private void Awake()
        {
            eventGroup.OnEvent += OnEvent;
        }
        private void OnDestroy()
        {
            eventGroup.OnEvent -= OnEvent;
        }

        private void OnEvent()
        {
            if (!OnlyTriggerIfActive || gameObject.activeInHierarchy)
            {
                OnChanged.Invoke();
            }
        }
    }
}
