// http://unlicense.org
using System.Collections.Generic;
using System.Collections.Specialized;

namespace mattmc3.Common.Collections.Generic
{
	public interface IOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IOrderedDictionary
	{
		new int Count { get; }
		new ICollection<TKey> Keys { get; }
		new ICollection<TValue> Values { get; }
		new TValue this[int index] { get; set; }
		new TValue this[TKey key] { get; set; }

		new void Add(TKey key, TValue value);

		new void Clear();

		new bool ContainsKey(TKey key);

		bool ContainsValue(TValue value);

		bool ContainsValue(TValue value, IEqualityComparer<TValue> comparer);

		new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator();

		KeyValuePair<TKey, TValue> GetItem(int index);

		TValue GetValue(TKey key);

		int IndexOf(TKey key);

		void Insert(int index, TKey key, TValue value);

		new bool Remove(TKey key);

		new void RemoveAt(int index);

		void SetItem(int index, TValue value);

		void SetValue(TKey key, TValue value);

		new bool TryGetValue(TKey key, out TValue value);
	}
}
