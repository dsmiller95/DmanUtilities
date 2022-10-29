using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class IntVariableChangedToUnityEvent : MonoBehaviour
    {
        public IntReference reference;

        public UnityEvent<int> OnChanged;
        private void Awake()
        {
            reference.ValueChanges
                .DistinctUntilChanged()
                .TakeUntilDisable(this)
                .Subscribe(pair =>
                {
                    OnChanged.Invoke(pair);
                }).AddTo(this);
        }
    }
}
