namespace DXAppProto2.FilterExpressions
{
	using System;
	using System.Collections.Generic;

	public enum FilterExpressionNodeType
	{
		FieldReference,
		Literal,
		MethodCall,
		Cast,
		Binary,
		Unary
	}

	public enum FilterExpressionVisitorAction
	{
		Enter,
		Exit
	}

	public abstract class FilterExpressionNode
	{
		public abstract FilterExpressionNodeType NodeType { get; }

		public abstract void Accept(IFilterExpressionVisitor visitor);
	}

	public sealed class FilterExpressionFieldReferenceNode : FilterExpressionNode
	{
		public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.FieldReference;

		public string FieldName { get; }

		public FilterExpressionFieldReferenceNode(string fieldName)
		{
			this.FieldName = fieldName;
		}

		public override void Accept(IFilterExpressionVisitor visitor)
		{
			visitor.Visit(this, FilterExpressionVisitorAction.Enter);
			visitor.Visit(this, FilterExpressionVisitorAction.Exit);
		}
	}

	public sealed class FilterExpressionLiteralNode : FilterExpressionNode
	{
		public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.Literal;

		public Type LiteralType { get; }

		public object Value { get; }

		public string MeasurementUnit { get; }

		public FilterExpressionLiteralNode(Type type, object value, string measurementUnit)
		{
			this.LiteralType = type;
			this.Value = value;
			this.MeasurementUnit = measurementUnit;
		}

		public override void Accept(IFilterExpressionVisitor visitor)
		{
			visitor.Visit(this, FilterExpressionVisitorAction.Enter);
			visitor.Visit(this, FilterExpressionVisitorAction.Exit);
		}
	}

	public sealed class FilterExpressionCastNode : FilterExpressionNode
	{
		public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.Cast;

		public Type TargetType { get; }

		public FilterExpressionNode Expression { get; }

		public FilterExpressionCastNode(Type targetType, FilterExpressionNode expression)
		{
			this.TargetType = targetType;
			this.Expression = expression;
		}

		public override void Accept(IFilterExpressionVisitor visitor)
		{
			visitor.Visit(this, FilterExpressionVisitorAction.Enter);
			this.Expression.Accept(visitor);
			visitor.Visit(this, FilterExpressionVisitorAction.Exit);
		}
	}

	public sealed class FilterExpressionMethodCallNode : FilterExpressionNode
	{
		public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.MethodCall;

		public string MethodName { get; }

		public IReadOnlyList<FilterExpressionNode> Arguments { get; }

		public FilterExpressionMethodCallNode(string methodName, IReadOnlyList<FilterExpressionNode> arguments)
		{
			this.MethodName = methodName;
			this.Arguments = arguments;
		}

		public override void Accept(IFilterExpressionVisitor visitor)
		{
			visitor.Visit(this, FilterExpressionVisitorAction.Enter);
			foreach (var item in this.Arguments)
			{
				item.Accept(visitor);
			}
			visitor.Visit(this, FilterExpressionVisitorAction.Exit);
		}
	}

	public enum FilterExpressionBinaryOperator
	{
		Or,
		And,
		Xor,
		Add,
		Subtract,
		Multiply,
		Divide,
		Remainder,
		Equals,
		NotEquals,
		LessThan,
		GreatThan,
		LessThanOrEquals,
		GreatThanOrEquals
	}

	public sealed class FilterExpressionBinaryNode : FilterExpressionNode
	{
		public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.Binary;

		public FilterExpressionBinaryOperator Operator { get; }

		public FilterExpressionNode LeftOperand { get; }

		public FilterExpressionNode RightOperand { get; }

		public FilterExpressionBinaryNode(FilterExpressionBinaryOperator oper, FilterExpressionNode a1,
			FilterExpressionNode a2)
		{
			this.Operator = oper;
			this.LeftOperand = a1;
			this.RightOperand = a2;
		}

		public override void Accept(IFilterExpressionVisitor visitor)
		{
			visitor.Visit(this, FilterExpressionVisitorAction.Enter);
			this.LeftOperand.Accept(visitor);
			this.RightOperand.Accept(visitor);
			visitor.Visit(this, FilterExpressionVisitorAction.Exit);
		}
	}

	public enum FilterExpressionUnaryOperator
	{
		Complement,
		Not
	}

	public sealed class FilterExpressionUnaryNode : FilterExpressionNode
	{
		public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.Unary;

		public FilterExpressionUnaryOperator Operator { get; }

		public FilterExpressionNode Operand { get; }

		public FilterExpressionUnaryNode(FilterExpressionUnaryOperator oper, FilterExpressionNode operand)
		{
			this.Operator = oper;
			this.Operand = operand;
		}

		public override void Accept(IFilterExpressionVisitor visitor)
		{
			visitor.Visit(this, FilterExpressionVisitorAction.Enter);
			this.Operand.Accept(visitor);
			visitor.Visit(this, FilterExpressionVisitorAction.Exit);
		}
	}

	/// <summary>
	/// Allows to process a filter expression AST.
	/// The visitor patter allows to process the AST without breaking of Liskov principle.
	/// </summary>
	public interface IFilterExpressionVisitor
	{
		void Visit(FilterExpressionBinaryNode node, FilterExpressionVisitorAction action);

		void Visit(FilterExpressionUnaryNode node, FilterExpressionVisitorAction action);

		void Visit(FilterExpressionCastNode node, FilterExpressionVisitorAction action);

		void Visit(FilterExpressionFieldReferenceNode node, FilterExpressionVisitorAction action);

		void Visit(FilterExpressionLiteralNode node, FilterExpressionVisitorAction action);

		void Visit(FilterExpressionMethodCallNode node, FilterExpressionVisitorAction action);
	}
}