using System;
using UnityEngine;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class BatchAssignerBoolean : MonoBehaviour
    {
        [SerializeField]
        AssignPair[] assignPairs;

        [Serializable]
        class AssignPair
        {
            public BooleanVariable target = null;
            public BooleanReference source = null;
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
