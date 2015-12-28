namespace DXAppProto2
{
	using System;
	using FilterExpressions;

	public interface IMeasurementUnit
	{
		string Abbreviation { get; }
	}

	public interface IMeasurementUnitRepository
	{
		IMeasurementUnit ResolveAbbreviation(string abbrev);
	}

	public class MeasurementUnitResolver
	{
		private readonly IMeasurementUnitRepository repo;

		public MeasurementUnitResolver(IMeasurementUnitRepository repo)
		{
			this.repo = repo;
		}

		public IMeasurementUnit GetMeasurementUnit(FilterExpressionNode expr)
		{
			var visitor = new FilterExpressionVisitor(repo);
			expr.Accept(visitor);
			return visitor.Result;
		}

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
				throw new NotImplementedException();
			}

			public void Visit(FilterExpressionLiteralNode node, FilterExpressionVisitorAction action)
			{
				throw new NotImplementedException();
			}

			public void Visit(FilterExpressionMethodCallNode node, FilterExpressionVisitorAction action)
			{
				throw new NotImplementedException();
			}

			public void Visit(FilterExpressionFieldReferenceNode node, FilterExpressionVisitorAction action)
			{
				throw new NotImplementedException();
			}

			public void Visit(FilterExpressionUnaryNode node, FilterExpressionVisitorAction action)
			{
				throw new NotImplementedException();
			}

			public void Visit(FilterExpressionBinaryNode node, FilterExpressionVisitorAction action)
			{
				throw new NotImplementedException();
			}
		}
	}
}