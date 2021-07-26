using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class FloatVariableChangedToUnityEvent : MonoBehaviour
    {
        public FloatReference reference;

        public UnityEvent<float> OnChanged;
        private void Awake()
        {
            reference.ValueChanges.TakeUntilDisable(this)
                .Subscribe(pair =>
                {
                    OnChanged.Invoke(pair);
                }).AddTo(this);
        }
    }
}
