using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class FloatVariableFormatter : MonoBehaviour
    {
        public FloatReference reference;
        public string formatString = "{0,6:P1} speed";

        public UnityEvent<string> TextUpdate;

        private void Awake()
        {
            reference.ValueChanges.TakeUntilDestroy(this)
                .Subscribe(next =>
                {
                    var newText = string.Format(formatString, next);
                    TextUpdate?.Invoke(newText);
                }).AddTo(this);
        }
    }
}
