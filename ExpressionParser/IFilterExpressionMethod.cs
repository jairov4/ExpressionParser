namespace DXAppProto2
{
	using System.Collections.Generic;

	public interface IFilterExpressionMethod
	{
		AlgebraicFactor ComputeResultingUnit(IReadOnlyList<AlgebraicFactor> inputUnits);

		object ComputeResultingValue(IReadOnlyList<object> inputValues, IReadOnlyList<AlgebraicFactor> inputUnits);
	}
}