using System;
using UniRx;
using UnityEngine;

namespace Dman.ReactiveVariables
{
    public abstract class GenericVariable<T> : ScriptableObject
    {
        [SerializeField]
        [Multiline]
#pragma warning disable 0414 // unused value
        private string DeveloperDescription = "";
#pragma warning restore 0414
        [SerializeField]
        private T InspectableValue;
        private BehaviorSubject<T> _value = new BehaviorSubject<T>(default);
        public BehaviorSubject<T> Value => _value;

        private IDisposable subscribtion;
        void OnEnable()
        {
            SetValueFromInspectable();
            subscribtion = Value.Subscribe(x => InspectableValue = x);
        }

        void OnDisable()
        {
            subscribtion?.Dispose();
            subscribtion = null;
        }

        void OnValidate()
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
