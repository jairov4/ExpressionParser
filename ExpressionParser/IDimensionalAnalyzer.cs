namespace DXAppProto2
{
	using System.Collections.Generic;

	/// <summary>
	/// <para>
	/// Allows to analyze expressions with measurement unit and physical dimension awareness.
	/// It provides the foundation to make algebraic manipulation over measurement units tagged expressions.
	/// </para>
	/// <para>
	/// This analyzer differences two types of information contained in <see cref="AlgebraicFactor" />, sometimes
	/// an algebraic factor is an expression containing measurement units pe. kg*m/s^2. But, sometimes it
	/// is an expression of physical dimensions pe. mass*distance/time^2. They are called Units Factors and
	/// Dimensional Factors respectively.
	/// </para>
	/// <para>
	/// This analyzer models two kinds of physical dimensions. <see cref="IFundamentalPhysicalDimension" /> are physical dimensions
	/// that you assume measure a very basic concept in nature. <see cref="IComposedPhysicalDimension" /> are physical dimensions
	/// that you can express in terms of other physical dimensions.
	/// </para>
	/// </summary>
	public interface IDimensionalAnalyzer
	{
		/// <summary>
		/// Gets the composed physical dimensions.
		/// </summary>
		/// <value>
		/// The composed physical dimensions.
		/// </value>
		IReadOnlyCollection<string> ComposedPhysicalDimensions { get; }

		/// <summary>
		/// Gets the fundamental physical dimensions.
		/// </summary>
		/// <value>
		/// The fundamental physical dimensions.
		/// </value>
		IReadOnlyCollection<string> FundamentalPhysicalDimensions { get; }

		/// <summary>
		/// Gets the composed physical dimension by its name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The composed physical dimension</returns>
		IComposedPhysicalDimension GetComposedPhysicalDimension(string name);

		/// <summary>
		/// Gets the fundamental physical dimension.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The fundamental physical dimension</returns>
		IFundamentalPhysicalDimension GetFundamentalPhysicalDimension(string name);

		/// <summary>
		/// Gets the physical dimension for measurement unit.
		/// </summary>
		/// <param name="measurementUnit">The measurement unit.</param>
		/// <returns>The physical dimension</returns>
		IPhysicalDimension GetPhysicalDimensionForMeasurementUnit(string measurementUnit);

		/// <summary>
		/// Gets the fundamental dimensional factor from units factor.
		/// </summary>
		/// <param name="unitsFactor">The units factor.</param>
		/// <returns>The units algebraic factor</returns>
		AlgebraicFactor GetFundamentalDimensionalFactorFromUnitsFactor(AlgebraicFactor unitsFactor);

		/// <summary>
		/// Indicates if the dimensional factors are dimensionally equivalent.
		/// </summary>
		/// <param name="dimensionalFactor1">The dimensional factor1.</param>
		/// <param name="dimensionalFactor2">The dimensional factor2.</param>
		/// <returns><c>True</c> on dimensionally equivalent factors, <c>False</c> otherwise</returns>
		bool AreDimensionalFactorsDimensionallyEquivalent(AlgebraicFactor dimensionalFactor1, AlgebraicFactor dimensionalFactor2);

		/// <summary>
		/// Indicates if the unit factors are dimensionally equivalent.
		/// </summary>
		/// <param name="unitFactor1">The unit factor1.</param>
		/// <param name="unitFactor2">The unit factor2.</param>
		/// <returns><c>True</c> on dimensionally equivalent factors, <c>False</c> otherwise</returns>
		bool AreUnitFactorsDimensionallyEquivalent(AlgebraicFactor unitFactor1, AlgebraicFactor unitFactor2);

		/// <summary>
		/// Gets the conversion parameters.
		/// </summary>
		/// <param name="currentUnits">The current units.</param>
		/// <param name="targetUnits">The target units.</param>
		/// <returns></returns>
		ConversionParameters GetConversionParameters(AlgebraicFactor currentUnits, AlgebraicFactor targetUnits);

		/// <summary>
		/// Applies a given conversion to a quantity.
		/// </summary>
		/// <param name="quantity">The quantity.</param>
		/// <param name="conversionParams">The conversion parameters.</param>
		/// <returns></returns>
		double ApplyConversion(double quantity, ConversionParameters conversionParams);
	}
}