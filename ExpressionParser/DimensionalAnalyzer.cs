using System;
using System.Linq;

namespace DXAppProto2
{
	using System.Collections.Generic;
	
	public class DimensionalAnalyzer : IDimensionalAnalyzer
	{
		private readonly Dictionary<string, ComposedPhysicalDimension> composedPhysicalDimensions;

		private readonly Dictionary<string, FundamentalPhysicalDimension> fundamentalPhysicalDimensions;

		private readonly Dictionary<string, IPhysicalDimension> dimensionByUnit;

		public DimensionalAnalyzer()
		{
			composedPhysicalDimensions = new Dictionary<string, ComposedPhysicalDimension>();
			fundamentalPhysicalDimensions = new Dictionary<string, FundamentalPhysicalDimension>();
			dimensionByUnit = new Dictionary<string, IPhysicalDimension>();
		}

		public void AddFundamentalDimension(string dimensionName, string newFundamentalUnit)
		{
			if (fundamentalPhysicalDimensions.ContainsKey(dimensionName)) throw new ArgumentException(nameof(dimensionName));
			if (composedPhysicalDimensions.ContainsKey(dimensionName)) throw new ArgumentException(nameof(dimensionName));
			if (dimensionByUnit.ContainsKey(newFundamentalUnit)) throw new ArgumentException(nameof(newFundamentalUnit));

			var dimension = new FundamentalPhysicalDimension(dimensionName, newFundamentalUnit);
			fundamentalPhysicalDimensions.Add(dimensionName, dimension);
			dimensionByUnit.Add(newFundamentalUnit, dimension);
		}

		public void AddComposedDimension(string dimensionName, string newUnit, AlgebraicFactor referenceFactor, ConversionParameters conversionParameters)
		{
			if (fundamentalPhysicalDimensions.ContainsKey(dimensionName)) throw new ArgumentException(nameof(dimensionName));
			if (composedPhysicalDimensions.ContainsKey(dimensionName)) throw new ArgumentException(nameof(dimensionName));
			if (dimensionByUnit.ContainsKey(newUnit)) throw new ArgumentException(nameof(newUnit));

			var dimensionalDefinition = this.GetFundamentalDimensionalFactorFromUnitsFactor(referenceFactor);
			var dimension =  new ComposedPhysicalDimension(dimensionName, dimensionalDefinition, referenceFactor, conversionParameters, newUnit);
			composedPhysicalDimensions.Add(dimensionName, dimension);
			dimensionByUnit.Add(newUnit, dimension);
		}

		public void AddMultiplierMeasurementUnit(string newUnit, string basicUnit, ConversionParameters conversionParameters)
		{
			if (dimensionByUnit.ContainsKey(newUnit)) throw new ArgumentException(nameof(newUnit));
			if (!dimensionByUnit.ContainsKey(basicUnit)) throw new ArgumentException(nameof(basicUnit));

			var dim = dimensionByUnit[basicUnit];
			ComposedPhysicalDimension cdimension;
			FundamentalPhysicalDimension fdimension;
			if (composedPhysicalDimensions.TryGetValue(dim.Name, out cdimension))
			{
				cdimension.Multiples.Add(newUnit, conversionParameters);
				dimensionByUnit.Add(newUnit, cdimension);
			}
			else if (fundamentalPhysicalDimensions.TryGetValue(dim.Name, out fdimension))
			{
				fdimension.Multiples.Add(newUnit, conversionParameters);
				dimensionByUnit.Add(newUnit, fdimension);
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		public IReadOnlyCollection<string> ComposedPhysicalDimensions => composedPhysicalDimensions.Keys;

		public IReadOnlyCollection<string> FundamentalPhysicalDimensions => fundamentalPhysicalDimensions.Keys;

		public IComposedPhysicalDimension GetComposedPhysicalDimension(string name)
		{
			return composedPhysicalDimensions[name];
		}

		public IFundamentalPhysicalDimension GetFundamentalPhysicalDimension(string name)
		{
			return fundamentalPhysicalDimensions[name];
		}

		public IPhysicalDimension GetPhysicalDimensionForMeasurementUnit(string measurementUnit)
		{
			return dimensionByUnit[measurementUnit];
		}
		
		public AlgebraicFactor GetFundamentalDimensionalFactorFromUnitsFactor(AlgebraicFactor unitsFactor)
		{
			var result = AlgebraicFactor.Dimensionless;
			foreach (var pair in unitsFactor.Numerator)
			{
				var dim = dimensionByUnit[pair.Key];
				ComposedPhysicalDimension cdimension;
				if (composedPhysicalDimensions.TryGetValue(dim.Name, out cdimension))
				{
					result = result.Multiply(cdimension.DimensionalDefinition);
				}
				else
				{
					var factor = AlgebraicFactor.FromSingleUnit(dim.Name);
					result = result.Multiply(factor);
				}
			}

			foreach (var pair in unitsFactor.Denominator)
			{
				var dim = dimensionByUnit[pair.Key];
				ComposedPhysicalDimension cdimension;
				if (composedPhysicalDimensions.TryGetValue(dim.Name, out cdimension))
				{
					result = result.Divide(cdimension.DimensionalDefinition);
				}
				else
				{
					var factor = AlgebraicFactor.FromSingleUnit(dim.Name);
					result = result.Divide(factor);
				}
			}

			return result;
		}

		public AlgebraicFactor GetFundamentalDimensionalFactorFromDimensionalFactor(AlgebraicFactor dimensionalFactor)
		{
			var result = AlgebraicFactor.Dimensionless;
			foreach (var pair in dimensionalFactor.Numerator)
			{
				ComposedPhysicalDimension cdimension;
				if (composedPhysicalDimensions.TryGetValue(pair.Key, out cdimension))
				{
					result = result.Multiply(cdimension.DimensionalDefinition);
				}
				else
				{
					var dim = fundamentalPhysicalDimensions[pair.Key];
					var factor = AlgebraicFactor.FromSingleUnit(dim.Name);
					result = result.Multiply(factor);
				}
			}

			foreach (var pair in dimensionalFactor.Denominator)
			{
				ComposedPhysicalDimension cdimension;
				if (composedPhysicalDimensions.TryGetValue(pair.Key, out cdimension))
				{
					result = result.Divide(cdimension.DimensionalDefinition);
				}
				else
				{
					var dim = fundamentalPhysicalDimensions[pair.Key];
					var factor = AlgebraicFactor.FromSingleUnit(dim.Name);
					result = result.Divide(factor);
				}
			}

			return result;
		}

		public AlgebraicFactor ReplaceDimension(AlgebraicFactor dimensionalFactor, string dimension, string newDimension)
		{
			throw new System.NotImplementedException();
		}

		public bool AreDimensionallyEquivalent(AlgebraicFactor dimensionalFactor1, AlgebraicFactor dimensionalFactor2)
		{
			var factor1 = GetFundamentalDimensionalFactorFromDimensionalFactor(dimensionalFactor1);
			var factor2 = GetFundamentalDimensionalFactorFromDimensionalFactor(dimensionalFactor1);
			return factor1.Equals(factor2);
		}

		public double ConvertUnits(double quantity, AlgebraicFactor currentUnits, AlgebraicFactor targetUnits)
		{
			throw new System.NotImplementedException();
		}
	}
}