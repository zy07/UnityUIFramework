using Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class DictionaryList<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _dataDic = new Dictionary<TKey, TValue>();
        private List<TKey> _dataKeyList = new List<TKey>();
        private List<TValue> _dataValueList = new List<TValue>();

        public int Count => _dataDic.Count;

        public List<TKey> KeyList => _dataKeyList;

        public List<TValue> ValueList => _dataValueList;

        public void Add(TKey key, TValue val)
        {
            _dataDic.Add(key, val);
            _dataKeyList.Add(key);
            _dataValueList.Add(val);
        }

        public void Remove(TKey key)
        {
            TValue val;
            if (_dataDic.ContainsKey(key))
            {
                val = _dataDic[key];
                _dataDic.Remove(key);
                if (_dataValueList.Contains(val))
                {
                    _dataValueList.Remove(val);
                }
            }

            if (_dataKeyList.Contains(key))
            {
                _dataKeyList.Remove(key);
            }


        }

        public bool TryGetValue(TKey key, out TValue val)
        {
            if (_dataDic.TryGetValue(key, out val))
            {
                return true;
            }

            return false;
        }

        public void Clear()
        {
            _dataDic.Clear();
            _dataKeyList.Clear();
            _dataValueList.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return _dataDic.ContainsKey(key);
        }

        int GetIdx(TKey key)
        {
            for (int i = 0; i < _dataKeyList.Count; i++)
            {
                if (_dataKeyList[i].Equals(key))
                {
                    return i;
                }
            }

            return -1;
        }

        public TValue this[TKey key]
        {
            set
            {
                _dataDic[key] = value;
                int idx = GetIdx(key);
                if (idx == -1)
                {
                    Debug.LogError($"key = {key} not found!");
                    return;
                }
                _dataValueList[idx] = value;
            }
            get => _dataDic[key];
        }
    }
}