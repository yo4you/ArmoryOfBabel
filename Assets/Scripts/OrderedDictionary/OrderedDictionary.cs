// http://unlicense.org
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace mattmc3.Common.Collections.Generic
{
	/// <summary>
	/// A dictionary object that allows rapid hash lookups using keys, but also
	/// maintains the key insertion order so that values can be retrieved by
	/// key index.
	/// </summary>
	public class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>
	{
		#region Fields/Properties

		private KeyedCollection2<TKey, KeyValuePair<TKey, TValue>> _keyedCollection;

		public IEqualityComparer<TKey> Comparer
		{
			get;
			private set;
		}

		public int Count => _keyedCollection.Count;

		public ICollection<TKey> Keys => _keyedCollection.Select(x => x.Key).ToList();

		public ICollection<TValue> Values => _keyedCollection.Select(x => x.Value).ToList();

		/// <summary>
		/// Gets or sets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key associated with the value to get or set.</param>
		public TValue this[TKey key]
		{
			get => GetValue(key);
			set => SetValue(key, value);
		}

		/// <summary>
		/// Gets or sets the value at the specified index.
		/// </summary>
		/// <param name="index">The index of the value to get or set.</param>
		public TValue this[int index]
		{
			get => GetItem(index).Value;
			set => SetItem(index, value);
		}

		#endregion Fields/Properties

		#region Constructors

		public OrderedDictionary()
		{
			Initialize();
		}

		public OrderedDictionary(IEqualityComparer<TKey> comparer)
		{
			Initialize(comparer);
		}

		public OrderedDictionary(IOrderedDictionary<TKey, TValue> dictionary)
		{
			Initialize();
			foreach (KeyValuePair<TKey, TValue> pair in dictionary)
			{
				_keyedCollection.Add(pair);
			}
		}

		public OrderedDictionary(IOrderedDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
		{
			Initialize(comparer);
			foreach (KeyValuePair<TKey, TValue> pair in dictionary)
			{
				_keyedCollection.Add(pair);
			}
		}

		#endregion Constructors

		#region Methods

		public void Add(TKey key, TValue value)
		{
			_keyedCollection.Add(new KeyValuePair<TKey, TValue>(key, value));
		}

		public void Clear()
		{
			_keyedCollection.Clear();
		}

		public bool ContainsKey(TKey key)
		{
			return _keyedCollection.Contains(key);
		}

		public bool ContainsValue(TValue value)
		{
			return Values.Contains(value);
		}

		public bool ContainsValue(TValue value, IEqualityComparer<TValue> comparer)
		{
			return Values.Contains(value, comparer);
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return _keyedCollection.GetEnumerator();
		}

		public KeyValuePair<TKey, TValue> GetItem(int index)
		{
			if (index < 0 || index >= _keyedCollection.Count)
			{
				throw new ArgumentException(String.Format("The index was outside the bounds of the dictionary: {0}", index));
			}
			return _keyedCollection[index];
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key associated with the value to get.</param>
		public TValue GetValue(TKey key)
		{
			if (_keyedCollection.Contains(key) == false)
			{
				throw new ArgumentException($"The given key is not present in the dictionary: {key}");
			}
			var kvp = _keyedCollection[key];
			return kvp.Value;
		}

		public int IndexOf(TKey key)
		{
			if (_keyedCollection.Contains(key))
			{
				return _keyedCollection.IndexOf(_keyedCollection[key]);
			}
			else
			{
				return -1;
			}
		}

		public void Insert(int index, TKey key, TValue value)
		{
			_keyedCollection.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
		}

		public bool Remove(TKey key)
		{
			return _keyedCollection.Remove(key);
		}

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _keyedCollection.Count)
			{
				throw new ArgumentException($"The index was outside the bounds of the dictionary: {index}");
			}
			_keyedCollection.RemoveAt(index);
		}

		/// <summary>
		/// Sets the value at the index specified.
		/// </summary>
		/// <param name="index">The index of the value desired</param>
		/// <param name="value">The value to set</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when the index specified does not refer to a KeyValuePair in this object
		/// </exception>
		public void SetItem(int index, TValue value)
		{
			if (index < 0 || index >= _keyedCollection.Count)
			{
				throw new ArgumentException($"The index is outside the bounds of the dictionary: {index}");
			}
			var kvp = new KeyValuePair<TKey, TValue>(_keyedCollection[index].Key, value);
			_keyedCollection[index] = kvp;
		}

		/// <summary>
		/// Sets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key associated with the value to set.</param>
		/// <param name="value">The the value to set.</param>
		public void SetValue(TKey key, TValue value)
		{
			var kvp = new KeyValuePair<TKey, TValue>(key, value);
			var idx = IndexOf(key);
			if (idx > -1)
			{
				_keyedCollection[idx] = kvp;
			}
			else
			{
				_keyedCollection.Add(kvp);
			}
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			if (_keyedCollection.Contains(key))
			{
				value = _keyedCollection[key].Value;
				return true;
			}
			else
			{
				value = default;
				return false;
			}
		}

		private void Initialize(IEqualityComparer<TKey> comparer = null)
		{
			Comparer = comparer;
			if (comparer != null)
			{
				_keyedCollection = new KeyedCollection2<TKey, KeyValuePair<TKey, TValue>>(x => x.Key, comparer);
			}
			else
			{
				_keyedCollection = new KeyedCollection2<TKey, KeyValuePair<TKey, TValue>>(x => x.Key);
			}
		}

		#endregion Methods

		#region sorting

		public void SortKeys()
		{
			_keyedCollection.SortByKeys();
		}

		public void SortKeys(IComparer<TKey> comparer)
		{
			_keyedCollection.SortByKeys(comparer);
		}

		public void SortKeys(Comparison<TKey> comparison)
		{
			_keyedCollection.SortByKeys(comparison);
		}

		public void SortValues()
		{
			var comparer = Comparer<TValue>.Default;
			SortValues(comparer);
		}

		public void SortValues(IComparer<TValue> comparer)
		{
			_keyedCollection.Sort((x, y) => comparer.Compare(x.Value, y.Value));
		}

		public void SortValues(Comparison<TValue> comparison)
		{
			_keyedCollection.Sort((x, y) => comparison(x.Value, y.Value));
		}

		#endregion sorting

		#region IDictionary<TKey, TValue>

		ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

		ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

		TValue IDictionary<TKey, TValue>.this[TKey key]
		{
			get => this[key];
			set => this[key] = value;
		}

		void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			Add(key, value);
		}

		bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
		{
			return ContainsKey(key);
		}

		bool IDictionary<TKey, TValue>.Remove(TKey key)
		{
			return Remove(key);
		}

		bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
		{
			return TryGetValue(key, out value);
		}

		#endregion IDictionary<TKey, TValue>

		#region ICollection<KeyValuePair<TKey, TValue>>

		int ICollection<KeyValuePair<TKey, TValue>>.Count => _keyedCollection.Count;

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
		{
			_keyedCollection.Add(item);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Clear()
		{
			_keyedCollection.Clear();
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
		{
			return _keyedCollection.Contains(item);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			_keyedCollection.CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			return _keyedCollection.Remove(item);
		}

		#endregion ICollection<KeyValuePair<TKey, TValue>>

		#region IEnumerable<KeyValuePair<TKey, TValue>>

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion IEnumerable<KeyValuePair<TKey, TValue>>

		#region IEnumerable

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion IEnumerable

		#region IOrderedDictionary

		object IOrderedDictionary.this[int index]
		{
			get => this[index];
			set => this[index] = (TValue)value;
		}

		IDictionaryEnumerator IOrderedDictionary.GetEnumerator()
		{
			return new DictionaryEnumerator<TKey, TValue>(this);
		}

		void IOrderedDictionary.Insert(int index, object key, object value)
		{
			Insert(index, (TKey)key, (TValue)value);
		}

		void IOrderedDictionary.RemoveAt(int index)
		{
			RemoveAt(index);
		}

		#endregion IOrderedDictionary

		#region IDictionary

		bool IDictionary.IsFixedSize => false;

		bool IDictionary.IsReadOnly => false;

		ICollection IDictionary.Keys => (ICollection)Keys;

		ICollection IDictionary.Values => (ICollection)Values;

		object IDictionary.this[object key]
		{
			get => this[(TKey)key];
			set => this[(TKey)key] = (TValue)value;
		}

		void IDictionary.Add(object key, object value)
		{
			Add((TKey)key, (TValue)value);
		}

		void IDictionary.Clear()
		{
			Clear();
		}

		bool IDictionary.Contains(object key)
		{
			return _keyedCollection.Contains((TKey)key);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator()
		{
			return new DictionaryEnumerator<TKey, TValue>(this);
		}

		void IDictionary.Remove(object key)
		{
			Remove((TKey)key);
		}

		#endregion IDictionary

		#region ICollection

		int ICollection.Count => ((ICollection)_keyedCollection).Count;

		bool ICollection.IsSynchronized => ((ICollection)_keyedCollection).IsSynchronized;

		object ICollection.SyncRoot => ((ICollection)_keyedCollection).SyncRoot;

		void ICollection.CopyTo(Array array, int index)
		{
			((ICollection)_keyedCollection).CopyTo(array, index);
		}

		#endregion ICollection
	}
}