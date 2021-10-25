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
        object GetSaveObject();
        void SetupFromSaveObject(object save);
    }
}
