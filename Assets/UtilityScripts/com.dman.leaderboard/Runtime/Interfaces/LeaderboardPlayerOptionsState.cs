using System;
using JetBrains.Annotations;

namespace Dman.Leaderboard
{
    [Serializable]
    internal struct LeaderboardPlayerOptionsState
    {
        public static LeaderboardPlayerOptionsState Default => new LeaderboardPlayerOptionsState
        {
            leaderboardEnabled = false,
            leaderboardName = null
        };
            
        public bool leaderboardEnabled;
        [CanBeNull]
        public string leaderboardName;

        public override string ToString()
        {
            return $"LeaderboardState: {nameof(leaderboardEnabled)}: {leaderboardEnabled}, {nameof(leaderboardName)}: {leaderboardName}";
        }
    }
}