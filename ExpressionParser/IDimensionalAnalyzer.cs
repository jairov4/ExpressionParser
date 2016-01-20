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
		
		AlgebraicFactor ToFundamentalDimensionalFactor(AlgebraicFactor unitsFactor);

		AlgebraicFactor ReplaceDimension(AlgebraicFactor dimensionalFactor, string dimension, string newDimension);

		bool AreDimensionallyEquivalent(AlgebraicFactor dimensionalFactor1, AlgebraicFactor dimensionalFactor2);

		double ConvertUnits(double quantity, AlgebraicFactor currentUnits, AlgebraicFactor targetUnits);
	}
}