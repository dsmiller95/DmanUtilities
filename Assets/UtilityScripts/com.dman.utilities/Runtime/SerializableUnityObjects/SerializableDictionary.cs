using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Dman.Utilities.SerializableUnityObjects
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue>: IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        public Dictionary<TKey, TValue> backingDictionary;

        public SerializableDictionary()
        {
            backingDictionary = new Dictionary<TKey, TValue>();
        }

        [SerializeField]
        private List<InternalKeyPair> keyValuePairs;

        [Serializable]
        class InternalKeyPair
        {
            public TKey key;
            public TValue value;
        }

        public void OnAfterDeserialize()
        {
            backingDictionary = keyValuePairs?
                .Where(x => x != null)
                .ToDictionary(x => x.key, x => x.value)
                ?? new Dictionary<TKey, TValue>();
        }

        public void OnBeforeSerialize()
        {
            keyValuePairs = backingDictionary?.Select(x => new InternalKeyPair
            {
                key = x.Key,
                value = x.Value
            }).ToList() ?? new List<InternalKeyPair>();
        }

        #region interface reimplementation
        public TValue this[TKey key] {
            get => ((IDictionary<TKey, TValue>)backingDictionary)[key];
            set => ((IDictionary<TKey, TValue>)backingDictionary)[key] = value; 
        }

        public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>)backingDictionary).Keys;

        public ICollection<TValue> Values => ((IDictionary<TKey, TValue>)backingDictionary).Values;

        public int Count => ((ICollection<KeyValuePair<TKey, TValue>>)backingDictionary).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)backingDictionary).IsReadOnly;

        public void Add(TKey key, TValue value)
        {
            ((IDictionary<TKey, TValue>)backingDictionary).Add(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)backingDictionary).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)backingDictionary).Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)backingDictionary).Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return ((IDictionary<TKey, TValue>)backingDictionary).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)backingDictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<TKey, TValue>>)backingDictionary).GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            return ((IDictionary<TKey, TValue>)backingDictionary).Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)backingDictionary).Remove(item);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return ((IDictionary<TKey, TValue>)backingDictionary).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)backingDictionary).GetEnumerator();
        }
        #endregion
    }
}
