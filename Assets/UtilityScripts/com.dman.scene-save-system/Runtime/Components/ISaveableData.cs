namespace Dman.SceneSaveSystem
{
    /// <summary>
    /// any <see cref="UnityEngine.Component"/> which implements this will be saved as part of the save system pass
    /// </summary>
    public interface ISaveableData
    {
        /// <summary>
        /// A unique indentifier inside the prefab, or inside the scene, to identify this data
        /// </summary>
        public string UniqueSaveIdentifier { get; }
        /// <summary>
        /// used to change the order in which this save data is deserialized, relative to other savable data in this scope
        /// default is 0, and prefab parents are 1000. This means that prefab parents load after default save data.
        /// a value of -10 will cause the data to load before all default save data
        /// </summary>
        public int LoadOrderPriority { get; }
        object GetSaveObject();
        void SetupFromSaveObject(object save);
    }
}
