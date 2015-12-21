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
		}

		public sealed class FilterExpressionFieldReferenceNode : FilterExpressionNode
		{
			public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.FieldReference;

			public string FieldName { get; }

			public FilterExpressionFieldReferenceNode(string fieldName)
			{
				FieldName = fieldName;
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
				LiteralType = type;
				Value = value;
				MeasurementUnit = measurementUnit;
			}
		}

		public sealed class FilterExpressionCastNode : FilterExpressionNode
		{
			public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.Cast;

			public Type TargetType { get; }

			public FilterExpressionNode Expression { get; }

			public FilterExpressionCastNode(Type targetType, FilterExpressionNode expression)
			{
				TargetType = targetType;
				Expression = expression;
			}
		}

		public sealed class FilterExpressionMethodCallNode : FilterExpressionNode
		{
			public override FilterExpressionNodeType NodeType => FilterExpressionNodeType.MethodCall;

			public string MethodName { get; }

			public IReadOnlyList<FilterExpressionNode> Arguments { get; }

			public FilterExpressionMethodCallNode(string methodName, IReadOnlyList<FilterExpressionNode> arguments)
			{
				MethodName = methodName;
				Arguments = arguments;
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
				Operator = oper;
				LeftOperand = a1;
				RightOperand = a2;
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
				Operator = oper;
				Operand = operand;
			}
		}

		public interface IMeasurementUnitValidator
		{
			bool IsMeasurementUnitValid(string measurementUnit);
		}

		public class MesaurementUnitValidator : IMeasurementUnitValidator
		{
			private HashSet<string> validUnits = new HashSet<string>()
			{
				"m",
				"cm",
				"mm",
				"km",
				"nm",
				"um",
				"pm",
				"fm"
			};

			public bool IsMeasurementUnitValid(string measurementUnit)
			{
				return validUnits.Contains(measurementUnit);
			}
		}

	}
}