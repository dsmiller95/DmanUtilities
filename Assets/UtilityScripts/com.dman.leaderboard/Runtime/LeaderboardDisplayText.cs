using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Leaderboard.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Leaderboard
{
    public interface ILeaderboardDisplay
    {
        UniTask DisplayEntries(LeaderboardEntry[] entries, CancellationToken cancel);
    }
    
    public class LeaderboardDisplayText : MonoBehaviour, ILeaderboardDisplay
    {
        [SerializeField] private string entryFormat = "{0} :{1} points";
        [SerializeField] private UnityEvent<string> changeLeaderboardText;

        public UniTask DisplayEntries(LeaderboardEntry[] entries, CancellationToken cancel)
        {
            var leaderboardText = string.Join("\n", entries.Select(FormatLeaderboardEntry));
            changeLeaderboardText.Invoke(leaderboardText);

            return UniTask.CompletedTask;
        }
        
        private string FormatLeaderboardEntry(LeaderboardEntry entry)
        {
            var formattedName = TruncateEnd(entry.name, 4).PadLeft(4, ' ');
            var formattedScore = entry.score.ToString().PadLeft(6, ' ');
            var formatted = string.Format(entryFormat, formattedName, formattedScore);
            if (entry.isCurrentUser)
            {
                return "<color=yellow>" + formatted + "</color>";
            }
            else
            {
                return formatted;
            }
        }

        private static string TruncateEnd(string str, int length)
        {
            if(str == null) return "";
            if (str.Length <= length) return str;
            return str[..length];
        }
    }
}