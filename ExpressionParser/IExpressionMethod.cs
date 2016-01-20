using System.Collections.Generic;

namespace DXAppProto2
{
	public interface IExpressionMethod
	{
		MeasurementUnitAlgebraicFactor ComputeResultingUnit(IReadOnlyList<MeasurementUnitAlgebraicFactor> inputUnits);

		object ComputeResultingValue(IReadOnlyList<object> inputValues, IReadOnlyList<MeasurementUnitAlgebraicFactor> inputUnits);
	}
}