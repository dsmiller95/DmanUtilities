using UnityEngine;

namespace Dman.ReactiveVariables
{
    [CreateAssetMenu(fileName = "IntVariable", menuName = "DmanUtilities/ReactiveVariables/IntVariable", order = 2)]
    public class IntVariable : GenericVariable<int>
    {
        public void Add(int extraValue)
        {
            var newValue = CurrentValue + extraValue;
            SetValue(newValue);
        }
    }
}
