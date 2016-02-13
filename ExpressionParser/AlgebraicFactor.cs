namespace DXAppProto2
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ListDictionary = System.Collections.Generic.Dictionary<string, int>;

	/// <summary>
	/// Represents an algebraic multiplicative factor
	/// </summary>
	public class AlgebraicFactor : IEquatable<AlgebraicFactor>
	{
		public static readonly AlgebraicFactor Dimensionless =
			new AlgebraicFactor(new ListDictionary(), new ListDictionary());

		/// <summary>
		/// Gets the numerator. Keys are measurement units, Values are power of measurement unit
		/// </summary>
		/// <value>The numerator.</value>
		public IReadOnlyDictionary<string, int> Numerator { get; }

		/// <summary>
		/// Gets the denominator. Keys are measurement units, Values are power of measurement unit
		/// </summary>
		/// <value>The denominator.</value>
		public IReadOnlyDictionary<string, int> Denominator { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="AlgebraicFactor"/> class.
		/// </summary>
		/// <param name="numerator">The numerator.</param>
		/// <param name="denominator">The denominator.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// </exception>
		public AlgebraicFactor(IEnumerable<KeyValuePair<string, int>> numerator,
			IEnumerable<KeyValuePair<string, int>> denominator)
		{
			this.Numerator = this.CreateDictionary(numerator);
			this.Denominator = this.CreateDictionary(denominator);
			if (this.Numerator.Any(x => x.Value <= 0))
			{
				throw new ArgumentOutOfRangeException(nameof(numerator));
			}

			if (this.Denominator.Any(x => x.Value <= 0))
			{
				throw new ArgumentOutOfRangeException(nameof(denominator));
			}
		}

		private AlgebraicFactor(ListDictionary numerator, ListDictionary denominator)
		{
			this.Numerator = numerator;
			this.Denominator = denominator;
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals(AlgebraicFactor other)
		{
			if (this.Numerator.Count != other.Numerator.Count || this.Denominator.Count != other.Denominator.Count) return false;
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

		/// <summary>
		/// Build a new factor with a single symbol
		/// </summary>
		/// <param name="symbol">The symbol.</param>
		/// <param name="power">The power.</param>
		/// <returns>The factor</returns>
		public static AlgebraicFactor FromSymbol(string symbol, int power = 1)
		{
			var num = new ListDictionary { { symbol, power } };
			var den = new ListDictionary(0);
			return new AlgebraicFactor(num, den);
		}

		/// <summary>
		/// Multiplies with other factor.
		/// </summary>
		/// <param name="other">The other factor.</param>
		/// <returns>The resulting factor</returns>
		public AlgebraicFactor Multiply(AlgebraicFactor other)
		{
			var nn = this.CreateDictionary(this.Numerator);
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

			var nd = this.CreateDictionary(this.Denominator);
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

		/// <summary>
		/// Divides with other factor
		/// </summary>
		/// <param name="other">The other factor.</param>
		/// <returns>The resulting factor</returns>
		public AlgebraicFactor Divide(AlgebraicFactor other)
		{
			var nn = this.CreateDictionary(this.Numerator);
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

			var nd = this.CreateDictionary(this.Denominator);
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

		/// <summary>
		/// Gets a new factor representing the inverse factor.
		/// </summary>
		/// <returns></returns>
		public AlgebraicFactor Inverse()
		{
			return new AlgebraicFactor(this.Denominator, this.Numerator);
		}
		
		/// <summary>
		/// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			var obj2 = obj as AlgebraicFactor;
			return obj2 != null && this.Equals(obj2);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return this.GetDictionaryHashCode(this.Numerator)*397 ^ this.GetDictionaryHashCode(this.Denominator);
		}
	}
}