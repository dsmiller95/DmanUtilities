using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Dman.Leaderboard
{
    public class LeaderboardPlayerConfigure : MonoBehaviour
    {
        [SerializeField] private int maxLeaderboardNameLength = 4;
        [SerializeField] private UnityEvent<bool> onLeaderboardEnabledChanged;
        [FormerlySerializedAs("onLeaderboardNameChanged")] [SerializeField] private UnityEvent<string> onPlayerNameChanged;

        [Tooltip("this will be disabled if the selected leaderboard does not support changing the player name")]
        [SerializeField] private GameObject nameInputField;
        
        private void Awake()
        {
            LeaderboardPlayerSingleton.PlayerStateChanged += OnPlayerStateChanged;
            
            var state = LeaderboardPlayerSingleton.GetLeaderboardPlayerState();
            OnPlayerStateChanged(state);
            
            LeaderboardSingleton.OnRepositoryMayChange += EnableIfAllowed;
            EnableIfAllowed();
        }

        private void EnableIfAllowed()
        {
            if(nameInputField == null) return;
            var supportsNameChange = LeaderboardSingleton.Repository?.SupportsNameChange ?? false;
            nameInputField.SetActive(supportsNameChange);
        }

        private LeaderboardPlayerOptionsState? lastState = null; 
        private void OnPlayerStateChanged(LeaderboardPlayerOptionsState newState)
        {
            if (newState.leaderboardEnabled != lastState?.leaderboardEnabled)
            {
                onLeaderboardEnabledChanged.Invoke(newState.leaderboardEnabled);
            }

            if (newState.leaderboardName != lastState?.leaderboardName)
            {
                onPlayerNameChanged.Invoke(newState.leaderboardName);
            }

            lastState = newState;
        }


        public void SetLeaderboardEnabled(bool leaderboardEnabled)
        {
            var state = LeaderboardPlayerSingleton.GetLeaderboardPlayerState();
            if (state.leaderboardEnabled == leaderboardEnabled) return;
            
            state.leaderboardEnabled = leaderboardEnabled;
            if (state.leaderboardName == null)
            {
                state.leaderboardName = GenerateRandomName();
            }
            LeaderboardPlayerSingleton.SaveLeaderboardPlayerState(state);
        }
        
        public bool GetLeaderboardEnabled()
        {
            return LeaderboardPlayerSingleton.GetLeaderboardPlayerState().leaderboardEnabled;
        }

        public void SetPlayerName([CanBeNull] string newName)
        {
            newName ??= GenerateRandomName();
            var state = LeaderboardPlayerSingleton.GetLeaderboardPlayerState();
            if (state.leaderboardName == newName) return;
            state.leaderboardName = newName;
            LeaderboardPlayerSingleton.SaveLeaderboardPlayerState(state);
        }
        
        public string GetPlayerName()
        {
            return LeaderboardPlayerSingleton.GetLeaderboardPlayerState().leaderboardName;
        }
        

        private string GenerateRandomName()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var random = new System.Random(UnityEngine.Random.Range(1, int.MaxValue));
            var name = new char[maxLeaderboardNameLength];

            for (int i = 0; i < maxLeaderboardNameLength; i++)
            {
                name[i] = chars[random.Next(chars.Length)];
            }

            return new string(name);
        }
    }
}