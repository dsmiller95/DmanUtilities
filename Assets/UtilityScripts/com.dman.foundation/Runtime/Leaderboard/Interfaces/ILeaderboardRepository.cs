using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Dman.Leaderboard
{
    public interface ILeaderboardRepository
    {
        // TODO: delegate this out into a wrapper, not every implementation needs this?
        /// <summary>
        /// will complete when there are no Write*** operations in progress
        /// </summary>
        public UniTask WaitForPendingWriters();
        
        /// <summary>
        /// The amount of time to wait after a write operation before reading the leaderboard.
        /// </summary>
        public TimeSpan WaitAfterWritesToRead { get; }

        /// <summary>
        /// Writes the current user's score to the given leaderboard.
        /// </summary>
        /// <param name="leaderboard"></param>
        /// <param name="score"></param>
        /// <param name="updateOptions"></param>
        /// <param name="cancel">May cancel any async operations when cancelled</param>
        /// <returns></returns>
        public UniTask WriteScore(LeaderboardDefinition leaderboard, int score, LeaderboardUpdateOptions updateOptions, CancellationToken cancel);

        /// <summary>
        /// Change the current player's preferred name. this name should appear in LeaderboardEntries.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cancel"></param>
        /// <returns></returns>
        public UniTask WritePlayerName(string name, CancellationToken cancel);
        
        /// <summary>
        /// Gets a list of leaderboard entries to display, up to <see cref="maxResults"/>
        /// </summary>
        /// <param name="leaderboard"></param>
        /// <param name="maxResults"></param>
        /// <param name="cancel">May cancel any async operations when cancelled</param>
        /// <returns></returns>
        public UniTask<LeaderboardEntry[]> GetLeaderboard(LeaderboardDefinition leaderboard, int maxResults, CancellationToken cancel);
        
    }
}