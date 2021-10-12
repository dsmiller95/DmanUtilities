using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class FloatVariableIncrementor : MonoBehaviour
    {
        public FloatVariable variable;
        public float incrementPerSecond;

        private void Update()
        {
            variable.Add(Time.deltaTime * incrementPerSecond);
        }
    }
}
