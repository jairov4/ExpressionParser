using System;
using System.Collections.Generic;
using System.Linq;

namespace DXAppProto2
{
	/// <summary>
	/// Allows to analyze multiple expressions with measurement unit and physical dimension awareness.
	/// 
	/// This class differences two types of information contained in <see cref="AlgebraicFactor"/>, sometimes
	/// an algebraic factor is an expression containing measurement units pe. kg*m/s^2. But, sometimes it
	/// is an expression of physical dimensions pe. mass*distance/time^2. They are called Units Factors and 
	/// Dimensional Factors respectively.
	/// 
	/// This analyzer models two kinds of physical dimensions. <see cref="IFundamentalPhysicalDimension"/>  are physical dimensions
	/// that you assume measure a very basic concept in nature. <see cref="IComposedPhysicalDimension"/> are physical dimensions 
	/// that you can express in terms of other physical dimensions.
	/// </summary>
	public class DimensionalAnalyzer : IDimensionalAnalyzer
	{
		private readonly Dictionary<string, ComposedPhysicalDimension> composedPhysicalDimensions;

		private readonly Dictionary<string, IPhysicalDimension> dimensionByUnit;

		private readonly HashSet<string> fundamentalMeasurementUnits;

		private readonly Dictionary<string, FundamentalPhysicalDimension> fundamentalPhysicalDimensions;

		public DimensionalAnalyzer()
		{
			composedPhysicalDimensions = new Dictionary<string, ComposedPhysicalDimension>();
			fundamentalPhysicalDimensions = new Dictionary<string, FundamentalPhysicalDimension>();
			dimensionByUnit = new Dictionary<string, IPhysicalDimension>();
			fundamentalMeasurementUnits = new HashSet<string>();
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

		public AlgebraicFactor ReplaceDimension(AlgebraicFactor dimensionalFactor, string dimension, string newDimension)
		{
			throw new NotImplementedException();
		}

		public bool AreDimensionalFactorsDimensionallyEquivalent(AlgebraicFactor dimensionalFactor1,
			AlgebraicFactor dimensionalFactor2)
		{
			var factor1 = GetFundamentalDimensionalFactorFromDimensionalFactor(dimensionalFactor1);
			var factor2 = GetFundamentalDimensionalFactorFromDimensionalFactor(dimensionalFactor1);
			return factor1.Equals(factor2);
		}

		public bool AreUnitFactorsDimensionallyEquivalent(AlgebraicFactor unitFactor1, AlgebraicFactor unitFactor2)
		{
			var factor1 = GetFundamentalDimensionalFactorFromUnitsFactor(unitFactor1);
			var factor2 = GetFundamentalDimensionalFactorFromUnitsFactor(unitFactor2);
			return factor1.Equals(factor2);
		}

		public double ConvertUnits(double quantity, AlgebraicFactor currentUnits, AlgebraicFactor targetUnits)
		{
			var conversionParams = new ConversionParameters(1.0, 0);
			var fundamentalUnits = AlgebraicFactor.Dimensionless;
			foreach (var pair in currentUnits.Numerator)
			{
				var unit = pair.Key;
				var dim = dimensionByUnit[unit];
				ComposedPhysicalDimension cdimension;
				if (composedPhysicalDimensions.TryGetValue(dim.Name, out cdimension))
				{
					var cparams = cdimension.Multiples[unit];
					conversionParams = ComposeConversionParameters(conversionParams, cparams);
					conversionParams = ComposeConversionParameters(conversionParams, cdimension.ConversionParameters);
					fundamentalUnits = fundamentalUnits.Multiply(cdimension.ReferenceFactor);
				}
				else
				{
					var cparams = dim.Multiples[unit];
					conversionParams = ComposeConversionParameters(conversionParams, cparams);
				}
			}

			foreach (var pair in currentUnits.Denominator)
			{
				var unit = pair.Key;
				var dim = dimensionByUnit[unit];
				ComposedPhysicalDimension cdimension;
				if (composedPhysicalDimensions.TryGetValue(dim.Name, out cdimension))
				{
					var cparams = cdimension.Multiples[unit];
					conversionParams = ComposeConversionParameters(conversionParams, cparams);
					conversionParams = ComposeConversionParameters(conversionParams, cdimension.ConversionParameters);
					fundamentalUnits = fundamentalUnits.Multiply(cdimension.ReferenceFactor);
				}
				else
				{
					var cparams = dim.Multiples[unit];
					conversionParams = ComposeConversionParameters(conversionParams, cparams);
				}
			}

			return ApplyConversion(quantity, conversionParams);
		}

		public void AddFundamentalDimension(string dimensionName, string newFundamentalUnit)
		{
			if (fundamentalPhysicalDimensions.ContainsKey(dimensionName)) throw new ArgumentException(nameof(dimensionName));
			if (composedPhysicalDimensions.ContainsKey(dimensionName)) throw new ArgumentException(nameof(dimensionName));
			if (dimensionByUnit.ContainsKey(newFundamentalUnit)) throw new ArgumentException(nameof(newFundamentalUnit));

			var dimension = new FundamentalPhysicalDimension(dimensionName, newFundamentalUnit);
			fundamentalPhysicalDimensions.Add(dimensionName, dimension);
			dimensionByUnit.Add(newFundamentalUnit, dimension);
			fundamentalMeasurementUnits.Add(newFundamentalUnit);
		}

		public void AddComposedDimension(string dimensionName, string newUnit, AlgebraicFactor referenceFactor,
			ConversionParameters conversionParameters)
		{
			if (fundamentalPhysicalDimensions.ContainsKey(dimensionName)) throw new ArgumentException(nameof(dimensionName));
			if (composedPhysicalDimensions.ContainsKey(dimensionName)) throw new ArgumentException(nameof(dimensionName));
			if (dimensionByUnit.ContainsKey(newUnit)) throw new ArgumentException(nameof(newUnit));
			if (!referenceFactor.Numerator.All(x => fundamentalMeasurementUnits.Contains(x.Key)))
				throw new ArgumentException(nameof(referenceFactor));
			if (!referenceFactor.Denominator.All(x => fundamentalMeasurementUnits.Contains(x.Key)))
				throw new ArgumentException(nameof(referenceFactor));

			var dimensionalDefinition = GetFundamentalDimensionalFactorFromUnitsFactor(referenceFactor);
			var dimension = new ComposedPhysicalDimension(dimensionName, dimensionalDefinition, referenceFactor,
				conversionParameters, newUnit);
			composedPhysicalDimensions.Add(dimensionName, dimension);
			dimensionByUnit.Add(newUnit, dimension);
		}

		public void AddMultiplierMeasurementUnit(string newUnit, string basicUnit, ConversionParameters conversionParameters)
		{
			if (dimensionByUnit.ContainsKey(newUnit)) throw new ArgumentException(nameof(newUnit));
			if (!dimensionByUnit.ContainsKey(basicUnit)) throw new ArgumentException(nameof(basicUnit));

			var dim = dimensionByUnit[basicUnit];
			ComposedPhysicalDimension cdimension;
			if (composedPhysicalDimensions.TryGetValue(dim.Name, out cdimension))
			{
				cdimension.Multiples.Add(newUnit, conversionParameters);
				dimensionByUnit.Add(newUnit, cdimension);
			}
			else
			{
				var fdimension = fundamentalPhysicalDimensions[dim.Name];
				fdimension.Multiples.Add(newUnit, conversionParameters);
				dimensionByUnit.Add(newUnit, fdimension);
			}
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

		private double ApplyConversion(double quantity, ConversionParameters conversionParams)
		{
			return quantity*conversionParams.Factor + conversionParams.Offset;
		}

		public ConversionParameters ComposeConversionParameters(ConversionParameters p1, ConversionParameters p2)
		{
			return new ConversionParameters(p1.Factor*p2.Factor, p1.Offset*p2.Factor + p2.Offset);
		}
	}
}