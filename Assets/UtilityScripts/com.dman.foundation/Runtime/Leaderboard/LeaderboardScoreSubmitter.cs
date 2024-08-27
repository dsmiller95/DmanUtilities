using System.Threading;
using Cysharp.Threading.Tasks;
using Dman.SaveSystem;
using UnityEngine;

namespace Dman.Leaderboard
{
    public class LeaderboardScoreSubmitter : MonoBehaviour
    {
        [SerializeField] private LeaderboardDefinition leaderboard = LeaderboardDefinition.Default;
        
        [SerializeField] bool isValidScore = false;
        
        private bool _hasNewScore = false;
        private int _currentCachedScore;
        private IKeepAliveHandle _saveSystemKeepAlive;

        public void SetIsValidScore(bool isValid)
        {
            isValidScore = isValid;
        }
        
        public void SetNewScore(int newScore)
        {
            _hasNewScore = true;
            _currentCachedScore = newScore;
        }
        
        public void SetLeaderboard(LeaderboardDefinition definition)
        {
            leaderboard = definition;
        }
        
        public void SubmitScore()
        {
            if (!_hasNewScore) return;
            if (!isValidScore) return;
            
            LeaderboardSingleton.Repository?
                .WriteScore(leaderboard, _currentCachedScore, CancellationToken.None).Forget();
        }
        
        private void OnDestroy()
        {
            SubmitScore();
        }
    }
}