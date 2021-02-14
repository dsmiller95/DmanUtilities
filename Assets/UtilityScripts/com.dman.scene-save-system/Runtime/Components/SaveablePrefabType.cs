using Dman.ObjectSets;
using UnityEngine;

namespace Dman.SceneSaveSystem
{
    [CreateAssetMenu(fileName = "SaveablePrefabType", menuName = "DmanUtilities/Saving/SaveablePrefabType", order = 1)]
    public class SaveablePrefabType : IDableObject
    {
        public SaveablePrefab prefab;
    }
}