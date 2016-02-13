namespace DXAppProto2
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using FilterExpressions;

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

		public IReadOnlyDictionary<FilterExpressionNode, AlgebraicFactor> GetMeasurementUnit(FilterExpressionNode expr)
		{
			var visitor = new FilterExpressionVisitor(this.repo);
			expr.Accept(visitor);
			return visitor.UnitsByNode;
		}

		/// <summary>
		/// Internal visitor implementation
		/// </summary>
		private class FilterExpressionVisitor : IFilterExpressionVisitor
		{
			private readonly Dictionary<FilterExpressionNode, AlgebraicFactor> units;
			private IFilterExpressionExecutionContext Repository { get; }

			public IReadOnlyDictionary<FilterExpressionNode, AlgebraicFactor> UnitsByNode => this.units;

			public FilterExpressionVisitor(IFilterExpressionExecutionContext repo)
			{
				this.Repository = repo;
				this.units = new Dictionary<FilterExpressionNode, AlgebraicFactor>();
			}

			public void Visit(FilterExpressionCastNode node, FilterExpressionVisitorAction action)
			{
				// Cast do not affect units
				if (action == FilterExpressionVisitorAction.Enter) return;
				this.units.Add(node, this.units[node]);
			}

			public void Visit(FilterExpressionLiteralNode node, FilterExpressionVisitorAction action)
			{
				if (action == FilterExpressionVisitorAction.Enter) return;
				var u = AlgebraicFactor.FromSymbol(node.MeasurementUnit);
				this.units.Add(node, u);
			}

			public void Visit(FilterExpressionMethodCallNode node, FilterExpressionVisitorAction action)
			{
				if (action == FilterExpressionVisitorAction.Enter) return;
				var argumentUnits = node.Arguments.Select(x => this.UnitsByNode[x]).ToList();
				var u = this.Repository.Methods[node.MethodName].ComputeResultingUnit(argumentUnits);
				this.units.Add(node, u);
			}

			public void Visit(FilterExpressionFieldReferenceNode node, FilterExpressionVisitorAction action)
			{
				if (action == FilterExpressionVisitorAction.Enter) return;
				var u = this.Repository.UnitsByField[node.FieldName];
				this.units.Add(node, u);
			}

			public void Visit(FilterExpressionUnaryNode node, FilterExpressionVisitorAction action)
			{
				if (action == FilterExpressionVisitorAction.Enter) return;
				this.units.Add(node, this.units[node.Operand]);
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
						if (!Equals(this.units[node.LeftOperand], this.units[node.RightOperand]))
						{
							throw new InvalidOperationException();
						}

						this.units.Add(node, this.units[node.LeftOperand]);
						break;

					case FilterExpressionBinaryOperator.Multiply:
					{
						var left = this.units[node.LeftOperand];
						var right = this.units[node.RightOperand];
						this.units.Add(node, left.Multiply(right));
					}
						break;

					case FilterExpressionBinaryOperator.Divide:
					{
						var left = this.units[node.LeftOperand];
						var right = this.units[node.RightOperand];
						this.units.Add(node, left.Divide(right));
					}
						break;

					case FilterExpressionBinaryOperator.Remainder:
					{
						var left = this.units[node.LeftOperand];
						this.units.Add(node, left);
					}
						break;

					case FilterExpressionBinaryOperator.Equals:
					case FilterExpressionBinaryOperator.NotEquals:
					case FilterExpressionBinaryOperator.LessThan:
					case FilterExpressionBinaryOperator.GreatThan:
					case FilterExpressionBinaryOperator.LessThanOrEquals:
					case FilterExpressionBinaryOperator.GreatThanOrEquals:
						// Comparison produces boolean adimensional magnitudes
						this.units.Add(node, AlgebraicFactor.Dimensionless);
						break;

					default:
						throw new NotSupportedException();
				}
			}
		}
	}
}