using System.Collections.Generic;
using System.Linq;

namespace Dman.SceneSaveSystem
{
    [System.Serializable]
    public class MasterSaveObject
    {
        public SaveData[] sceneSaveData;
        public SavedPrefab[] sceneSavedPrefabInstances;
    }
}
