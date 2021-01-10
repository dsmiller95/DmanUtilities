using System;
using System.Collections.Generic;
using System.Linq;

namespace Dman.SceneSaveSystem
{
    [Serializable]
    public class SavedPrefab
    {
        public int prefabTypeId;
        public string prefabParentId;
        public SaveData[] saveData;

        private IDictionary<string, SaveData> _savedDict;
        public IDictionary<string, SaveData> SaveDataDictionary
        {
            get
            {
                if (_savedDict == null)
                {
                    _savedDict = saveData.ToDictionary(x => x.uniqueSaveDataId);
                }
                return _savedDict;
            }
        }
    }
}
