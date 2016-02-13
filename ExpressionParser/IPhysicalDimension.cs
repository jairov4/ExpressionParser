namespace DXAppProto2
{
	using System.Collections.Generic;

	public interface IPhysicalDimension
	{
		string Name { get; }

		string DefaultMeasurementUnit { get; }

		IReadOnlyCollection<string> MeasurementUnits { get; }

		IReadOnlyDictionary<string, ConversionParameters> Multiples { get; }
	}
}