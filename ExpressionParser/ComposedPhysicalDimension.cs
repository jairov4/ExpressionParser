namespace DXAppProto2
{
	using System.Collections.Generic;

	public struct ComposedPhysicalDimension : IComposedPhysicalDimension
	{
		public string Name { get; }

		public AlgebraicFactor DimensionalDefinition { get; }

		public AlgebraicFactor ReferenceFactor { get; }

		public ConversionParameters ConversionParameters { get; }

		public string DefaultMeasurementUnit { get; }

		public IReadOnlyCollection<string> MeasurementUnits => this.Multiples.Keys;

		public Dictionary<string, ConversionParameters> Multiples { get; }

		IReadOnlyDictionary<string, ConversionParameters> IPhysicalDimension.Multiples => this.Multiples;

		public ComposedPhysicalDimension(string name, AlgebraicFactor dimensionalDefinition, AlgebraicFactor referenceFactor,
			ConversionParameters conversionParameters, string defaultMeasurementUnit)
		{
			this.Name = name;
			this.DimensionalDefinition = dimensionalDefinition;
			this.ReferenceFactor = referenceFactor;
			this.ConversionParameters = conversionParameters;
			this.DefaultMeasurementUnit = defaultMeasurementUnit;
			this.Multiples = new Dictionary<string, ConversionParameters>
			{
				{ defaultMeasurementUnit, new ConversionParameters(1.0, 0.0) }
			};
		}
	}
}