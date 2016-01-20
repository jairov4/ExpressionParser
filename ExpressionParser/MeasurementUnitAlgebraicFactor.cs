using System.Collections.Generic;
using System.Linq;

namespace DXAppProto2
{
	public class MeasurementUnitAlgebraicFactor
	{
		public MeasurementUnitAlgebraicFactor(IEnumerable<KeyValuePair<string, int>> numerator, IEnumerable<KeyValuePair<string, int>> denominator)
		{
			Numerator = numerator.ToDictionary(x => x.Key, x => x.Value);
			Denominator = denominator.ToDictionary(x => x.Key, x => x.Value);
		}

		private MeasurementUnitAlgebraicFactor(Dictionary<string, int> numerator, Dictionary<string, int> denominator)
		{
			Numerator = numerator;
			Denominator = denominator;
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

		public bool IsDimensionless => !Numerator.Any() && !Denominator.Any();

		public static MeasurementUnitAlgebraicFactor FromSingleUnit(string measurementUnit)
		{
			var num = new Dictionary<string, int>() {{measurementUnit, 1}};
			var den = new Dictionary<string, int>(0);
			return new MeasurementUnitAlgebraicFactor(num, den);
		}

		public MeasurementUnitAlgebraicFactor Multiply(MeasurementUnitAlgebraicFactor other)
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

			var n = new MeasurementUnitAlgebraicFactor(nn, nd);
			return n;
		}


		public MeasurementUnitAlgebraicFactor Divide(MeasurementUnitAlgebraicFactor other)
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

			var n = new MeasurementUnitAlgebraicFactor(nn, nd);
			return n;
		}

		public static readonly MeasurementUnitAlgebraicFactor Dimensionless = new MeasurementUnitAlgebraicFactor(new Dictionary<string, int>(), new Dictionary<string, int>());
	}
}