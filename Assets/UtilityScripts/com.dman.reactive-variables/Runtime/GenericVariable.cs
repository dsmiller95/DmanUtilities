using UniRx;
using UnityEngine;

namespace Dman.ReactiveVariables
{
    public abstract class GenericVariable<T> : ScriptableObject
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif
        private BehaviorSubject<T> _value = new BehaviorSubject<T>(default);
        public BehaviorSubject<T> Value => _value;

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
