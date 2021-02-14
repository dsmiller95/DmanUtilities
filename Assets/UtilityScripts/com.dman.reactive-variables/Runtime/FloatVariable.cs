using UnityEngine;

namespace Dman.ReactiveVariables
{
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "DmanUtilities/ReactiveVariables/FloatVariable", order = 2)]
    public class FloatVariable : GenericVariable<float>
    {
        public float Add(float extraValue)
        {
            var newValue = CurrentValue + extraValue;
            SetValue(newValue);
            return newValue;
        }

    }
}
