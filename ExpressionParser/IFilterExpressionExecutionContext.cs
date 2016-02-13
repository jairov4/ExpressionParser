namespace DXAppProto2
{
	using System.Collections.Generic;

	/// <summary>
	/// Provides means to get measurement units from other representations
	/// </summary>
	public interface IFilterExpressionExecutionContext
	{
		/// <summary>
		/// Gets the methods that an expression can use.
		/// </summary>
		/// <value>The methods.</value>
		IReadOnlyDictionary<string, IFilterExpressionMethod> Methods { get; }

		/// <summary>
		/// Gets the measurement units grouped by expression field.
		/// </summary>
		/// <value>The units by field.</value>
		IReadOnlyDictionary<string, AlgebraicFactor> UnitsByField { get; }
	}
}