# Object Sets 0.1.0

Object sets helps with maintaining indexable lists of either monobehaviors or scriptable objects. UniqueObjectRegistries are very handy when trying to save references to assets. Example usage, trimmed down from from com.dman.scene-save-system:

```C#

    [CreateAssetMenu(menuName = "SaveablePrefabRegistry")]
    public class SaveablePrefabRegistry : UniqueObjectRegistryWithAccess<SaveablePrefabType>
    {
    }
    [CreateAssetMenu(menuName = "SaveablePrefabType")]
    public class SaveablePrefabType : IDableObject
    {
        public SaveablePrefab prefab;
    }

    public class SaveablePrefab : MonoBehaviour
    {
        public SaveablePrefabType myPrefabType;

        public SavedPrefab GetPrefabSaveData()
        {
            var result = new SavedPrefab();
            result.prefabTypeId = myPrefabType.myId;
            return result;
        }
    }

    [Serializable]
    public class SavedPrefab
    {
        public int prefabTypeId;

        public SaveablePrefab GetPrefab()
        {
            var prefabRegistry = RegistryRegistry.GetObjectRegistry<SaveablePrefabType>();
            return prefabRegistry.GetUniqueObjectFromID(prefabTypeId).prefab;
        }
    }
```

For this example, in-editor setup will consist of these objects:

- One SaveablePrefabRegistry scriptable object
- many SaveablePrefabType scriptable objects
- as many Prefabs as SaveablePrefabType
  - each prefab has a SaveablePrefab component on the root object
  - each SaveablePrefabType is assigned to exactly one SaveablePrefab object, and vice versa
- All SaveablePrefabTypes are added to the SaveablePrefabRegistry, and assigned unique IDs

This system allows serialization of the SavedPrefab, and allows for the same prefab to be recreated based off of that data using only the prefab ID. This can be useful for pointers to other metadata. For example instead of a prefab the unique object could hold configuration data, and consumers of that data could switch which configuration they reference and save that reference to a file easily.
