using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RotaryHeart.Lib.SerializableDictionary
{
    public abstract class DrawableDictionary
    {
        [UnityEngine.HideInInspector]
        public ReorderableList reorderableList = null;
        [UnityEngine.HideInInspector]
        public RequiredReferences reqReferences;
        public bool isExpanded;
    }

    [System.Serializable]
    public class SerializableDictionaryBase<TKey, TValue> : DrawableDictionary, IDictionary<TKey, TValue>, UnityEngine.ISerializationCallbackReceiver
    {
        Dictionary<TKey, TValue> _dict;
        static readonly Dictionary<TKey, TValue> _staticEmptyDict = new Dictionary<TKey, TValue>(0);

        public void CopyFrom(IDictionary<TKey, TValue> src)
        {
            foreach (KeyValuePair<TKey, TValue> data in src)
            {
                if (ContainsKey(data.Key))
                {
                    this[data.Key] = data.Value;
                }
                else
                {
                    Add(data.Key, data.Value);
                }
            }
        }

        public void CopyFrom(object src)
        {
            Dictionary<TKey, TValue> dictionary = src as Dictionary<TKey, TValue>;
            if (dictionary != null)
            {
                CopyFrom(dictionary);
            }
        }

        public void CopyTo(IDictionary<TKey, TValue> dest)
        {
            foreach (KeyValuePair<TKey, TValue> data in this)
            {
                if (dest.ContainsKey(data.Key))
                {
                    dest[data.Key] = data.Value;
                }
                else
                {
                    dest.Add(data.Key, data.Value);
                }
            }
        }

        public Dictionary<TKey, TValue> Clone()
        {
            Dictionary<TKey, TValue> dest = new Dictionary<TKey, TValue>(Count);

            foreach (KeyValuePair<TKey, TValue> data in this)
            {
                dest.Add(data.Key, data.Value);
            }

            return dest;
        }

        public bool ContainsValue(TValue value)
        {
            if (_dict == null)
                return false;

            return _dict.ContainsValue(value);
        }

        #region IDictionary Interface

        #region Properties

        public TValue this[TKey key]
        {
            get
            {
                if (_dict == null) throw new KeyNotFoundException();
                return _dict[key];
            }
            set
            {
                if (_dict == null) _dict = new Dictionary<TKey, TValue>();
                _dict[key] = value;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                if (_dict == null)
                    _dict = new Dictionary<TKey, TValue>();

                return _dict.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                if (_dict == null)
                    _dict = new Dictionary<TKey, TValue>();

                return _dict.Values;
            }
        }

        public int Count
        {
            get
            {
                return (_dict != null) ? _dict.Count : 0;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        #endregion Properties


        public bool ContainsKey(TKey key)
        {
            if (_dict == null)
                return false;

            return _dict.ContainsKey(key);
        }

#if UNITY_EDITOR

        public void Add(TKey key, TValue value)
        {
            if (_dict == null)
                _dict = new Dictionary<TKey, TValue>();

            _dict.Add(key, value);

            if (_keyValues == null)
                _keyValues = new List<TKey>();
            if (_keys == null)
                _keys = new List<TKey>();
            if (_values == null)
                _values = new List<TValue>();

            _keyValues.Add(key);
            _keys.Add(key);
            _values.Add(value);
        }

        public void Clear()
        {
            if (_dict != null)
                _dict.Clear();

            if (_keyValues != null)
                _keyValues.Clear();
            if (_keys != null)
                _keys.Clear();
            if (_values != null)
                _values.Clear();
        }

        public bool Remove(TKey key)
        {
            if (_dict == null)
                return false;

            int index = -1;

            if (_keys != null)
            {
                index = _keys.IndexOf(key);

                if (index != -1)
                    _keys.RemoveAt(index);
            }

            if (index != -1)
            {
                if (_keyValues != null)
                    _keyValues.RemoveAt(index);

                if (_values != null)
                    _values.RemoveAt(index);
            }

            return _dict.Remove(key);
        }
#else

        public void Add(TKey key, TValue value)
        {
            if (_dict == null)
                _dict = new Dictionary<TKey, TValue>();

            _dict.Add(key, value);
        }

        public void Clear()
        {
            if (_dict != null)
                _dict.Clear();
        }

        public bool Remove(TKey key)
        {
            if (_dict == null)
                return false;

            return _dict.Remove(key);
        }
        
#endif

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_dict == null)
            {
                value = default(TValue);
                return false;
            }

            return _dict.TryGetValue(key, out value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            if (_dict == null) _dict = new Dictionary<TKey, TValue>();
            (_dict as ICollection<KeyValuePair<TKey, TValue>>).Add(item);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            if (_dict == null) return false;
            return (_dict as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (_dict == null) return;
            (_dict as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (_dict == null) return false;
            return (_dict as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
        }

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            if (_dict == null) return _staticEmptyDict.GetEnumerator();
            return _dict.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region ISerializationCallbackReceiver

        [SerializeField]
        List<TKey> _keyValues;

        [SerializeField]
        List<TKey> _keys;
        [SerializeField]
        List<TValue> _values;

#if UNITY_EDITOR

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (_keys != null && _values != null)
            {
                if (_dict == null)
                    _dict = new Dictionary<TKey, TValue>(_keys.Count);
                else
                    _dict.Clear();

                for (int i = 0; i < _keys.Count; i++)
                {
                    if (_keys[i] == null)
                    {
                        if (typeof(Object).IsAssignableFrom(typeof(TKey)))
                        {
                            string tKeyType = typeof(TKey).ToString();

                            if (reqReferences == null)
                            {
                                Debug.LogError("A key of type: " + tKeyType + " requires to have a valid RequiredReferences reference");
                                continue;
                            }

                            foreach (FieldInfo field in typeof(RequiredReferences).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                            {
                                if (field.FieldType.ToString().Equals(tKeyType))
                                {
                                    _keys[i] = (TKey)(field.GetValue(reqReferences));
                                    break;
                                }
                            }

                            if (_keys[i] == null)
                            {
                                Debug.LogError("Couldn't find " + tKeyType + " reference.");
                                continue;
                            }
                        }
                        else
                        {
                            _keys[i] = System.Activator.CreateInstance<TKey>();
                        }
                    }

                    if (i < _values.Count)
                        _dict[_keys[i]] = _values[i];
                    else
                        _dict[_keys[i]] = default(TValue);
                }
            }
        }

#else
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (_keys != null && _values != null)
            {
                //Need to clear the dictionary
                if (_dict == null)
                    _dict = new Dictionary<TKey, TValue>(_keys.Count);
                else
                    _dict.Clear();

                for (int i = 0; i < _keys.Count; i++)
                {
                    //This should only happen with reference type keys (Generic, Object, etc)
                    if (_keys[i] == null)
                    {
                        //Special case for UnityEngine.Object classes
                        if (typeof(Object).IsAssignableFrom(typeof(TKey)))
                        {
                            //Key type
                            string tKeyType = typeof(TKey).ToString();

                            //We need the reference to the reference holder class
                            if (reqReferences == null)
                            {
                                Debug.LogError("A key of type: " + tKeyType + " requires to have a valid RequiredReferences reference");
                                continue;
                            }

                            //Use reflection to check all the fields included on the class
                            foreach (FieldInfo field in typeof(RequiredReferences).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                            {
                                //Only set the value if the type is the same
                                if (field.FieldType.ToString().Equals(tKeyType))
                                {
                                    _keys[i] = (TKey)(field.GetValue(reqReferences));
                                    break;
                                }
                            }

                            //References class is missing the field, skip the element
                            if (_keys[i] == null)
                            {
                                Debug.LogError("Couldn't find " + tKeyType + " reference.");
                                continue;
                            }
                        }
                        else
                        {
                            //Create a instance for the key
                            _keys[i] = System.Activator.CreateInstance<TKey>();
                        }
                    }

                    //Add the data to the dictionary. Value can be null so no special step is required
                    if (i < _values.Count)
                        _dict[_keys[i]] = _values[i];
                    else
                        _dict[_keys[i]] = default(TValue);
                }
            }

            _keyValues = null;
            _keys = null;
            _values = null;
        }

#endif

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_dict == null || _dict.Count == 0)
            {
                _keyValues = null;
                _keys = null;
                _values = null;
            }
            else
            {
                int cnt = _dict.Count;
                _keyValues = new List<TKey>(cnt);
                _keys = new List<TKey>(cnt);
                _values = new List<TValue>(cnt);

                using (Dictionary<TKey, TValue>.Enumerator e = _dict.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        _keyValues.Add(e.Current.Key);
                        _keys.Add(e.Current.Key);
                        _values.Add(e.Current.Value);
                    }
                }
            }
        }

        #endregion

    }
}