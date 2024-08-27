using UnityEngine;

namespace Dman.Leaderboard
{
    [DefaultExecutionOrder(-1000)]
    public class LeaderboardConfigurationSingleton : MonoBehaviour
    {
        [SerializeField] private bool emitDebugLogs = false;
        [SerializeField] private string leaderboardSaveContextName = "root";
        
        private void Awake()
        {
            LeaderboardPlayerSingleton.InitializeContext(leaderboardSaveContextName, emitDebugLogs);
        }
    }
}