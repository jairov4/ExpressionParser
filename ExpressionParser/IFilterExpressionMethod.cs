using System.Collections.Generic;

namespace DXAppProto2
{
	public interface IFilterExpressionMethod
	{
		MeasurementUnitAlgebraicFactor ComputeResultingUnit(IReadOnlyList<MeasurementUnitAlgebraicFactor> inputUnits);

		object ComputeResultingValue(IReadOnlyList<object> inputValues, IReadOnlyList<MeasurementUnitAlgebraicFactor> inputUnits);
	}
}