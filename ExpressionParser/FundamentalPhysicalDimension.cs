using System.Collections.Generic;

namespace DXAppProto2
{
	public class FundamentalPhysicalDimension : IFundamentalPhysicalDimension
	{
		public string Name { get; }

		public string DefaultMeasurementUnit { get; }
		
		public IReadOnlyCollection<string> MeasurementUnits => Multiples.Keys;

		public Dictionary<string, ConversionParameters> Multiples { get; }

		IReadOnlyDictionary<string, ConversionParameters> IPhysicalDimension.Multiples => Multiples;

		public FundamentalPhysicalDimension(string name, string defaultMeasurementUnit)
		{
			this.Name = name;
			this.DefaultMeasurementUnit = defaultMeasurementUnit;
			this.Multiples = new Dictionary<string, ConversionParameters>
			{
				{defaultMeasurementUnit, new ConversionParameters(1.0, 0.0)}
			};
		}
	}
}