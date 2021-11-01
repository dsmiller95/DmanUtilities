using Dman.SceneSaveSystem.Objects.Identifiers;
using System.Collections.Generic;
using System.Linq;

namespace Dman.SceneSaveSystem.Objects
{
    [System.Serializable]
    internal class SaveScopeData : ISaveDataPiece
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

        public void InsertSaveData(SaveData newSaveData)
        {
            dataInScope.RemoveAll(x => x.uniqueSaveDataId == newSaveData.uniqueSaveDataId);
            dataInScope.Add(newSaveData);
        }

        public void OverwriteWith(SaveScopeData other)
        {
            if (!other.scopeIdentifier.Equals(scopeIdentifier))
            {
                throw new System.Exception("Cannot overwrite. Scope identifiers do not match");
            }
            for (int i = 0; i < other.dataInScope.Count; i++)
            {
                var otherItem = other.dataInScope[i];
                DataInScopeDictionary[otherItem.uniqueSaveDataId] = otherItem;
            }
            this.dataInScope = DataInScopeDictionary.Select(x => x.Value).ToList();
            _savedDict = null; // un-cache the dictionary since modifications were made to the data dictionary

            // assuming all child scopes are prefab scopes. in practice, this is how the tree gets set up currently
            if (!childScopes.All(x => x.scopeIdentifier is PrefabSaveScopeIdentifier) || !other.childScopes.All(x => x.scopeIdentifier is PrefabSaveScopeIdentifier))
            {
                throw new System.NotImplementedException("Nested scopes of a type other than a prefab scope are not supported");
            }

            var otherPrefabParentNames = other.childScopes
                .Select(x => x.scopeIdentifier)
                .OfType<PrefabSaveScopeIdentifier>()
                .Select(x => x.prefabParentId)
                .Distinct()
                .ToList();

            // remove every prefab under prefab parents that are present 
            this.childScopes = childScopes
                .Where(x =>
                    {
                        var prefabParent = (x.scopeIdentifier as PrefabSaveScopeIdentifier).prefabParentId;
                        return !otherPrefabParentNames.Contains(prefabParent);
                    })
                .Concat(other.childScopes)
                .ToList();
        }
    }
}
