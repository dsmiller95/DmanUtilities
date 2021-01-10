using System;

namespace Dman.SceneSaveSystem
{
    [Serializable]
    public class SaveData
    {
        public string uniqueSaveDataId;
        public object savedSerializableObject;
        public string[] saveDataIDDependencies;
    }
}
