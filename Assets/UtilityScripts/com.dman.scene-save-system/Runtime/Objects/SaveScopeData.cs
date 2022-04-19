using Dman.SceneSaveSystem.Objects.Identifiers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
                    _savedDict = new Dictionary<string, SaveData>();
                    foreach (var saveData in dataInScope)
                    {
                        if (_savedDict.ContainsKey(saveData.uniqueSaveDataId))
                        {
                            Debug.LogError($"More than one save data in scope with unique ID {saveData.uniqueSaveDataId}");
                            throw new SaveFormatException($"More than one save data in scope with unique ID {saveData.uniqueSaveDataId}");
                        }
                        _savedDict[saveData.uniqueSaveDataId] = saveData;
                    }
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

        public SaveScopeTreeIterator GetIterator()
        {
            return new SaveScopeTreeIterator(this);
        }

        public class SaveScopeTreeIterator : IEnumerator<SaveData>
        {
            private Stack<SaveScopeNodeIterationState> currentStack;
            private SaveScopeData topTree;

            private class SaveScopeNodeIterationState
            {
                public SaveScopeData Node { get; private set; }
                private int currentIndex;

                public SaveScopeNodeIterationState(SaveScopeData root)
                {
                    this.Node = root;
                    currentIndex = 0;
                }

                public bool HasSaveData()
                {
                    return currentIndex < Node.dataInScope.Count;
                }

                public SaveData GetNextSaveData()
                {
                    return Node.dataInScope[currentIndex++];
                }

                public bool HasChildren()
                {
                    return currentIndex < (Node.dataInScope.Count + Node.childScopes.Count);
                }
                public SaveScopeData GetNextChild()
                {
                    return Node.childScopes[(currentIndex++) - Node.dataInScope.Count];
                }
            }

            public SaveData Current { get; private set; }

            object IEnumerator.Current => Current;

            public SaveScopeTreeIterator(SaveScopeData topTree)
            {
                this.topTree = topTree;
                Reset();
            }

            public IEnumerable<SaveScopeData> CurrentStack()
            {
                return currentStack.Select(x => x.Node);
            }

            public bool MoveNext()
            {
                if(currentStack.Count <= 0)
                {
                    Current = null;
                    return false;
                }
                var currentNode = currentStack.Peek();
                if (currentNode.HasSaveData())
                {
                    Current = currentNode.GetNextSaveData();
                    return true;
                }
                else if (currentNode.HasChildren())
                {
                    var nextChild = currentNode.GetNextChild();
                    currentStack.Push(new SaveScopeNodeIterationState(nextChild));
                }else
                {
                    currentStack.Pop();
                }
                return MoveNext();
            }

            public void Reset()
            {
                currentStack = new Stack<SaveScopeNodeIterationState>();
                currentStack.Push(new SaveScopeNodeIterationState(topTree));
            }

            public void Dispose()
            {
                currentStack = null;
            }
        }


    }
}
