using System.Linq;
using UnityEngine;

namespace Dman.SceneSaveSystem
{
    /// <summary>
    /// Attatch to the parent of any prefab that should be instantiated multiple times as part of loading
    ///     Will group all save data underneath it as part of the prefab, and not as part of the scene save data
    /// Requires a <see cref="SaveablePrefabParent"/> on the parent transform to uniquely identify the parent in the scene
    ///     which is needed to ensure the prefab is instantiated in the correct context when save loaded
    /// </summary>
    public class SaveablePrefab : MonoBehaviour
    {
        public SaveablePrefabType myPrefabType;

        public SavedPrefab GetPrefabSaveData()
        {
            var saveDataList = GetComponentsInChildren<ISaveableData>()
                    .Select(x => new SaveData
                    {
                        savedSerializableObject = x.GetSaveObject(),
                        uniqueSaveDataId = x.UniqueSaveIdentifier,
                        saveDataIDDependencies = x.GetDependencies().Select(x => x.UniqueSaveIdentifier).ToArray()
                    }).ToList();
            WorldSaveManager.SortSavedDatasBasedOnInterdependencies(saveDataList);
            var result = new SavedPrefab();

            var parent = transform.parent.gameObject.GetComponent<SaveablePrefabParent>();
            if (parent == null)
            {
                Debug.LogError($"prefab not directly underneath prefab parent, no parent found in {transform.parent.gameObject}");
            }
            result.prefabParentId = parent.prefabParentName;
            result.prefabTypeId = myPrefabType.myId;
            result.saveData = saveDataList.ToArray();
            return result;
        }
    }
}