﻿namespace ExpressionParser.Test
{
	using System.IO;
	using System.Text;
	using DXAppProto2.FilterExpressions;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class ParserSimpleTests
	{
		[TestMethod]
		public void ParseSimpleExpression_ExpectAst()
		{
			var expression = "a + b";
			var parser = ParseString(expression);

			Assert.IsNotNull(parser.Root);
			Assert.IsTrue(parser.Root is FilterExpressionBinaryNode);
			Assert.AreEqual(FilterExpressionBinaryOperator.Add, ((FilterExpressionBinaryNode)parser.Root).Operator);
		}

		[TestMethod]
		public void WithParentesis_ParseSimpleExpression_ExpectAst()
		{
			var expression = "(a + b) * (c + d)";
			var parser = ParseString(expression);

			Assert.IsNotNull(parser.Root);
			Assert.IsTrue(parser.Root is FilterExpressionBinaryNode);
			Assert.AreEqual(FilterExpressionBinaryOperator.Multiply, ((FilterExpressionBinaryNode)parser.Root).Operator);
		}

		[TestMethod]
		public void WithMethod_ParseSimpleExpression_ExpectAst()
		{
			var expression = "(a + b) * f(c + d, 12 + 23)";
			var parser = ParseString(expression);

			Assert.IsNotNull(parser.Root);
			Assert.IsTrue(parser.Root is FilterExpressionBinaryNode);
			Assert.AreEqual(FilterExpressionBinaryOperator.Multiply, ((FilterExpressionBinaryNode)parser.Root).Operator);
		}

		[TestMethod]
		public void WithUnis_ParseSimpleExpression_ExpectAst()
		{
			var expression = "2 cm + 14 mm";
			var parser = ParseString(expression);

			Assert.IsNotNull(parser.Root);
			Assert.IsTrue(parser.Root is FilterExpressionBinaryNode);
			Assert.AreEqual(FilterExpressionBinaryOperator.Add, ((FilterExpressionBinaryNode)parser.Root).Operator);
		}

		[TestMethod]
		public void WithCast_ParseSimpleExpression_ExpectAst()
		{
			var expression = "(double)(2 cm + 14 mm)";
			var parser = ParseString(expression);

			Assert.IsNotNull(parser.Root);
			Assert.IsTrue(parser.Root is FilterExpressionCastNode);
			Assert.AreEqual(typeof (double), ((FilterExpressionCastNode)parser.Root).TargetType);
		}

		[TestMethod]
		public void WithBoolean_ParseSimpleExpression_ExpectAst()
		{
			var expression = "b = c Or d <> e And p Xor j And b > c And b < c And c >= b And b <= d";
			var parser = ParseString(expression);

			Assert.IsNotNull(parser.Root);
		}

		private static Parser ParseString(string expression)
		{
			var buffer = Encoding.Default.GetBytes(expression);
			var stream = new MemoryStream(buffer);
			var scanner = new Scanner(stream);
			var parser = new Parser(scanner);
			parser.Parse();
			return parser;
		}
	}
}