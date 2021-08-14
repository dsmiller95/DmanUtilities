using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class IntVariableFormatter : MonoBehaviour
    {
        public IntReference reference;
        public string formatString = "{0,3:D} speed";

        public UnityEvent<string> TextUpdate;

        private void Awake()
        {
            reference.ValueChanges.TakeUntilDestroy(this)
                .StartWith(reference.CurrentValue)
                .Subscribe(next =>
                {
                    var newText = string.Format(formatString, next);
                    TextUpdate?.Invoke(newText);
                }).AddTo(this);
        }
    }
}
