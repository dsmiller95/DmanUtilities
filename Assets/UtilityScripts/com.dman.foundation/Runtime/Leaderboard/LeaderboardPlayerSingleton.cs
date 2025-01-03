﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dman.SaveSystem;
using Dman.Utilities;
using Dman.Utilities.Logger;

namespace Dman.Leaderboard
{
    /// <summary>
    /// Used for internal management of the leaderboard player.
    /// To configure the leaderboard player in user code, use the provided ConfigureLeaderboardPlayer component
    /// </summary>
    internal static class LeaderboardPlayerSingleton
    {
        public static Action<LeaderboardPlayerOptionsState> PlayerStateChanged;
        private static LeaderboardPlayerOptionsState? _playerState;

        private static string _saveContextName = null;
        private static bool _isInitialized;
        
        
        private static ISaveDataContext _saveContext;
        private static ISaveDataContext SaveContext
        {
            get
            {
                if (_saveContext?.IsAlive == false) _saveContext = null;
                if (_saveContext == null)
                {
                    _saveContext = JsonSaveSystemSingleton.GetContextProvider()
                        .GetContext(_saveContextName);
                }

                return _saveContext;
            }
        }
        
        public static void InitializeContext(string newSaveContextName, bool emitLogs)
        {
            if (newSaveContextName == _saveContextName) return;
            if (_isInitialized)
            {
                Log.Warning($"Save context already initialized, initializing again");
            }
            _isInitialized = true;
            _saveContextName = newSaveContextName;
            _saveContext = null;
        }
        
        public static LeaderboardPlayerOptionsState GetLeaderboardPlayerState()
        {
            ThrowIfNotInitialized();
            if (!SaveContext.TryLoad(LeaderboardConstants.PlayerOptionsSaveKey, out LeaderboardPlayerOptionsState state))
            {
                state = new LeaderboardPlayerOptionsState();
            }
            _playerState = state;
            return state;
        }

        public static void SaveLeaderboardPlayerState(LeaderboardPlayerOptionsState newPlayerOptionsState)
        {
            ThrowIfNotInitialized();
            if (_playerState.Equals(newPlayerOptionsState))
            {
                return;
            }
            SaveContext.Save(LeaderboardConstants.PlayerOptionsSaveKey, newPlayerOptionsState);
            var lastPlayerState = _playerState;
            _playerState = newPlayerOptionsState;

            if (lastPlayerState?.leaderboardName != newPlayerOptionsState.leaderboardName)
            {
                LeaderboardSingleton.Repository?
                .WritePlayerName(newPlayerOptionsState.leaderboardName, CancellationToken.None).Forget();
            }
            
            PlayerStateChanged?.Invoke(newPlayerOptionsState);
        }

        private static void ThrowIfNotInitialized()
        {
            if (!_isInitialized)
            {
                throw new Exception("LeaderboardPlayerSingleton not initialized. Add a LeaderboardConfigurationSingleton to the initial scene");
            }
        }
    }
}