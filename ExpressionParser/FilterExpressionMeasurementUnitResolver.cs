namespace DXAppProto2
{
    using System;
    using FilterExpressions;
    using System.Collections.Generic;

	/// <summary>
	/// Resolves the measurement unit of the whole expression performing dimensional analysis
	/// </summary>
	public class FilterExpressionMeasurementUnitResolver
	{
		private readonly IMeasurementUnitRepository repo;

		public FilterExpressionMeasurementUnitResolver(IMeasurementUnitRepository repo)
		{
			this.repo = repo;
		}

		public IMeasurementUnit GetMeasurementUnit(FilterExpressionNode expr)
		{
			var visitor = new FilterExpressionVisitor(repo);
			expr.Accept(visitor);
			return visitor.Result;
		}

		/// <summary>
		/// Internal visitor implementation
		/// </summary>
		private class FilterExpressionVisitor : IFilterExpressionVisitor
		{
			private IMeasurementUnitRepository Repository { get; }

			public IMeasurementUnit Result { get; private set; }

			public FilterExpressionVisitor(IMeasurementUnitRepository repo)
			{
				this.Repository = repo;
			}
			
			public void Visit(FilterExpressionCastNode node, FilterExpressionVisitorAction action)
			{
                // Cast do not affect units
			}

            public void Visit(FilterExpressionLiteralNode node, FilterExpressionVisitorAction action)
            {
                if (action == FilterExpressionVisitorAction.Enter) return;
                Result = Repository.ResolveAbbreviation(node.MeasurementUnit);
            }

			public void Visit(FilterExpressionMethodCallNode node, FilterExpressionVisitorAction action)
			{
                if (action == FilterExpressionVisitorAction.Enter) return;
                // Process due to method name and its arguments the resulting unit
                Result = null;
            }

			public void Visit(FilterExpressionFieldReferenceNode node, FilterExpressionVisitorAction action)
			{
                if (action == FilterExpressionVisitorAction.Enter) return;
                // Process due to field provider the measurement unit
                Result = null;
            }

			public void Visit(FilterExpressionUnaryNode node, FilterExpressionVisitorAction action)
			{
                // Unary expressions do not affect measurement unit
            }

			public void Visit(FilterExpressionBinaryNode node, FilterExpressionVisitorAction action)
			{
                if (action == FilterExpressionVisitorAction.Enter) return;
                switch (node.Operator)
                {
                    case FilterExpressionBinaryOperator.Or:
                    case FilterExpressionBinaryOperator.And:
                    case FilterExpressionBinaryOperator.Xor:
                    case FilterExpressionBinaryOperator.Add:
                    case FilterExpressionBinaryOperator.Subtract:
                        // These operators do not affect unit measurement
                        break;

                    case FilterExpressionBinaryOperator.Multiply:
                    case FilterExpressionBinaryOperator.Divide:
                    case FilterExpressionBinaryOperator.Remainder:
                        // TODO: Algebraic handling of measurement unit
                        break;

                    case FilterExpressionBinaryOperator.Equals:
                    case FilterExpressionBinaryOperator.NotEquals:
                    case FilterExpressionBinaryOperator.LessThan:
                    case FilterExpressionBinaryOperator.GreatThan:
                    case FilterExpressionBinaryOperator.LessThanOrEquals:
                    case FilterExpressionBinaryOperator.GreatThanOrEquals:
                        // Comparison produces boolean adimensional magnitudes
                        Result = null;
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
		}
	}
}