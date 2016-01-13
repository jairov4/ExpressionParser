namespace DXAppProto2
{
	/// <summary>
	/// Provides means to get measurement units from other representations
	/// </summary>
	public interface IMeasurementUnitRepository
	{
		IMeasurementUnit ResolveAbbreviation(string abbrev);
	}
}