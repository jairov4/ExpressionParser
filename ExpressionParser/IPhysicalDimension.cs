using System.Collections.Generic;

namespace DXAppProto2
{
	public interface IPhysicalDimension
	{
		string Name { get; }

		string DefaultMeasurementUnit { get; }

		IReadOnlyCollection<string> MeasurementUnits { get; }

		IReadOnlyDictionary<string, ConversionParameters> Multiples { get; }
	}
}