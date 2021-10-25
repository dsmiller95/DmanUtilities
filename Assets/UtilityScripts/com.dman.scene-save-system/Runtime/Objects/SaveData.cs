using System;

namespace Dman.SceneSaveSystem.Objects
{
    [Serializable]
    internal class SaveData : ISaveDataPiece
    {
        public string uniqueSaveDataId;
        public object savedSerializableObject;
    }
}
