using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class IntVariableComparisonToUnityEvent : MonoBehaviour
    {
        public IntReference valueA;
        public IntReference valueB;

        public UnityEvent OnAGreaterThanB;
        public UnityEvent OnAEqualsB;
        public UnityEvent OnALessThanB;

        private int lastComparisonResult;

        private void Awake()
        {
            valueA.ValueChanges.TakeUntilDestroy(this)
                .Merge(valueB.ValueChanges)
                .StartWith(0)
                .Subscribe(valueUpdate =>
                {
                    var comparisonResult = -1;
                    if (valueA.CurrentValue > valueB.CurrentValue)
                    {
                        comparisonResult = 0;
                    }
                    else if (valueA.CurrentValue >= valueB.CurrentValue)
                    {
                        comparisonResult = 1;
                    }
                    else
                    {
                        comparisonResult = 2;
                    }

                    if(comparisonResult == lastComparisonResult)
                    {
                        return;
                    }
                    lastComparisonResult = comparisonResult;
                    switch (comparisonResult)
                    {
                        case 0:
                            OnAGreaterThanB.Invoke();
                            break;
                        case 1:
                            OnAEqualsB.Invoke();
                            break;
                        case 2:
                            OnALessThanB.Invoke();
                            break;
                        default:
                            break;
                    }
                }).AddTo(this);
        }

    }
}
