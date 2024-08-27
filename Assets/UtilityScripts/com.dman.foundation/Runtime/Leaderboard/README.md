

## Usage

Create an implementation of ILeaderboardRepositoryFactory as a UnitySingleton and add it to the scene, for example

```csharp
[UnitySingleton]
public class LeaderboardRepositoryFactory : MonoBehaviour, ILeaderboardRepositoryFactory
{
    private PlayfabLeaderboardRepository _playfabLeaderboardRepository;
    public ILeaderboardRepository GetRepository()
    {
        _playfabLeaderboardRepository ??= new PlayfabLeaderboardRepository();
        return _playfabLeaderboardRepository;
    }
}
```

Also add an instance of LeaderboardConfigurationSingleton to the scene, and configure it with your desired parameters.




Then use the LeaderboardPluginDisplay, LeaderboardPluginScoreSubmitter, and ConfigureLeaderboardPlayer to interact with the leaderboard.

### ConfigureLeaderboardPlayer

This component configures the name of the player for the leaderboard and
whether the player has opted in to leaderboard tracking.
Place it on any form where the player can enter their name, 
and bind it via events to the ConfigureLeaderboardPlayer component.

For example, set the OnEndEdit of an input field to invoke ConfigureLeaderboardPlayer.SetPlayerName.
And set the OnPlayerNameChanged event on ConfigureLeaderboardPlayer to set the text field in the input.

### LeaderboardDisplayController


To display the leaderboard, add both the LeaderboardDisplayController and the LeaderboardDisplayText to the UI prefab
which will render the leaderboard. The LeaderboardDisplayText is configured to display the leaderboard as a wall of text.

Optionally, implement your own ILeaderboardDisplay component and add that instead.
The LeaderboardDisplayController will invoke your implementation of ILeaderboardDisplay whenever there are changes
to the leaderboard, due to a new score or a player changing their name.

### LeaderboardScoreSubmitter

This component is used to submit scores to the leaderboard. Invoke SetNewScore as often as desired, this will
cache the score. Then call SubmitScore to submit the score to the leaderboard. SubmitScore will be automatically
called by the leaderboard displayer as well.

If the score should only be conditionally submitted, set the isValidScore property via SetIsValidScore. If set to false,
the score will not be submitted.

Alternatively to this component, you can invoke the score submit action directly:
    
```csharp
LeaderboardSingleton.Repository?
    .WriteScore(leaderboard, _currentCachedScore, CancellationToken.None).Forget();
```