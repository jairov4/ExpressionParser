namespace DXAppProto2
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Allows to analyze multiple expressions with measurement unit and physical dimension awareness.
	/// 
	/// This class differences two types of information contained in <see cref="AlgebraicFactor" />, sometimes
	/// an algebraic factor is an expression containing measurement units pe. kg*m/s^2. But, sometimes it
	/// is an expression of physical dimensions pe. mass*distance/time^2. They are called Units Factors and
	/// Dimensional Factors respectively.
	/// 
	/// This analyzer models two kinds of physical dimensions. <see cref="IFundamentalPhysicalDimension" /> are physical dimensions
	/// that you assume measure a very basic concept in nature. <see cref="IComposedPhysicalDimension" /> are physical dimensions
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
			this.composedPhysicalDimensions = new Dictionary<string, ComposedPhysicalDimension>();
			this.fundamentalPhysicalDimensions = new Dictionary<string, FundamentalPhysicalDimension>();
			this.dimensionByUnit = new Dictionary<string, IPhysicalDimension>();
			this.fundamentalMeasurementUnits = new HashSet<string>();
		}

		public IReadOnlyCollection<string> ComposedPhysicalDimensions => this.composedPhysicalDimensions.Keys;

		public IReadOnlyCollection<string> FundamentalPhysicalDimensions => this.fundamentalPhysicalDimensions.Keys;

		public IComposedPhysicalDimension GetComposedPhysicalDimension(string name)
		{
			return this.composedPhysicalDimensions[name];
		}

		public IFundamentalPhysicalDimension GetFundamentalPhysicalDimension(string name)
		{
			return this.fundamentalPhysicalDimensions[name];
		}

		public IPhysicalDimension GetPhysicalDimensionForMeasurementUnit(string measurementUnit)
		{
			return this.dimensionByUnit[measurementUnit];
		}

		public AlgebraicFactor GetFundamentalDimensionalFactorFromUnitsFactor(AlgebraicFactor unitsFactor)
		{
			var result = AlgebraicFactor.Dimensionless;
			Action<IReadOnlyDictionary<string, int>, Func<AlgebraicFactor, AlgebraicFactor, AlgebraicFactor>> process =
				(terms, aggregate) =>
				{
					foreach (var pair in terms)
					{
						var dim = this.dimensionByUnit[pair.Key];
						ComposedPhysicalDimension cdimension;
						if (this.composedPhysicalDimensions.TryGetValue(dim.Name, out cdimension))
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
			var factor1 = this.GetFundamentalDimensionalFactorFromDimensionalFactor(dimensionalFactor1);
			var factor2 = this.GetFundamentalDimensionalFactorFromDimensionalFactor(dimensionalFactor2);
			return factor1.Equals(factor2);
		}

		public bool AreUnitFactorsDimensionallyEquivalent(AlgebraicFactor unitFactor1, AlgebraicFactor unitFactor2)
		{
			var factor1 = this.GetFundamentalDimensionalFactorFromUnitsFactor(unitFactor1);
			var factor2 = this.GetFundamentalDimensionalFactorFromUnitsFactor(unitFactor2);
			return factor1.Equals(factor2);
		}

		public ConversionParameters GetConversionParameters(AlgebraicFactor currentUnits, AlgebraicFactor targetUnits)
		{
			var currentToFundamental = this.GetConversionParametersToFundamentalUnits(currentUnits);
			var targetToFundamental = this.GetConversionParametersToFundamentalUnits(targetUnits);
			var fundamentalToTarget = this.InvertConversionParameters(targetToFundamental);
			var currentToTarget = this.ComposeConversionParameters(currentToFundamental, fundamentalToTarget);
			return currentToTarget;
		}

		public double ApplyConversion(double quantity, ConversionParameters conversionParams)
		{
			return quantity*conversionParams.Factor + conversionParams.Offset;
		}

		private ConversionParameters InvertConversionParameters(ConversionParameters parameters)
		{
			return new ConversionParameters(1.0/parameters.Factor, -parameters.Offset/parameters.Factor);
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
						var dim = this.dimensionByUnit[unit];
						ComposedPhysicalDimension cdimension;
						if (this.composedPhysicalDimensions.TryGetValue(dim.Name, out cdimension))
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

			process(unitsFactor.Numerator, this.ComposeConversionParameters);
			process(unitsFactor.Denominator, (x, y) => this.ComposeConversionParameters(x, this.InvertConversionParameters(y)));

			return conversionParams;
		}

		public void AddFundamentalDimension(string dimensionName, string newFundamentalUnit)
		{
			if (this.fundamentalPhysicalDimensions.ContainsKey(dimensionName)) throw new ArgumentException(nameof(dimensionName));
			if (this.composedPhysicalDimensions.ContainsKey(dimensionName)) throw new ArgumentException(nameof(dimensionName));
			if (this.dimensionByUnit.ContainsKey(newFundamentalUnit)) throw new ArgumentException(nameof(newFundamentalUnit));

			var dimension = new FundamentalPhysicalDimension(dimensionName, newFundamentalUnit);
			this.fundamentalPhysicalDimensions.Add(dimensionName, dimension);
			this.dimensionByUnit.Add(newFundamentalUnit, dimension);
			this.fundamentalMeasurementUnits.Add(newFundamentalUnit);
		}

		public void AddComposedDimension(string dimensionName, string newUnit, AlgebraicFactor referenceFactor,
			ConversionParameters conversionParameters)
		{
			if (this.fundamentalPhysicalDimensions.ContainsKey(dimensionName)) throw new ArgumentException(nameof(dimensionName));
			if (this.composedPhysicalDimensions.ContainsKey(dimensionName)) throw new ArgumentException(nameof(dimensionName));
			if (this.dimensionByUnit.ContainsKey(newUnit)) throw new ArgumentException(nameof(newUnit));
			if (!referenceFactor.Numerator.All(x => this.fundamentalMeasurementUnits.Contains(x.Key)))
				throw new ArgumentException(nameof(referenceFactor));
			if (!referenceFactor.Denominator.All(x => this.fundamentalMeasurementUnits.Contains(x.Key)))
				throw new ArgumentException(nameof(referenceFactor));

			var dimensionalDefinition = this.GetFundamentalDimensionalFactorFromUnitsFactor(referenceFactor);
			var dimension = new ComposedPhysicalDimension(dimensionName, dimensionalDefinition, referenceFactor,
				conversionParameters, newUnit);
			this.composedPhysicalDimensions.Add(dimensionName, dimension);
			this.dimensionByUnit.Add(newUnit, dimension);
		}

		public void AddMultiplierMeasurementUnit(string newUnit, string basicUnit, ConversionParameters conversionParameters)
		{
			if (this.dimensionByUnit.ContainsKey(newUnit)) throw new ArgumentException(nameof(newUnit));
			if (!this.dimensionByUnit.ContainsKey(basicUnit)) throw new ArgumentException(nameof(basicUnit));

			var dim = this.dimensionByUnit[basicUnit];
			ComposedPhysicalDimension cdimension;
			if (this.composedPhysicalDimensions.TryGetValue(dim.Name, out cdimension))
			{
				cdimension.Multiples.Add(newUnit, conversionParameters);
				this.dimensionByUnit.Add(newUnit, cdimension);
			}
			else
			{
				var fdimension = this.fundamentalPhysicalDimensions[dim.Name];
				fdimension.Multiples.Add(newUnit, conversionParameters);
				this.dimensionByUnit.Add(newUnit, fdimension);
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
						if (this.composedPhysicalDimensions.TryGetValue(pair.Key, out cdimension))
						{
							result = aggregate(result, cdimension.DimensionalDefinition);
						}
						else
						{
							var dim = this.fundamentalPhysicalDimensions[pair.Key];
							var factor = AlgebraicFactor.FromSymbol(dim.Name, pair.Value);
							result = aggregate(result, factor);
						}
					}
				};

			process(dimensionalFactor.Numerator, (x, y) => x.Multiply(y));
			process(dimensionalFactor.Denominator, (x, y) => x.Divide(y));

			return result;
		}

		public ConversionParameters ComposeConversionParameters(ConversionParameters p1, ConversionParameters p2)
		{
			return new ConversionParameters(p1.Factor*p2.Factor, p1.Offset*p2.Factor + p2.Offset);
		}
	}
}