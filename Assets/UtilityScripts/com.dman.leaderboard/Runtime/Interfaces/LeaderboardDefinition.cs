using System;

namespace Leaderboard.Interfaces
{
    [Serializable]
    public struct LeaderboardDefinition
    {
        public string leaderboardName;
        public bool higherIsBetter;
    }
}