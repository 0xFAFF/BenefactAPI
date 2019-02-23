using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenefactAPI
{
    public class AutoSortList<TKey, TValue> : ICollection<TValue>
    {
        private SortedList<TKey, TValue> list;
        private Func<TValue, TKey> keyFunc;
        public AutoSortList(Func<TValue, TKey> keyFunc)
        {
            list = new SortedList<TKey, TValue>();
            this.keyFunc = keyFunc;
        }
        public int Count => list.Count;
        public bool IsReadOnly => false;
        public void Add(TValue item) => list.Add(keyFunc(item), item);
        public bool Remove(TValue item)
        {
            var index = list.Values.IndexOf(item);
            if (index == -1) return false;
            list.RemoveAt(index);
            return true;
        }
        public void Clear() => list.Clear();
        public bool Contains(TValue item) => list.ContainsValue(item);
        public void CopyTo(TValue[] array, int arrayIndex) => list.Values.CopyTo(array, arrayIndex);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<TValue> GetEnumerator() => list.Values.GetEnumerator();
    }
}
