using UnityEngine;

namespace Dman.ReactiveVariables
{
    [CreateAssetMenu(fileName = "ScriptableObjectVariable", menuName = "DmanUtilities/ReactiveVariables/ScriptableObjectVariable", order = 1)]
    public class ScriptableObjectVariable : GenericVariable<ScriptableObject>
    {
    }
}
