using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXAppProto2
{
	namespace FilterExpressions
	{
		public enum FilterExpressionNodeType
		{
			FieldReference,
			Literal,
			MethodCall,
			Cast,
			Binary,
			Unary
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
                visitor.Visit(this);
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
                visitor.Visit(this);
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
                visitor.Visit(this);
                this.Expression.Accept(visitor);
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
                visitor.Visit(this);
                foreach (var item in this.Arguments)
                {
                    item.Accept(visitor);
                }
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
                visitor.Visit(this);
                LeftOperand.Accept(visitor);
                RightOperand.Accept(visitor);
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
                visitor.Visit(this);
                this.Operand.Accept(visitor);
            }
        }

		public interface IMeasurementUnitValidator
		{
			bool IsMeasurementUnitValid(string measurementUnit);
		}

        public interface IFilterExpressionVisitor
        {
            void Visit(FilterExpressionBinaryNode node);

            void Visit(FilterExpressionUnaryNode node);

            void Visit(FilterExpressionCastNode node);

            void Visit(FilterExpressionFieldReferenceNode node);

            void Visit(FilterExpressionLiteralNode node);

            void Visit(FilterExpressionMethodCallNode node);
        }
	}
}