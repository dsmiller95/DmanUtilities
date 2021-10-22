using System.Collections.Generic;
using System.Linq;

namespace Dman.SceneSaveSystem
{
    [System.Serializable]
    public class SaveScopeData : ISaveDataPiece
    {
        public ISaveScopeIdentifier scopeIdentifier;
        public List<SaveData> dataInScope;
        public List<SaveScopeData> childScopes;

        [System.NonSerialized]
        private IDictionary<string, SaveData> _savedDict;
        public IDictionary<string, SaveData> DataInScopeDictionary
        {
            get
            {
                if (_savedDict == null)
                {
                    _savedDict = dataInScope.ToDictionary(x => x.uniqueSaveDataId);
                }
                return _savedDict;
            }
        }

        public SaveScopeData(ISaveScopeIdentifier identifier)
        {
            dataInScope = new List<SaveData>();
            childScopes = new List<SaveScopeData>();
            scopeIdentifier = identifier;
        }

        public void OverwriteWith(SaveScopeData other)
        {
            // TODO!
            if (!other.scopeIdentifier.Equals(scopeIdentifier))
            {
                throw new System.Exception("Cannot overwrite. Scope identifiers do not match");
            }
        }
    }
}
