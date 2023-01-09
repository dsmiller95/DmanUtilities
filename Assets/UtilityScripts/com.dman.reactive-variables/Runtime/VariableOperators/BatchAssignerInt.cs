using System;
using UnityEngine;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class BatchAssignerInt : MonoBehaviour
    {
        [SerializeField]
        AssignPair[] assignPairs;

        [Serializable]
        class AssignPair
        {
            public IntVariable target = null;
            public IntReference source = null;

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
