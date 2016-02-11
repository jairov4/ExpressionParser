using System.Collections.Generic;

namespace DXAppProto2
{
	public struct ComposedPhysicalDimension : IComposedPhysicalDimension
	{
		public string Name { get; }

		public AlgebraicFactor DimensionalDefinition { get; }

		/// <summary>
		/// Gets the reference factor in fundamental measurement units.
		/// </summary>
		/// <value>
		/// The reference factor.
		/// </value>
		public AlgebraicFactor ReferenceFactor { get; }

		public ConversionParameters ConversionParameters { get; }

		public string DefaultMeasurementUnit { get; }
		
		public IReadOnlyCollection<string> MeasurementUnits => Multiples.Keys;

		public Dictionary<string, ConversionParameters> Multiples { get; }

		IReadOnlyDictionary<string, ConversionParameters> IPhysicalDimension.Multiples => Multiples;

		public ComposedPhysicalDimension(string name, AlgebraicFactor dimensionalDefinition, AlgebraicFactor referenceFactor, ConversionParameters conversionParameters, string defaultMeasurementUnit)
		{
			this.Name = name;
			this.DimensionalDefinition = dimensionalDefinition;
			this.ReferenceFactor = referenceFactor;
			this.ConversionParameters = conversionParameters;
			this.DefaultMeasurementUnit = defaultMeasurementUnit;
			this.Multiples = new Dictionary<string, ConversionParameters>
			{
				{defaultMeasurementUnit, new ConversionParameters(1.0, 0.0)}
			};
		}
	}
}