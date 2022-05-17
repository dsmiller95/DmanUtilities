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
            public IntVariable target;
            public IntReference source;

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
