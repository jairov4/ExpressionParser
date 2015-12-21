using System.Collections.Generic;

namespace DXAppProto2.FilterExpressions
{
	public class MesaurementUnitValidator : IMeasurementUnitValidator
	{
		private HashSet<string> validUnits = new HashSet<string>()
		{
			"m",
			"cm",
			"mm",
			"km",
			"nm",
			"um",
			"pm",
			"fm"
		};

		public bool IsMeasurementUnitValid(string measurementUnit)
		{
			return validUnits.Contains(measurementUnit);
		}
	}
}