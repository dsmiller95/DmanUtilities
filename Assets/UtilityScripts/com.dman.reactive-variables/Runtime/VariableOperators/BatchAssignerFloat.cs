using System;
using UnityEngine;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class BatchAssignerFloat : MonoBehaviour
    {
        [SerializeField]
        AssignPair[] assignPairs = null;

        [Serializable]
        class AssignPair
        {
            public FloatVariable target = null;
            public FloatReference source = null;

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
