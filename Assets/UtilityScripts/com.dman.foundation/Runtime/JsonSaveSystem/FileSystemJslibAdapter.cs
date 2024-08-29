using System.Runtime.CompilerServices;

namespace Dman.SaveSystem
{
    public static class FileSystemJslibAdapter
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void JS_FileSystem_Sync();
#else
        private static void JS_FileSystem_Sync() { }
#endif

        public static void EnsureSynced()
        {
            JS_FileSystem_Sync();
        }
    }
}