using System;

namespace Dman.Leaderboard
{
    /// <summary>
    /// To Implementers: Implement this interface on a monobehavior, and tag it with [UnitySingleton] <br/>
    /// It will act as a singleton, and will only be searched for on initial world load
    /// and when <see cref="OnRepositoryMayChange"/> is invoked
    /// </summary>
    public interface ILeaderboardRepositoryFactory
    {
        /// <summary>
        /// To Implementers: use any logic desired to return a selected implementation of ILeaderboardRepository <br/>
        /// The return value will be automatically cached until <see cref="OnRepositoryMayChange"/> is invoked
        /// </summary>
        /// <returns></returns>
        ILeaderboardRepository GetRepository();

        /// <summary>
        /// To Implementers: Invoke this event whenever the returned value from GetRepository may have changed
        /// </summary>
        event Action OnRepositoryMayChange;
    }
}