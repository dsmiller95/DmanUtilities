using UnityEngine;

namespace Dman.ReactiveVariables
{
    [CreateAssetMenu(fileName = "IntVariable", menuName = "ReactiveVariables/IntVariable", order = 2)]
    public class IntVariable : GenericVariable<int>
    {
        public float Add(int extraValue)
        {
            var newValue = CurrentValue + extraValue;
            SetValue(newValue);
            return newValue;
        }
    }
}
