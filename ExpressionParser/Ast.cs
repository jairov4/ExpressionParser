namespace DXAppProto2.FilterExpressions
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Type of filter expression node
	/// </summary>
	public enum FilterExpressionNodeType
	{
		FieldReference,
		Literal,
		MethodCall,
		Cast,
		Binary,
		Unary
	}

	/// <summary>
	/// Filter expression visitor action
	/// </summary>
	public enum FilterExpressionVisitorAction
	{
		Enter,
		Exit
	}

	/// <summary>
	/// Abstract filter expression node
	/// </summary>
	public abstract class FilterExpressionNode
	{
		/// <summary>
		/// Gets the type of the node.
		/// </summary>
		/// <value> The type of the node. </value>
		public abstract FilterExpressionNodeType NodeType { get; }

		/// <summary>
		/// Accepts the specified visitor.
		/// </summary>
		/// <param name="visitor">The visitor.</param>
		public abstract void Accept(IFilterExpressionVisitor visitor);
	}

	/// <summary>
	/// Expression node for reference to fields
	/// </summary>
	public sealed class FilterExpressionFieldReferenceNode : FilterExpressionNode
	{
		public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.FieldReference;

		/// <summary>
		/// Gets the name of the referenced field.
		/// </summary>
		/// <value> The name of the field. </value>
		public string FieldName { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterExpressionFieldReferenceNode" /> class.
		/// </summary>
		/// <param name="fieldName">Name of the field.</param>
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

	/// <summary>
	/// Expression node for literal values
	/// </summary>
	public sealed class FilterExpressionLiteralNode : FilterExpressionNode
	{
		public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.Literal;

		/// <summary>
		/// Gets the type of the literal.
		/// </summary>
		/// <value> The type of the literal. </value>
		public Type LiteralType { get; }

		/// <summary>
		/// Gets the literal value.
		/// </summary>
		/// <value> The value. </value>
		public object Value { get; }

		/// <summary>
		/// Gets the measurement unit if is expressed.
		/// </summary>
		/// <value> The measurement unit. </value>
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

	/// <summary>
	/// Expression node for type casts
	/// </summary>
	public sealed class FilterExpressionCastNode : FilterExpressionNode
	{
		public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.Cast;

		/// <summary>
		/// Gets the type of the target.
		/// </summary>
		/// <value> The type of the target. </value>
		public Type TargetType { get; }

		/// <summary>
		/// Gets the inner expression which valuation is cast.
		/// </summary>
		/// <value> The expression. </value>
		public FilterExpressionNode Expression { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterExpressionCastNode" /> class.
		/// </summary>
		/// <param name="targetType">Type of the target.</param>
		/// <param name="expression">The expression.</param>
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

	/// <summary>
	/// Expression node for method invocations
	/// </summary>
	public sealed class FilterExpressionMethodCallNode : FilterExpressionNode
	{
		public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.MethodCall;

		/// <summary>
		/// Gets the name of the invoked method.
		/// </summary>
		/// <value> The name of the method. </value>
		public string MethodName { get; }

		/// <summary>
		/// Gets the expressions for each argument of the invocation.
		/// </summary>
		/// <value> The arguments. </value>
		public IReadOnlyList<FilterExpressionNode> Arguments { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterExpressionMethodCallNode" /> class.
		/// </summary>
		/// <param name="methodName">Name of the invoked method.</param>
		/// <param name="arguments">The arguments.</param>
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

	/// <summary>
	/// Binary operators for <see cref="FilterExpressionBinaryNode" />
	/// </summary>
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

	/// <summary>
	/// Expression node for binary sub-expressions
	/// </summary>
	public sealed class FilterExpressionBinaryNode : FilterExpressionNode
	{
		public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.Binary;

		/// <summary>
		/// Gets the binary operator.
		/// </summary>
		/// <value> The operator. </value>
		public FilterExpressionBinaryOperator Operator { get; }

		/// <summary>
		/// Gets the left operand.
		/// </summary>
		/// <value> The left operand. </value>
		public FilterExpressionNode LeftOperand { get; }

		/// <summary>
		/// Gets the right operand.
		/// </summary>
		/// <value> The right operand. </value>
		public FilterExpressionNode RightOperand { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterExpressionBinaryNode" /> class.
		/// </summary>
		/// <param name="oper">The operator.</param>
		/// <param name="a1">The left operand.</param>
		/// <param name="a2">The right operand.</param>
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

	/// <summary>
	/// Unary operators for <see cref="FilterExpressionUnaryNode"/>
	/// </summary>
	public enum FilterExpressionUnaryOperator
	{
		Complement,
		Not
	}

	/// <summary>
	/// Filter expression for unary sub-expressions
	/// </summary>
	public sealed class FilterExpressionUnaryNode : FilterExpressionNode
	{
		public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.Unary;

		/// <summary>
		/// Gets the operator.
		/// </summary>
		/// <value> The unary operator. </value>
		public FilterExpressionUnaryOperator Operator { get; }

		/// <summary>
		/// Gets the operand.
		/// </summary>
		/// <value> The operand. </value>
		public FilterExpressionNode Operand { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FilterExpressionUnaryNode" /> class.
		/// </summary>
		/// <param name="oper">The unary operator.</param>
		/// <param name="operand">The operand.</param>
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
		/// <summary>
		/// Visits the specified node.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="action">The action.</param>
		void Visit(FilterExpressionBinaryNode node, FilterExpressionVisitorAction action);

		/// <summary>
		/// Visits the specified node.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="action">The action.</param>
		void Visit(FilterExpressionUnaryNode node, FilterExpressionVisitorAction action);

		/// <summary>
		/// Visits the specified node.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="action">The action.</param>
		void Visit(FilterExpressionCastNode node, FilterExpressionVisitorAction action);

		/// <summary>
		/// Visits the specified node.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="action">The action.</param>
		void Visit(FilterExpressionFieldReferenceNode node, FilterExpressionVisitorAction action);

		/// <summary>
		/// Visits the specified node.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="action">The action.</param>
		void Visit(FilterExpressionLiteralNode node, FilterExpressionVisitorAction action);

		/// <summary>
		/// Visits the specified node.
		/// </summary>
		/// <param name="node">The node.</param>
		/// <param name="action">The action.</param>
		void Visit(FilterExpressionMethodCallNode node, FilterExpressionVisitorAction action);
	}
}