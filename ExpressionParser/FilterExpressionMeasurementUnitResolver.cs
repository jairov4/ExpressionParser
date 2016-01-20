using System.Linq;

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
		private readonly IFilterExpressionExecutionContext repo;

		public FilterExpressionMeasurementUnitResolver(IFilterExpressionExecutionContext repo)
		{
			this.repo = repo;
		}

		public IReadOnlyDictionary<FilterExpressionNode, MeasurementUnitAlgebraicFactor> GetMeasurementUnit(FilterExpressionNode expr)
		{
			var visitor = new FilterExpressionVisitor(repo);
			expr.Accept(visitor);
			return visitor.UnitsByNode;
		}

		/// <summary>
		/// Internal visitor implementation
		/// </summary>
		private class FilterExpressionVisitor : IFilterExpressionVisitor
		{
			private IFilterExpressionExecutionContext Repository { get; }

			private Dictionary<FilterExpressionNode, MeasurementUnitAlgebraicFactor> units;

			public IReadOnlyDictionary<FilterExpressionNode, MeasurementUnitAlgebraicFactor> UnitsByNode => units;

			public FilterExpressionVisitor(IFilterExpressionExecutionContext repo)
			{
				this.Repository = repo;
				this.units = new Dictionary<FilterExpressionNode, MeasurementUnitAlgebraicFactor>();
			}
			
			public void Visit(FilterExpressionCastNode node, FilterExpressionVisitorAction action)
			{
                // Cast do not affect units
			}

            public void Visit(FilterExpressionLiteralNode node, FilterExpressionVisitorAction action)
            {
                if (action == FilterExpressionVisitorAction.Enter) return;
                var u = MeasurementUnitAlgebraicFactor.FromSingleUnit(node.MeasurementUnit);
				units.Add(node, u);
            }

			public void Visit(FilterExpressionMethodCallNode node, FilterExpressionVisitorAction action)
			{
                if (action == FilterExpressionVisitorAction.Enter) return;
				var argumentUnits = node.Arguments.Select(x => UnitsByNode[x]).ToList();
				var u = Repository.Methods[node.MethodName].ComputeResultingUnit(argumentUnits);
                units.Add(node, u);
            }

			public void Visit(FilterExpressionFieldReferenceNode node, FilterExpressionVisitorAction action)
			{
                if (action == FilterExpressionVisitorAction.Enter) return;
				var u = Repository.UnitsByField[node.FieldName];
				units.Add(node, u);
			}

			public void Visit(FilterExpressionUnaryNode node, FilterExpressionVisitorAction action)
			{
				if (action == FilterExpressionVisitorAction.Enter) return;
                units.Add(node, units[node]);
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
		                if (!Equals(units[node.LeftOperand], units[node.RightOperand]))
		                {
			                throw new InvalidOperationException();
		                }

						units.Add(node, units[node.LeftOperand]);
                        break;

                    case FilterExpressionBinaryOperator.Multiply:
	                {
		                var left = units[node.LeftOperand];
		                var right = units[node.RightOperand];
		                units.Add(node, left.Multiply(right));
	                }
		                break;

                    case FilterExpressionBinaryOperator.Divide:
	                {
		                var left = units[node.LeftOperand];
		                var right = units[node.RightOperand];
		                units.Add(node, left.Divide(right));
	                }
		                break;

                    case FilterExpressionBinaryOperator.Remainder:
	                {
		                var left = units[node.LeftOperand];
		                units.Add(node, left);
	                }
		                break;

	                case FilterExpressionBinaryOperator.Equals:
                    case FilterExpressionBinaryOperator.NotEquals:
                    case FilterExpressionBinaryOperator.LessThan:
                    case FilterExpressionBinaryOperator.GreatThan:
                    case FilterExpressionBinaryOperator.LessThanOrEquals:
                    case FilterExpressionBinaryOperator.GreatThanOrEquals:
                        // Comparison produces boolean adimensional magnitudes
                        units.Add(node, MeasurementUnitAlgebraicFactor.Dimensionless);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
		}
	}
}