using System;
using UniRx;
using UnityEngine;

namespace Dman.ReactiveVariables
{
    public abstract class GenericVariable<T> : ScriptableObject
    {
        [Multiline]
        public string DeveloperDescription = "";
        [SerializeField]
        private T InspectableValue;
        private BehaviorSubject<T> _value = new BehaviorSubject<T>(default);
        public BehaviorSubject<T> Value => _value;

        private IDisposable subscribtion;
        public void OnEnable()
        {
            SetValueFromInspectable();
            subscribtion = Value.Subscribe(x => InspectableValue = x);
        }

        public void OnDisable()
        {
            subscribtion?.Dispose();
            subscribtion = null;
        }

        public void OnValidate()
        {
            SetValueFromInspectable();
        }

        private void SetValueFromInspectable()
        {
            if (InspectableValue is UnityEngine.Object && (InspectableValue as UnityEngine.Object) == null)
            {
                // special case to handle UnityEngine.Object , which will not -actually- be null but will behave as nulls
                SetValue(default);
            }
            else
            {
                SetValue(InspectableValue);
            }
        }

        public void SetValue(T value)
        {
            Value.OnNext(value);
        }

        /// <summary>
        /// set the value to the C# defined default of this type.
        /// Useful for binding to unity events, since it is not possible to define "null" in a gameobject variable slot
        /// </summary>
        public void ClearToDefaultValue()
        {
            SetValue(default(T));
        }

        public T CurrentValue { get => Value.Value; set => SetValue(value); }
    }
}
