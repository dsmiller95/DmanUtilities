using System;

namespace Dman.Leaderboard
{
    [Serializable]
    public struct LeaderboardDefinition
    {
        public static LeaderboardDefinition Default => new LeaderboardDefinition
        {
            leaderboardName = "default",
            higherIsBetter = true
        };
        
        public string leaderboardName;
        public bool higherIsBetter;
    }
}