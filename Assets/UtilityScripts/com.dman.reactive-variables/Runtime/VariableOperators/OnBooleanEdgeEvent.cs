using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class OnBooleanEdgeEvent : MonoBehaviour
    {
        public BooleanReference booleanToWatch;
        public bool whichEdge;

        public UnityEvent OnTransitionToEdge;

        private void Awake()
        {
            booleanToWatch.ValueChanges.TakeUntilDisable(this)
                .Pairwise()
                .Subscribe(pair =>
                {
                    if (pair.Current == whichEdge && pair.Current != pair.Previous)
                    {
                        OnTransitionToEdge.Invoke();
                    }
                }).AddTo(this);
        }
    }
}
