using System.Linq;
using System.Text.RegularExpressions;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

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
                .StartWith(reference.CurrentValue)
                .Subscribe(next =>
                {
                    this.FormatFloat(next);
                }).AddTo(this);
        }

        public void FormatFloat(float value)
        {
            if(reference.DataSource == ReferenceDataSource.CONSTANT)
            {
                reference.ConstantValue = value;
            }
            var newText = string.Format(formatString, value);
            TextUpdate?.Invoke(newText);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            TextUpdate = new UnityEvent<string>();
            var allComponents = gameObject.GetComponents(typeof(MonoBehaviour));
            foreach (var comp in allComponents)
            {
                var componentType = comp.GetType();
                var componentTextProperty = componentType.GetProperties()
                    .Where(x => x.PropertyType == typeof(string) && x.Name == "text" && x.CanWrite && x.SetMethod.IsPublic)
                    .FirstOrDefault();

                if (componentTextProperty == null)
                    continue;


                var textSetMethod = componentTextProperty.GetSetMethod().CreateDelegate(typeof(UnityAction<string>), comp) as UnityAction<string>;

                UnityEditor.Events.UnityEventTools.AddPersistentListener<string>(
                    TextUpdate,
                    textSetMethod);

                var existingText = componentTextProperty.GetMethod.Invoke(comp, new object[0]) as string;
                var numberMatch = new Regex(@"[-\d.]+").Match(existingText);
                if (numberMatch != null)
                {
                    var myText = existingText.Substring(0, numberMatch.Index) + "{0,6:F1}" + existingText.Substring(numberMatch.Index + numberMatch.Length);
                    formatString = myText;
                }
                break;
            }
        }
#endif
    }
}
