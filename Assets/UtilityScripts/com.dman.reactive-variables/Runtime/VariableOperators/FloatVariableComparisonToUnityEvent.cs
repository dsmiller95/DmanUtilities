using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class FloatVariableComparisonToUnityEvent : MonoBehaviour
    {
        public FloatReference valueA;
        public FloatReference valueB;

        public UnityEvent OnAGreaterThanB;
        public UnityEvent OnALessThanB;

        private bool lastComparisonResult;

        private void Awake()
        {
            valueA.ValueChanges.TakeUntilDestroy(this)
                .Merge(valueB.ValueChanges)
                .StartWith(0)
                .Subscribe(valueUpdate =>
                {
                    var AGreaterThanB = valueA.CurrentValue > valueB.CurrentValue;
                    if (lastComparisonResult != AGreaterThanB) {
                        if (AGreaterThanB)
                        {
                            OnAGreaterThanB.Invoke();
                        }else
                        {
                            OnALessThanB.Invoke();
                        }
                    }
                    lastComparisonResult = AGreaterThanB;
                }).AddTo(this);
        }

    }
}
