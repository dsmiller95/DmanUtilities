using UnityEngine;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class FloatVariableIncrementor : MonoBehaviour
    {
        public FloatVariable variable;
        public FloatReference incrementPerSecond;

        private void Update()
        {
            variable.Add(Time.deltaTime * incrementPerSecond);
        }
    }
}
