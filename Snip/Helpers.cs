using System;
using System.Collections.Generic;

namespace Winter
{
    public static class Helpers
    {
        public static Dictionary<TKey, TValue> Clone<TKey, TValue>
            (this Dictionary<TKey, TValue> original)
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(original.Count,
                original.Comparer);
            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                ret.Add(entry.Key, (TValue) entry.Value);
            }
            return ret;
        }
    }
}