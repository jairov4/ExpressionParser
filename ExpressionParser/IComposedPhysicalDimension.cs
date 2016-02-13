namespace DXAppProto2
{
	public interface IComposedPhysicalDimension : IPhysicalDimension
	{
		ConversionParameters ConversionParameters { get; }

		AlgebraicFactor DimensionalDefinition { get; }

		AlgebraicFactor ReferenceFactor { get; }
	}
}