namespace DXAppProto2
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public class AlgebraicFactor : IEquatable<AlgebraicFactor>
	{
		public static readonly AlgebraicFactor Dimensionless =
			new AlgebraicFactor(new Dictionary<string, int>(), new Dictionary<string, int>());

		public AlgebraicFactor(IEnumerable<KeyValuePair<string, int>> numerator,
			IEnumerable<KeyValuePair<string, int>> denominator)
		{
			Numerator = numerator.ToDictionary(x => x.Key, x => x.Value);
			Denominator = denominator.ToDictionary(x => x.Key, x => x.Value);
			IsDimensionless = this.DictionaryEquals(this.Numerator, this.Denominator);
		}

		private AlgebraicFactor(Dictionary<string, int> numerator, Dictionary<string, int> denominator)
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

		private bool DictionaryEquals(IReadOnlyDictionary<string, int> a, IReadOnlyDictionary<string, int> b)
		{
			return a.OrderBy(x => x.Key).SequenceEqual(b.OrderBy(x => x.Key));
		}

		public static AlgebraicFactor FromSingleUnit(string measurementUnit)
		{
			var num = new Dictionary<string, int> {{measurementUnit, 1}};
			var den = new Dictionary<string, int>(0);
			return new AlgebraicFactor(num, den);
		}

		public AlgebraicFactor Multiply(AlgebraicFactor other)
		{
			var nn = Numerator.ToDictionary(x => x.Key, x => x.Value);
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

			var nd = Denominator.ToDictionary(x => x.Key, x => x.Value);
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
			var nn = Numerator.ToDictionary(x => x.Key, x => x.Value);
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

			var nd = Denominator.ToDictionary(x => x.Key, x => x.Value);
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
		
		public override bool Equals(object obj)
		{
			var obj2 = obj as AlgebraicFactor;
			return obj2 != null && Equals(obj2);
		}

		public override int GetHashCode()
		{
			return Numerator.GetHashCode()*397 ^ Denominator.GetHashCode();
		}
	}
}