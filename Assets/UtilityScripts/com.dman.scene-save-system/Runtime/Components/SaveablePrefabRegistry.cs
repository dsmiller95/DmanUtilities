using Dman.ObjectSets;
using UnityEngine;

namespace Dman.SceneSaveSystem
{
    [CreateAssetMenu(fileName = "SaveablePrefabRegistry", menuName = "DmanUtilities/Saving/SaveablePrefabRegistry", order = 10)]
    public class SaveablePrefabRegistry : UniqueObjectRegistryWithAccess<SaveablePrefabType>
    {
    }
}