﻿namespace DXAppProto2
{
	using System.Collections.Generic;

	/// <summary>
	/// Provides means to get measurement units from other representations
	/// </summary>
	public interface IFilterExpressionExecutionContext
	{
		IReadOnlyDictionary<string, IExpressionMethod> Methods { get; }

		IReadOnlyDictionary<string, MeasurementUnitAlgebraicFactor> UnitsByField { get; }
	}
}