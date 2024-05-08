using Dman.Utilities;

namespace Dman.SceneSaveSystem
{
    public class SaveSystemHooks
    {
        private static SaveSystemHooks _instance;
        public static SaveSystemHooks Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SaveSystemHooks();
                }
                return _instance;
            }
        }

        public delegate void SaveLifecycleHook(SceneReference targetScene);

        public event SaveLifecycleHook PreSave;
        internal static void TriggerPreSave(SceneReference targetScene)
        {
            Instance.PreSave?.Invoke(targetScene);
        }

        public event SaveLifecycleHook PostSave;
        internal static void TriggerPostSave(SceneReference targetScene)
        {
            Instance.PostSave?.Invoke(targetScene);
        }

        public event SaveLifecycleHook PreLoad;
        internal static void TriggerPreLoad(SceneReference targetScene)
        {
            Instance.IsLoadProcessActive = true;
            Instance.PreLoad?.Invoke(targetScene);
        }

        /// <summary>
        /// an event which triggers after the new scene has been loaded, but before any of the save data is loaded into the scene
        ///     This is meant primarily for testing purposes, it is recommended to avoid using this hook for gameplay
        /// </summary>
        public event SaveLifecycleHook MidLoad;
        internal static void TriggerMidLoad(SceneReference targetScene)
        {
            Instance.MidLoad?.Invoke(targetScene);
        }

        public event SaveLifecycleHook PostLoad;
        internal static void TriggerPostLoad(SceneReference targetScene)
        {
            Instance.PostLoad?.Invoke(targetScene);
            Instance.IsLoadProcessActive = false;
        }

        public bool IsLoadProcessActive { get; private set; }
    }
}
