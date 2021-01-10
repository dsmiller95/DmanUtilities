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
        /// <summary>
        /// TODO: test
        /// All dependencies are going to have their save data loaded before this loads its data.
        ///     this only applies inside of scene, and prefabs.
        /// </summary>
        /// <returns></returns>
        ISaveableData[] GetDependencies();
    }
}
