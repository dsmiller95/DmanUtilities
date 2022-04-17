using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;

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

#if UNITY_EDITOR
        private void Reset()
        {
            TextUpdate = new UnityEvent<string>();
            var allComponents = gameObject.GetComponents(typeof(MonoBehaviour));
            foreach (var comp in allComponents)
            {
                var componentType = comp.GetType();
                var componentTextProperty = componentType.GetProperties()
                    .Where(x => x.PropertyType == typeof(string) && x.Name == "text" && x.CanWrite && x.SetMethod.IsPublic && x.CanRead)
                    .FirstOrDefault();

                if (componentTextProperty == null)
                    continue;


                var textSetMethod = componentTextProperty.SetMethod.CreateDelegate(typeof(UnityAction<string>), comp) as UnityAction<string>;

                UnityEditor.Events.UnityEventTools.AddPersistentListener<string>(
                    TextUpdate,
                    textSetMethod);

                var existingText = componentTextProperty.GetMethod.Invoke(comp, new object[0]) as string;
                var numberMatch = new Regex(@"\d+").Match(existingText);
                if(numberMatch != null)
                {
                    var myText = existingText.Substring(0, numberMatch.Index) + "{0,3:D}" + existingText.Substring(numberMatch.Index + numberMatch.Length);
                    formatString = myText;
                }
                break;
            }
        }
#endif
    }
}
