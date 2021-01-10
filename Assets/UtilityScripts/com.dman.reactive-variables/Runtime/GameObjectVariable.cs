using UnityEngine;

namespace Dman.ReactiveVariables
{
    [CreateAssetMenu(fileName = "GameObjectVariable", menuName = "ReactiveVariables/GameObjectVariable", order = 1)]
    public class GameObjectVariable : GenericVariable<GameObject>
    {
    }
}
