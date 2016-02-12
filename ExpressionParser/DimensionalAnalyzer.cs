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
			Action<IReadOnlyDictionary<string, int>, Func<AlgebraicFactor, AlgebraicFactor, AlgebraicFactor>> process =
				(terms, aggregate) =>
				{
					foreach (var pair in terms)
					{
						var dim = dimensionByUnit[pair.Key];
						ComposedPhysicalDimension cdimension;
						if (composedPhysicalDimensions.TryGetValue(dim.Name, out cdimension))
						{
							result = aggregate(result, cdimension.DimensionalDefinition);
						}
						else
						{
							var factor = AlgebraicFactor.FromSymbol(dim.Name, pair.Value);
							result = aggregate(result, factor);
						}
					}
				};

			process(unitsFactor.Numerator, (x, y) => x.Multiply(y));
			process(unitsFactor.Denominator, (x, y) => x.Divide(y));

			return result;
		}
		
		public bool AreDimensionalFactorsDimensionallyEquivalent(AlgebraicFactor dimensionalFactor1,
			AlgebraicFactor dimensionalFactor2)
		{
			var factor1 = GetFundamentalDimensionalFactorFromDimensionalFactor(dimensionalFactor1);
			var factor2 = GetFundamentalDimensionalFactorFromDimensionalFactor(dimensionalFactor2);
			return factor1.Equals(factor2);
		}

		public bool AreUnitFactorsDimensionallyEquivalent(AlgebraicFactor unitFactor1, AlgebraicFactor unitFactor2)
		{
			var factor1 = GetFundamentalDimensionalFactorFromUnitsFactor(unitFactor1);
			var factor2 = GetFundamentalDimensionalFactorFromUnitsFactor(unitFactor2);
			return factor1.Equals(factor2);
		}

		public ConversionParameters GetConversionParameters(AlgebraicFactor currentUnits, AlgebraicFactor targetUnits)
		{
			var currentToFundamental = GetConversionParametersToFundamentalUnits(currentUnits);
			var targetToFundamental = GetConversionParametersToFundamentalUnits(targetUnits);
			var fundamentalToTarget = InvertConversionParameters(targetToFundamental);
			var currentToTarget = ComposeConversionParameters(currentToFundamental, fundamentalToTarget);
			return currentToTarget;
		}

		private ConversionParameters InvertConversionParameters(ConversionParameters parameters)
		{
			return  new ConversionParameters(1.0/parameters.Factor, -parameters.Offset/parameters.Factor);
		}

		private ConversionParameters GetConversionParametersToFundamentalUnits(AlgebraicFactor unitsFactor)
		{
			var conversionParams = new ConversionParameters(1.0, 0);
			Action<IReadOnlyDictionary<string, int>, Func<ConversionParameters, ConversionParameters, ConversionParameters>> process =
				(terms, aggregate) =>
				{
					foreach (var pair in terms)
					{
						var unit = pair.Key;
						var dim = dimensionByUnit[unit];
						ComposedPhysicalDimension cdimension;
						if (composedPhysicalDimensions.TryGetValue(dim.Name, out cdimension))
						{
							var cparams = cdimension.Multiples[unit];

							conversionParams = aggregate(conversionParams, cparams);
							conversionParams = aggregate(conversionParams, cdimension.ConversionParameters);
						}
						else
						{
							var cparams = dim.Multiples[unit];
							conversionParams = aggregate(conversionParams, cparams);
						}
					}
				};

			process(unitsFactor.Numerator, ComposeConversionParameters);
			process(unitsFactor.Denominator, (x,y) => ComposeConversionParameters(x, InvertConversionParameters(y)));

			return conversionParams;
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
			Action<IReadOnlyDictionary<string, int>, Func<AlgebraicFactor, AlgebraicFactor, AlgebraicFactor>> process =
				(terms, aggregate) =>
				{
					foreach (var pair in terms)
					{
						ComposedPhysicalDimension cdimension;
						if (composedPhysicalDimensions.TryGetValue(pair.Key, out cdimension))
						{
							result = aggregate(result, cdimension.DimensionalDefinition);
						}
						else
						{
							var dim = fundamentalPhysicalDimensions[pair.Key];
							var factor = AlgebraicFactor.FromSymbol(dim.Name, pair.Value);
							result = aggregate(result, factor);
						}
					}
				};

			process(dimensionalFactor.Numerator, (x, y) => x.Multiply(y));
			process(dimensionalFactor.Denominator, (x, y) => x.Divide(y));

			return result;
		}

		public double ApplyConversion(double quantity, ConversionParameters conversionParams)
		{
			return quantity*conversionParams.Factor + conversionParams.Offset;
		}

		public ConversionParameters ComposeConversionParameters(ConversionParameters p1, ConversionParameters p2)
		{
			return new ConversionParameters(p1.Factor*p2.Factor, p1.Offset*p2.Factor + p2.Offset);
		}
	}
}