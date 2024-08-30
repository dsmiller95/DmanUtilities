using System;

namespace Dman.Leaderboard
{
    public enum LeaderboardUpdateType
    {
        KeepBest,
        AlwaysReplace,
    } 
    [Serializable]
    public struct LeaderboardUpdateOptions
    {
        public static LeaderboardUpdateOptions Default => new LeaderboardUpdateOptions
        {
            updateType = LeaderboardUpdateType.KeepBest,
        };
        
        public LeaderboardUpdateType updateType;
    }
}