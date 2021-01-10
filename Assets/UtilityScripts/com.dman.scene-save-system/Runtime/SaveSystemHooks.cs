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
        public static void TriggerPreSave()
        {
            Instance.PreSave?.Invoke();
        }
        public event SaveLifecycleHook PreSave;

        public static void TriggerPostSave()
        {
            Instance.PostSave?.Invoke();
        }
        public event SaveLifecycleHook PostSave;

        public static void TriggerPreLoad()
        {
            Instance.PreLoad?.Invoke();
        }
        public event SaveLifecycleHook PreLoad;

        public static void TriggerPostLoad()
        {
            Instance.PostLoad?.Invoke();
        }
        public event SaveLifecycleHook PostLoad;
    }
}
