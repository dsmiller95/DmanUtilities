using UnityEngine;

namespace Dman.ReactiveVariables
{
    [CreateAssetMenu(fileName = "ScriptableObjectVariable", menuName = "DmanUtilities/ReactiveVariables/ScriptableObjectVariable", order = 1)]
    public class ScriptableObjectVariable : GenericVariable<ScriptableObject>
    {
        /// <summary>
        /// Set the value of this variable to <paramref name="value"/>. If it is already set to the same value,
        /// Reset to null.
        /// </summary>
        /// <param name="value"></param>
        public void ToggleValue(ScriptableObject value)
        {
            if (CurrentValue == value)
            {
                ClearToDefaultValue();
            }
            else
            {
                SetValue(value);
            }
        }
    }
}
