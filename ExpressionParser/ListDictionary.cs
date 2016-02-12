using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DXAppProto2
{
	public class ListDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
	{
		private readonly IEqualityComparer<TKey> keyComparer;
		private readonly List<KeyValuePair<TKey, TValue>> list;

		public ListDictionary(IEnumerable<KeyValuePair<TKey, TValue>> initialContents, IEqualityComparer<TKey> keyComparer)
		{
			list = new List<KeyValuePair<TKey, TValue>>(initialContents);
			this.keyComparer = keyComparer;
			Keys = new SubCollection<TKey>(keyComparer, () => this.Count, () => list.Select(x => x.Key).GetEnumerator());
			Values = new SubCollection<TValue>(EqualityComparer<TValue>.Default, () => this.Count, () => list.Select(x => x.Value).GetEnumerator());
		}

		public ListDictionary(IEnumerable<KeyValuePair<TKey, TValue>> initialContents)
			: this(initialContents, EqualityComparer<TKey>.Default)
		{
		}

		public ListDictionary() : this(Enumerable.Empty<KeyValuePair<TKey, TValue>>())
		{
		}

		public ListDictionary(int capacity, IEqualityComparer<TKey> keyComparer)
		{
			list = new List<KeyValuePair<TKey, TValue>>(capacity);
			this.keyComparer = keyComparer;
			Keys = new SubCollection<TKey>(keyComparer, () => this.Count, () => list.Select(x => x.Key).GetEnumerator());
			Values = new SubCollection<TValue>(EqualityComparer<TValue>.Default, () => this.Count, () => list.Select(x => x.Value).GetEnumerator());
		}

		public ListDictionary(int capacity) : this(capacity, EqualityComparer<TKey>.Default)
		{
		}

		public ListDictionary(IEqualityComparer<TKey> keyComparer) : this(0, keyComparer)
		{
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			if (ContainsKey(item.Key))
			{
				throw new ArgumentException();
			}
			list.Add(item);
		}

		public void Clear()
		{
			list.Clear();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return list.Contains(item);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			list.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return list.Remove(item);
		}

		public int Count => list.Count;

		public bool IsReadOnly => false;

		public bool ContainsKey(TKey key)
		{
			return list.Exists(x => keyComparer.Equals(x.Key, key));
		}

		public void Add(TKey key, TValue value)
		{
			Add(new KeyValuePair<TKey, TValue>(key, value));
		}

		public bool Remove(TKey key)
		{
			var i = list.FindIndex(x => keyComparer.Equals(x.Key, key));
			if (i < 0) return false;
			list.RemoveAt(i);
			return true;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			var i = list.FindIndex(x => keyComparer.Equals(x.Key, key));
			if (i < 0)
			{
				value = default(TValue);
				return false;
			}

			value = list[i].Value;
			return true;
		}

		public TValue this[TKey key]
		{
			get
			{
				TValue v;
				if (!TryGetValue(key, out v)) throw new KeyNotFoundException();
				return v;
			}

			set
			{
				var i = list.FindIndex(x => keyComparer.Equals(x.Key, key));
				if (i < 0)
				{
					list.Add(new KeyValuePair<TKey, TValue>(key, value));
				}
				else
				{
					list[i] = new KeyValuePair<TKey, TValue>(key, value);
				}
			}
		}

		public ICollection<TKey> Keys { get; }

		public ICollection<TValue> Values { get; }

		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
		{
			get { return Keys; }
		}

		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
		{
			get { return Values; }
		}

		private class SubCollection<T> : ICollection<T>
		{
			private readonly IEqualityComparer<T> comparer;
			private readonly Func<int> countFunc;
			private readonly Func<IEnumerator<T>> getEnumeratorFunc;

			public SubCollection(IEqualityComparer<T> comparer, Func<int> countFunc, Func<IEnumerator<T>> getEnumeratorFunc)
			{
				this.comparer = comparer;
				this.countFunc = countFunc;
				this.getEnumeratorFunc = getEnumeratorFunc;
			}

			public IEnumerator<T> GetEnumerator()
			{
				return getEnumeratorFunc();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public void Add(T item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				throw new NotSupportedException();
			}

			public bool Contains(T item)
			{
				return this.Any(x => comparer.Equals(x, item));
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				throw new NotSupportedException();
			}

			public bool Remove(T item)
			{
				throw new NotSupportedException();
			}

			public int Count => countFunc();

			public bool IsReadOnly => true;
		}
	}
}