using UnityEngine;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class BooleanToggle : MonoBehaviour
    {
        public BooleanReference variableToToggle;

        public void Toggle()
        {
            variableToToggle.SetValue(!variableToToggle.CurrentValue);
        }
    }
}
