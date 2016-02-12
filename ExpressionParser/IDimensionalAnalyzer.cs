using System.Collections.Generic;

namespace DXAppProto2
{
	public interface IDimensionalAnalyzer
	{
		IReadOnlyCollection<string> ComposedPhysicalDimensions { get; }

		IReadOnlyCollection<string> FundamentalPhysicalDimensions { get; }

		IComposedPhysicalDimension GetComposedPhysicalDimension(string name);

		IFundamentalPhysicalDimension GetFundamentalPhysicalDimension(string name);

		IPhysicalDimension GetPhysicalDimensionForMeasurementUnit(string measurementUnit);

		AlgebraicFactor GetFundamentalDimensionalFactorFromUnitsFactor(AlgebraicFactor unitsFactor);

		bool AreDimensionalFactorsDimensionallyEquivalent(AlgebraicFactor dimensionalFactor1, AlgebraicFactor dimensionalFactor2);

		bool AreUnitFactorsDimensionallyEquivalent(AlgebraicFactor unitFactor1, AlgebraicFactor unitFactor2);

		ConversionParameters GetConversionParameters(AlgebraicFactor currentUnits, AlgebraicFactor targetUnits);

		double ApplyConversion(double quantity, ConversionParameters conversionParams);
	}
}