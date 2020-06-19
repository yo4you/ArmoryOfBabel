// http://unlicense.org
using System;
using System.Collections;
using System.Collections.Generic;

namespace mattmc3.Common.Collections.Generic
{
	public class DictionaryEnumerator<TKey, TValue> : IDictionaryEnumerator, IDisposable
	{
		private readonly IEnumerator<KeyValuePair<TKey, TValue>> impl;

		public DictionaryEnumerator(IDictionary<TKey, TValue> value)
		{
			impl = value.GetEnumerator();
		}

		public object Current => Entry;

		public DictionaryEntry Entry
		{
			get
			{
				var pair = impl.Current;
				return new DictionaryEntry(pair.Key, pair.Value);
			}
		}

		public object Key => impl.Current.Key;

		public object Value => impl.Current.Value;

		public void Dispose()
		{
			impl.Dispose();
		}

		public bool MoveNext()
		{
			return impl.MoveNext();
		}

		public void Reset()
		{
			impl.Reset();
		}
	}
}