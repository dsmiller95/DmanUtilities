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

        public delegate void SaveLifecycleHook();
        internal static void TriggerPreSave()
        {
            Instance.PreSave?.Invoke();
        }
        public event SaveLifecycleHook PreSave;

        internal static void TriggerPostSave()
        {
            Instance.PostSave?.Invoke();
        }
        public event SaveLifecycleHook PostSave;

        internal static void TriggerPreLoad()
        {
            Instance.PreLoad?.Invoke();
        }
        public event SaveLifecycleHook PreLoad;

        internal static void TriggerMidLoad()
        {
            Instance.MidLoad?.Invoke();
        }
        /// <summary>
        /// an event which triggers after the new scene has been loaded, but before any of the save data is loaded into the scene
        ///     This is meant primarily for testing purposes, it is recommended to avoid using this hook for gameplay
        /// </summary>
        public event SaveLifecycleHook MidLoad;

        internal static void TriggerPostLoad()
        {
            Instance.PostLoad?.Invoke();
        }
        public event SaveLifecycleHook PostLoad;
    }
}
