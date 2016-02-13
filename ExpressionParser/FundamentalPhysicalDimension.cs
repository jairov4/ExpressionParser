namespace DXAppProto2
{
	using System.Collections.Generic;

	public class FundamentalPhysicalDimension : IFundamentalPhysicalDimension
	{
		public Dictionary<string, ConversionParameters> Multiples { get; }

		public FundamentalPhysicalDimension(string name, string defaultMeasurementUnit)
		{
			this.Name = name;
			this.DefaultMeasurementUnit = defaultMeasurementUnit;
			this.Multiples = new Dictionary<string, ConversionParameters>
			{
				{ defaultMeasurementUnit, new ConversionParameters(1.0, 0.0) }
			};
		}

		public string Name { get; }

		public string DefaultMeasurementUnit { get; }

		public IReadOnlyCollection<string> MeasurementUnits => this.Multiples.Keys;

		IReadOnlyDictionary<string, ConversionParameters> IPhysicalDimension.Multiples => this.Multiples;
	}
}