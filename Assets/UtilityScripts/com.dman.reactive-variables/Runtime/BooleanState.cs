using UnityEngine;

namespace Dman.ReactiveVariables
{
    [CreateAssetMenu(fileName = "BooleanState", menuName = "ReactiveVariables/BooleanState", order = 2)]
    public class BooleanState : GenericState<bool>
    {
        public bool defaultState;
        public override GenericVariable<bool> GenerateNewVariable()
        {
            var instanced = CreateInstance<BooleanVariable>();
            instanced.SetValue(defaultState);
            return instanced;
        }

        public override object GetSaveObjectFromVariable(GenericVariable<bool> variable)
        {
            return variable.CurrentValue;
        }

        public override void SetSaveObjectIntoVariable(GenericVariable<bool> variable, object savedValue)
        {
            var saveValue = (bool)savedValue;
            variable.SetValue(saveValue);
        }
    }
}
