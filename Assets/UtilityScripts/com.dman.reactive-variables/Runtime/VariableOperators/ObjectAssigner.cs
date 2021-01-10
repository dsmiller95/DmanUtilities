using UnityEngine;

namespace Dman.ReactiveVariables.VariableOperators
{
    public class ObjectAssigner : MonoBehaviour
    {
        public GameObjectVariable variableToSet;
        public GameObject objectToAssign;


        public bool AssignOnInit = false;

        private void Awake()
        {
            if (AssignOnInit)
            {
                SetToVariable();
            }
        }

        public void SetToVariable()
        {
            variableToSet.SetValue(objectToAssign);
        }
    }
}
