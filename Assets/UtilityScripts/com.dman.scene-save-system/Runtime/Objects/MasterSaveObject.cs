using System.Collections.Generic;
using System.Linq;

namespace Dman.SceneSaveSystem
{
    [System.Serializable]
    public class MasterSaveObject
    {
        public SaveData[] sceneSaveData;
        public SavedPrefab[] sceneSavedPrefabInstances;

        private IDictionary<string, SaveData> _sceneSavedDict;
        public IDictionary<string, SaveData> SceneSaveDataDictionary
        {
            get
            {
                if (_sceneSavedDict == null)
                {
                    _sceneSavedDict = sceneSaveData.ToDictionary(x => x.uniqueSaveDataId);
                }
                return _sceneSavedDict;
            }
        }
    }
}
