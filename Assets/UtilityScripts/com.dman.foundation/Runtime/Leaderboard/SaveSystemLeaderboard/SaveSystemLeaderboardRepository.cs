using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Dman.SaveSystem;
using Dman.Utilities;

namespace Dman.Leaderboard.SaveSystemLeaderboard
{
    public class SaveSystemLeaderboardRepository : ILeaderboardRepository
    {
        private static readonly string SaveKeyName = nameof(SaveSystemLeaderboardRepository);
        private static string EntriesSaveKey => SaveKeyName + "_entries";
        private static string UsersSaveKey => SaveKeyName + "_users";
        private readonly string _saveContextName;
        private readonly string _currentPlayerId;

        private ISaveDataContext _saveContext;
        private ISaveDataContext SaveContext
        {
            get
            {
                if (_saveContext?.IsAlive == false) _saveContext = null;
                if (_saveContext == null)
                {
                    _saveContext = SingletonLocator<ISaveDataBehavior>.Instance
                        .GetContext(_saveContextName);
                }

                return _saveContext;
            }
        }


        public SaveSystemLeaderboardRepository(string saveContextName, string currentPlayerId)
        {
            _saveContextName = saveContextName;
            _currentPlayerId = currentPlayerId;
        }
        public UniTask WaitForPendingWriters()
        {
            return UniTask.CompletedTask;
        }

        public TimeSpan WaitAfterWritesToRead => TimeSpan.Zero;
        public bool SupportsNameChange => true;

        public UniTask WriteScore(LeaderboardDefinition leaderboard, int score, LeaderboardUpdateOptions updateOptions, CancellationToken cancel)
        {
            if (!SaveContext.TryLoad(EntriesSaveKey, out List<SavedLeaderboardEntry> entries))
            {
                entries = new List<SavedLeaderboardEntry>();
            }

            var existingEntryIndex = entries.FindIndex(x => x.userId == _currentPlayerId);
            if (existingEntryIndex == -1)
            {
                var newEntry = new SavedLeaderboardEntry
                {
                    userId = _currentPlayerId,
                    score = score
                };
                entries.Add(newEntry);
            }
            else
            {
                var existingEntry = entries[existingEntryIndex];
                if (updateOptions.updateType == LeaderboardUpdateType.KeepBest)
                {
                    if(leaderboard.higherIsBetter && existingEntry.score > score)
                    {
                        return UniTask.CompletedTask;
                    }
                    if(!leaderboard.higherIsBetter && existingEntry.score < score)
                    {
                        return UniTask.CompletedTask;
                    }
                }
                
                existingEntry.score = score;
                entries[existingEntryIndex] = existingEntry;
            }
            
            SaveContext.Save(EntriesSaveKey, entries);
            return UniTask.CompletedTask;
        }

        public UniTask WritePlayerName(string name, CancellationToken cancel)
        {
            if (!SaveContext.TryLoad(UsersSaveKey, out List<SaveUserEntry> users))
            {
                users = new List<SaveUserEntry>();
            }
            var existingUserIndex = users.FindIndex(x => x.userId == _currentPlayerId);
            if (existingUserIndex == -1)
            {
                var newUser = new SaveUserEntry
                {
                    userId = _currentPlayerId,
                    userName = name
                };
                users.Add(newUser);
            }
            else
            {
                var existingUser = users[existingUserIndex];
                if (existingUser.userName == name)
                {
                    return UniTask.CompletedTask;
                }
                
                existingUser.userName = name;
                users[existingUserIndex] = existingUser;
            }
            SaveContext.Save(UsersSaveKey, users);
            return UniTask.CompletedTask;
        }

        public UniTask<LeaderboardEntry[]> GetLeaderboard(LeaderboardDefinition leaderboard, int maxResults, CancellationToken cancel)
        {
            if (!SaveContext.TryLoad(EntriesSaveKey, out List<SavedLeaderboardEntry> entries))
            {
                entries = new List<SavedLeaderboardEntry>();
            }
            if (!SaveContext.TryLoad(UsersSaveKey, out List<SaveUserEntry> users))
            {
                users = new List<SaveUserEntry>();
            }

            var usersDictionary = users.ToDictionary(x => x.userId, x => x);
            
            var mappedEntries = entries.Select(x =>
            {
                if (!usersDictionary.TryGetValue(x.userId, out var user))
                {
                    user = default;
                }
                
                return new LeaderboardEntry
                {
                    name = user.userName ?? "Unknown",
                    isCurrentUser = x.userId == _currentPlayerId,
                    score = x.score,
                };
            }).ToArray();

            if (leaderboard.higherIsBetter)
            {
                Array.Sort(mappedEntries, (a, b) => b.score.CompareTo(a.score));
            }
            else
            {
                Array.Sort(mappedEntries, (a, b) => a.score.CompareTo(b.score));
            }

            for (int i = 0; i < mappedEntries.Length; i++)
            {
                mappedEntries[i].rank = i;
            }
            
            var truncatedEntries = mappedEntries.Take(maxResults).ToArray();
            
            return UniTask.FromResult(truncatedEntries);
        }


        [Serializable]
        private struct SavedLeaderboardEntry
        {
            public string userId; 
            public int score;
        }

        [Serializable]
        private struct SaveUserEntry
        {
            public string userId;
            public string userName;
        }
    }
}