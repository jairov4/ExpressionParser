namespace DXAppProto2
{
	using System.Collections.Generic;

	/// <summary>
	/// Represent a method that an expression can invoke
	/// </summary>
	public interface IFilterExpressionMethod
	{
		/// <summary>
		/// Computes the resulting measurement unit of invocation.
		/// </summary>
		/// <param name="inputUnits">The measurement units for input arguments.</param>
		/// <returns>The resulting units algebraic factor</returns>
		AlgebraicFactor ComputeResultingUnit(IReadOnlyList<AlgebraicFactor> inputUnits);

		/// <summary>
		/// Computes the resulting value.
		/// </summary>
		/// <param name="inputValues">The input values.</param>
		/// <param name="inputUnits">The input measurement units.</param>
		/// <returns>The resulting value</returns>
		object ComputeResultingValue(IReadOnlyList<object> inputValues, IReadOnlyList<AlgebraicFactor> inputUnits);
	}
}