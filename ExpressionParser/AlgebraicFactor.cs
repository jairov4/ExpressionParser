﻿using System.Diagnostics;

namespace DXAppProto2
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ListDictionary = System.Collections.Generic.Dictionary<string, int>;

	public class AlgebraicFactor : IEquatable<AlgebraicFactor>
	{
		public static readonly AlgebraicFactor Dimensionless =
			new AlgebraicFactor(new ListDictionary(), new ListDictionary());

		public AlgebraicFactor(IEnumerable<KeyValuePair<string, int>> numerator,
			IEnumerable<KeyValuePair<string, int>> denominator)
		{
			Numerator = CreateDictionary(numerator);
			Denominator = CreateDictionary(denominator);
			IsDimensionless = this.DictionaryEquals(this.Numerator, this.Denominator);
			if (Numerator.Any(x => x.Value <= 0))
			{
				throw new ArgumentOutOfRangeException(nameof(numerator));
			}

			if (Denominator.Any(x => x.Value <= 0))
			{
				throw new ArgumentOutOfRangeException(nameof(denominator));
			}
		}

		private AlgebraicFactor(ListDictionary numerator, ListDictionary denominator)
		{
			Numerator = numerator;
			Denominator = denominator;
			IsDimensionless = this.DictionaryEquals(this.Numerator, this.Denominator);
		}

		/// <summary>
		/// Gets the numerator. Keys are measurement units, Values are power of measurement unit
		/// </summary>
		/// <value>
		/// The numerator.
		/// </value>
		public IReadOnlyDictionary<string, int> Numerator { get; }

		/// <summary>
		/// Gets the denominator. Keys are measurement units, Values are power of measurement unit
		/// </summary>
		/// <value>
		/// The denominator.
		/// </value>
		public IReadOnlyDictionary<string, int> Denominator { get; }

		public bool IsDimensionless { get; }

		public bool Equals(AlgebraicFactor other)
		{
			if (Numerator.Count != other.Numerator.Count || Denominator.Count != other.Denominator.Count) return false;
			var r = this.DictionaryEquals(this.Numerator, other.Numerator);
			if (!r) return false;
			r = this.DictionaryEquals(this.Denominator, other.Denominator);
			return r;
		}

		private ListDictionary CreateDictionary(IEnumerable<KeyValuePair<string, int>> seq)
		{
			return seq.ToDictionary(x => x.Key, x => x.Value);
		}

		private bool DictionaryEquals(IReadOnlyDictionary<string, int> a, IReadOnlyDictionary<string, int> b)
		{
			return a.OrderBy(x => x.Key).SequenceEqual(b.OrderBy(x => x.Key));
		}

		private int GetDictionaryHashCode(IReadOnlyDictionary<string, int> d)
		{
			return d.Aggregate(397, (current, pair) => current ^ pair.GetHashCode());
		}

		public static AlgebraicFactor FromSymbol(string symbol, int power = 1)
		{
			var num = new ListDictionary {{symbol, power}};
			var den = new ListDictionary(0);
			return new AlgebraicFactor(num, den);
		}

		public AlgebraicFactor Multiply(AlgebraicFactor other)
		{
			var nn = CreateDictionary(Numerator);
			foreach (var nni in other.Numerator)
			{
				if (nn.ContainsKey(nni.Key))
				{
					nn[nni.Key] += nni.Value;
				}
				else
				{
					nn.Add(nni.Key, nni.Value);
				}
			}

			var nd = CreateDictionary(Denominator);
			foreach (var ndi in other.Denominator)
			{
				if (nd.ContainsKey(ndi.Key))
				{
					nd[ndi.Key] += ndi.Value;
				}
				else
				{
					nd.Add(ndi.Key, ndi.Value);
				}
			}

			var n = new AlgebraicFactor(nn, nd);
			return n;
		}


		public AlgebraicFactor Divide(AlgebraicFactor other)
		{
			var nn = CreateDictionary(Numerator);
			foreach (var nni in other.Denominator)
			{
				if (nn.ContainsKey(nni.Key))
				{
					nn[nni.Key] += nni.Value;
				}
				else
				{
					nn.Add(nni.Key, nni.Value);
				}
			}

			var nd = CreateDictionary(Denominator);
			foreach (var ndi in other.Numerator)
			{
				if (nd.ContainsKey(ndi.Key))
				{
					nd[ndi.Key] += ndi.Value;
				}
				else
				{
					nd.Add(ndi.Key, ndi.Value);
				}
			}

			var n = new AlgebraicFactor(nn, nd);
			return n;
		}

		public AlgebraicFactor Inverse()
		{
			return new AlgebraicFactor(this.Denominator, this.Numerator);
		}

		public override bool Equals(object obj)
		{
			var obj2 = obj as AlgebraicFactor;
			return obj2 != null && Equals(obj2);
		}

		public override int GetHashCode()
		{
			return GetDictionaryHashCode(this.Numerator)*397 ^ GetDictionaryHashCode(this.Denominator);
		}
	}
}