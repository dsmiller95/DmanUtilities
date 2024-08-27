using System;

namespace Leaderboard.Interfaces
{
    [Serializable]
    public struct LeaderboardEntry
    {
        /// <summary>
        /// The display name provided by the players
        /// </summary>
        public string name;
        public bool isCurrentUser;
        public int score;
        /// <summary>
        /// The ranking in the leaderboard. always increasing order, I.E. 0 is the best in 1st place, 10 is the 11th place 
        /// </summary>
        public int rank;
    }
}