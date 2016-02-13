namespace DXAppProto2
{
	using System.Collections.Generic;
	
	/// <summary>
	/// Represents a physical dimension
	/// </summary>
	public interface IPhysicalDimension
	{
		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value> The name. </value>
		string Name { get; }

		/// <summary>
		/// Gets the default measurement unit for this physical dimension.
		/// </summary>
		/// <value> The default measurement unit. </value>
		string DefaultMeasurementUnit { get; }

		/// <summary>
		/// Gets the measurement units.
		/// </summary>
		/// <value> The measurement units. </value>
		IReadOnlyCollection<string> MeasurementUnits { get; }

		/// <summary>
		/// Gets the multiples conversion parameters grouped by measurement unit.
		/// </summary>
		/// <value> The multiples. </value>
		IReadOnlyDictionary<string, ConversionParameters> Multiples { get; }
	}
}