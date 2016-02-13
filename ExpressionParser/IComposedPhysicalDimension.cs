namespace DXAppProto2
{
	/// <summary>
	/// Represents a composed physical dimension
	/// </summary>
	public interface IComposedPhysicalDimension : IPhysicalDimension
	{
		/// <summary>
		/// Gets the conversion parameters applied to a quantity expressed with measurement units in the default measurement units of this dimension
		/// to get expressed in the measurement units of <see cref="ReferenceFactor"/>.
		/// </summary>
		/// <value>
		/// The conversion parameters.
		/// </value>
		ConversionParameters ConversionParameters { get; }

		/// <summary>
		/// Gets the dimensional definition. It is the algebraic factor expressed in physical dimensions.
		/// </summary>
		/// <value>
		/// The dimensional definition.
		/// </value>
		AlgebraicFactor DimensionalDefinition { get; }

		/// <summary>
		/// Gets the reference factor expressed in fundamental measurement units.
		/// </summary>
		/// <value>
		/// The reference factor.
		/// </value>
		AlgebraicFactor ReferenceFactor { get; }
	}
}