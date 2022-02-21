﻿using System.Collections;

namespace SignalGo.Shared.Models
{
    public class UltraMapDictionary
    {
#if (PORTABLE)
        private Dictionary<object,object> Items { get; set; } = new Dictionary<object, object>();
#else
        private Hashtable Items { get; set; } = new Hashtable();
#endif
        private readonly object lockTable = new object();

        public int Count
        {
            get
            {
                return Items.Count;
            }
        }

        public void Add(object key, object value)
        {
            lock (lockTable)
            {
                Items.Add(key, value);
            }
        }

        public bool TryGetValue<T>(object key, out T value)
        {
            lock (lockTable)
            {
                if (Items.ContainsKey(key))
                {
                    value = (T)Items[key];
                    return true;
                }
                else
                {
                    value = default(T);
                    return false;
                }
            }
        }


        public void Remove(object key)
        {
            lock (lockTable)
            {
                Items.Remove(key);
            }
        }
    }
}
