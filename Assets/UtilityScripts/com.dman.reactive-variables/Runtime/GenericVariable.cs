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

        public BehaviorSubject<T> Value = new BehaviorSubject<T>(default);

        public void SetValue(T value)
        {
            Value.OnNext(value);
        }

        public T CurrentValue => Value.Value;
    }
}
