using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DXAppProto2
{
	public class ListDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
	{
		private readonly IEqualityComparer<TKey> keyComparer;
		private readonly List<KeyValuePair<TKey, TValue>> list;

		public ListDictionary(IEnumerable<KeyValuePair<TKey, TValue>> initialContents, IEqualityComparer<TKey> keyComparer)
		{
			this.list = new List<KeyValuePair<TKey, TValue>>(initialContents);
			this.keyComparer = keyComparer;
			this.Keys = new SubCollection<TKey>(keyComparer, () => this.Count, () => list.Select(x => x.Key).GetEnumerator());
			this.Values = new SubCollection<TValue>(EqualityComparer<TValue>.Default, () => this.Count, () => list.Select(x => x.Value).GetEnumerator());
			if (this.list.Count > 20)
			{
				Trace.TraceWarning("ListDictionary instance with many items, consider use Dictionary instead");
			}
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
			if (capacity > 20)
			{
				Trace.TraceWarning("ListDictionary instance with high capacity, consider use Dictionary instead");
			}

			this.list = new List<KeyValuePair<TKey, TValue>>(capacity);
			this.keyComparer = keyComparer;
			this.Keys = new SubCollection<TKey>(keyComparer, () => this.Count, () => this.list.Select(x => x.Key).GetEnumerator());
			this.Values = new SubCollection<TValue>(EqualityComparer<TValue>.Default, () => this.Count, () => this.list.Select(x => x.Value).GetEnumerator());
		}

		public ListDictionary(int capacity) : this(capacity, EqualityComparer<TKey>.Default)
		{
		}

		public ListDictionary(IEqualityComparer<TKey> keyComparer) : this(0, keyComparer)
		{
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return this.list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			if (this.ContainsKey(item.Key))
			{
				throw new ArgumentException();
			}

			this.list.Add(item);
		}

		public void Clear()
		{
			this.list.Clear();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return this.list.Contains(item);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			list.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return this.list.Remove(item);
		}

		public int Count => this.list.Count;

		public bool IsReadOnly => false;

		public bool ContainsKey(TKey key)
		{
			return this.list.Exists(x => keyComparer.Equals(x.Key, key));
		}

		public void Add(TKey key, TValue value)
		{
			this.Add(new KeyValuePair<TKey, TValue>(key, value));
		}

		public bool Remove(TKey key)
		{
			var i = this.list.FindIndex(x => keyComparer.Equals(x.Key, key));
			if (i < 0) return false;
			this.list.RemoveAt(i);
			return true;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			var i = this.list.FindIndex(x => keyComparer.Equals(x.Key, key));
			if (i < 0)
			{
				value = default(TValue);
				return false;
			}

			value = this.list[i].Value;
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
				var i = this.list.FindIndex(x => keyComparer.Equals(x.Key, key));
				if (i < 0)
				{
					this.list.Add(new KeyValuePair<TKey, TValue>(key, value));
				}
				else
				{
					this.list[i] = new KeyValuePair<TKey, TValue>(key, value);
				}
			}
		}

		public ICollection<TKey> Keys { get; }

		public ICollection<TValue> Values { get; }

		IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
		{
			get { return this.Keys; }
		}

		IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
		{
			get { return this.Values; }
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

	public static class EnumerableExtensions
	{
		public static ListDictionary<TKey, TValue> ToListDictionary<T, TKey, TValue>(this IEnumerable<T> seq,
			Func<T, TKey> keySelector, Func<T, TValue> valueSelector)
		{
			return new ListDictionary<TKey, TValue>(seq.Select(x => new KeyValuePair<TKey, TValue>(keySelector(x), valueSelector(x))));
		}
	}
}