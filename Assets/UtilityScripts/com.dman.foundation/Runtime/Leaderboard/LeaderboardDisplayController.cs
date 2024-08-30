using System.Threading;
using Cysharp.Threading.Tasks;
using Dman.Utilities.Logger;
using UnityEngine;

namespace Dman.Leaderboard
{
    /// <summary>
    /// Refreshes the leaderboard when:
    /// - it is enabled
    /// - the player's name changes
    ///
    /// A leaderboard refresh consists of:
    /// - submitting all scores represented by ScoreSubmitter components in the scene
    /// - waiting for the scores to be written
    /// - reading the leaderboard
    /// - displaying the leaderboard via an attached ILeaderboardDisplay component
    /// </summary>
    public class LeaderboardDisplayController : MonoBehaviour
    {
        [SerializeField] private int maxResults = 10;
        [SerializeField] private LeaderboardDefinition leaderboard = LeaderboardDefinition.Default;
        
        private AsyncFnOnceCell _renderLeaderboardCell;
        
        public void SetLeaderboard(LeaderboardDefinition definition)
        {
            this.leaderboard = definition;
            RefreshLeaderboard();
        }

        public void RefreshLeaderboard()
        {
            _renderLeaderboardCell.CancelThenTryRunNextFrame(RenderLeaderboard,
                "could not restart rendering new leaderboard");
        }
        
        private void Awake()
        {
            _renderLeaderboardCell = new AsyncFnOnceCell(gameObject);
        }

        private void OnEnable()
        {
            LeaderboardPlayerSingleton.PlayerStateChanged += OnPlayerStateChanged;
            _renderLeaderboardCell.TryRun(RenderLeaderboard, "could not render leaderboard");
        }

        private void OnDisable()
        {
            LeaderboardPlayerSingleton.PlayerStateChanged -= OnPlayerStateChanged;
        }

        private void OnPlayerStateChanged(LeaderboardPlayerOptionsState obj)
        {
            RefreshLeaderboard();
        }

        private async UniTask RenderLeaderboard(CancellationToken cancel)
        {
            var allSubmitters = GameObject.FindObjectsOfType<LeaderboardScoreSubmitter>();
            foreach (LeaderboardScoreSubmitter submitter in allSubmitters)
            {
                submitter.SubmitScore();
            }
            await UniTask.NextFrame(cancel);
            var repository = LeaderboardSingleton.Repository;
            if (repository == null)
            {
                Log.Warning("No leaderboard repository found");
                await UniTask.Delay(LeaderboardConstants.WaitForSingletonToAppear, ignoreTimeScale: true, PlayerLoopTiming.Update, cancel);
            }
            repository = LeaderboardSingleton.Repository;
            if(repository == null)
            {
                Log.Error("No leaderboard repository found after waiting");
                return;
            }
            await repository.WaitForPendingWriters().AttachExternalCancellation(cancel);
            await UniTask.Delay(repository.WaitAfterWritesToRead, ignoreTimeScale: true, PlayerLoopTiming.Update, cancel);

            var leaderboardContents = await repository.GetLeaderboard(leaderboard, maxResults, cancel)
                .AttachExternalCancellation(cancel);
            var displayer = GetComponent<ILeaderboardDisplay>();
            if (displayer == null)
            {
                Log.Error($"Found no leaderboard displayer. add a component that implements {nameof(ILeaderboardDisplay)} to this object");
                return;
            }

            await displayer.DisplayEntries(leaderboardContents, cancel);
        }
    }
}