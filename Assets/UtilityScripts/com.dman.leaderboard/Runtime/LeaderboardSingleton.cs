using Dman.Utilities;
using Dman.Utilities.Logger;
using JetBrains.Annotations;
using Leaderboard.Interfaces;

namespace Leaderboard
{
    /// <summary>
    /// consume this in all user code which needs to query or change the leaderboard
    /// </summary>
    public static class LeaderboardSingleton
    {
        private static ILeaderboardRepository _repository = null;

        private static ILeaderboardRepositoryFactory _repositoryFactory;
        
        [CanBeNull]
        public static ILeaderboardRepository Repository
        {
            get
            {
                if (_repository != null) return _repository;
                
                if (_repositoryFactory != null)
                {
                    _repositoryFactory.OnRepositoryMayChange -= OnRepositoryChanged;
                }
                _repositoryFactory = SingletonLocator<ILeaderboardRepositoryFactory>.Instance;
                if (_repositoryFactory == null)
                {
                    Log.Error("No leaderboard repository factory found. " +
                              "Implement ILeaderboardRepositoryFactory and add it to the scene");
                    _repository = null;
                    return _repository;
                }

                _repositoryFactory.OnRepositoryMayChange += OnRepositoryChanged;
                _repository = _repositoryFactory.GetRepository();
                if (_repository == null)
                {
                    Log.Error($"Leaderboard repository factory returned null repository. " +
                              $"factory: {_repositoryFactory}");
                }


                return _repository;
            }
        }

        private static void OnRepositoryChanged()
        {
            // trigger a re-pull of the repository on the next Get
            _repository = null;
        }
    }
}