using System;

namespace Dman.SceneSaveSystem
{
    [Serializable]
    public class SaveData : ISaveDataPiece
    {
        public string uniqueSaveDataId;
        public object savedSerializableObject;
    }
}
