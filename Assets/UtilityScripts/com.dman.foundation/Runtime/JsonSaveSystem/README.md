# Player prefs inspired save system

Save data is organized into contexts, which are keyed by strings. This allows for saving and 
loading into save slots, or saving and loading a "root" context.

## Setup

Add a `SaveDatacontextProviderBehavior` to a gameobject in the scene, this is a singleton object.
It will keep itself alive, so load it in the first scene of the game.

Add a list of Contexts which you want the save system to save and load from. For a game without save slots, this
will contain a single key. for example, "Root". use this save context name to load save contexts
in the rest of the app.

## Usage Example


This script will maintain a list of strings that have been seen, and save them to the save system.
The "seen" strings can be queried from outside, of managed with logic contained to this class.

```csharp
public class SomethingThatSavesData : MonoBehaviour
{
    // To use Save Slots, this context may be set externally.
    //    or queried from the game object heirarchy
    [SerializeField] private string saveContextName = "SomeContextName";
    private readonly string _saveKeyRoot = $"{nameof(SomethingThatSavesData)}_SeenStrings";
    
    private ISaveDataContext _saveContext;
    private ISaveDataContext SaveContext
    {
        get
        {
            // the _saveContext may be invalidated, in which case, we need a new one
            if (_saveContext?.IsAlive == false) _saveContext = null;
            
            if (_saveContext == null)
            {
                // if we don't have a context, use the SingletonLocator<ISaveDataBehavior>
                //  utility to get one, using the current context name
                _saveContext = SingletonLocator<ISaveDataBehavior>.Instance
                    .GetContext(seenAbilitiesContextName);
            }

            return _saveContext;
        }
    }
    
    private bool HasSeen(string thing)
    {
        var key = _saveKeyRoot + "_" + "set";
        if(SaveContext.TryLoad(key, out List<string> seenSet))
        {
            return seenSet.Contains(thing);
        }

        return false;
    }

    private void SetSeen(string thing, bool seen = true)
    {
        var key = _saveKeyRoot + "_" + "set";
        if(!SaveContext.TryLoad(key, out List<string> seenSet))
        {
            seenSet = new List<string>();
        }
        var containsThing = seenSet.Contains(thing);
        if (seen)
        {
            if (containsThing) return;
            seenSet.Add(thing);
        }

        if (!seen)
        {
            if (!containsThing) return;
            seenSet.Remove(thing);
        }
        
        SaveContext.Save(key, seenSet);
    }
}
```

The json will be saved to `{Application.persistentDataPath}/SaveContexts/SomeContextName.json`, in this format:
```json
{
  "SomethingThatSavesData_SeenStrings_set": [
    "thing1",
    "thing_Two",
    "another thingamajig"
  ]
}
```


## Advanced usage

For performance reasons, you may want to cache data inside a component, and only save it when the component is destroyed.
In this case we need to notify the save system that it should wait until we are done persisting data before it writes this
data into the save file.

This is done by taking a Keep Alive Handle from the save context, and disposing it after our object lifetime ends in OnDestroy.
The save system will only persist data when all Keep Alive Handles are disposed, and the save system singleton is destroyed.

```csharp
[SerializeField] private float cachedFloatData;
private IKeepAliveHandle _keepSaveContextAlive;

private void Awake()
{
    _keepSaveContextAlive = SingletonLocator<ISaveDataBehavior>.Instance.KeepAliveUntil();
    SaveContext.TryLoad(_leaderboardRoot, out cachedFloatData);
}

private void OnDestroy()
{
    SaveContext.Save(_leaderboardRoot, cachedFloatData);
    _keepSaveContextAlive?.Dispose();
}
```
