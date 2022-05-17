using System;
using UnityEngine;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class BatchAssignerFloat : MonoBehaviour
    {
        [SerializeField]
        AssignPair[] assignPairs;

        [Serializable]
        class AssignPair
        {
            public FloatVariable target;
            public FloatReference source;

            public void Assign()
            {
                target.SetValue(source.CurrentValue);
            }
        }

        public void AssignAll()
        {
            foreach (var pair in assignPairs)
            {
                pair.Assign();
            }
        }
    }
}
