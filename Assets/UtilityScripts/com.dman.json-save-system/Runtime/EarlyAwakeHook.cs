using UnityEngine;

namespace SaveSystem
{
    public interface IAwakeEarly
    {
        public void AwakeEarly();
    }
    [DefaultExecutionOrder(-1000)]
    public class EarlyAwakeHook : MonoBehaviour
    {
        private void Awake()
        {
            foreach (var awakeEarly in GetComponents<IAwakeEarly>())
            {
                awakeEarly.AwakeEarly();
            }
        }
    }
}