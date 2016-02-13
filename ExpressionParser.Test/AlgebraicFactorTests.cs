using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressionParser.Test
{
	using System.Linq;
	using DXAppProto2;

	[TestClass]
	public class AlgebraicFactorTests
	{
		[TestMethod]
		public void WithTwoSimpleAlgebraicFactors_TestMultiply_ExpectedSquared()
		{
			var result = AlgebraicFactor.FromSymbol("s").Multiply(AlgebraicFactor.FromSymbol("s"));

			Assert.IsTrue(!result.Denominator.Any());
			Assert.IsTrue(result.Numerator.Count == 1);
			Assert.AreEqual("s", result.Numerator.First().Key);
			Assert.AreEqual(2, result.Numerator.First().Value);
		}

		[TestMethod]
		public void WithTwoSimpleAlgebraicFactors_TestDivide_ExpectedRightValue()
		{
			var result = AlgebraicFactor.FromSymbol("s").Divide(AlgebraicFactor.FromSymbol("km"));
			
			Assert.AreEqual("s", result.Numerator.First().Key);
			Assert.AreEqual("km", result.Denominator.First().Key);
		}

		[TestMethod]
		public void WithTwoFractionalAlgebraicFactors_TestDivide_ExpectedRightValue()
		{
			var factor1 = AlgebraicFactor.FromSymbol("m").Divide(AlgebraicFactor.FromSymbol("s"));
			var factor2 = AlgebraicFactor.FromSymbol("kg").Divide(AlgebraicFactor.FromSymbol("V"));
			var result = factor1.Divide(factor2);

			Assert.AreEqual(2, result.Numerator.Count);
			Assert.AreEqual(2, result.Denominator.Count);
			Assert.IsTrue(result.Numerator.ContainsKey("m"));
			Assert.IsTrue(result.Numerator.ContainsKey("V"));
			Assert.IsTrue(result.Denominator.ContainsKey("kg"));
			Assert.IsTrue(result.Denominator.ContainsKey("s"));
		}

		[TestMethod]
		public void WithTwoRepeatedFractionalAlgebraicFactors_TestDivide_ExpectedRightValue()
		{
			// m/s^2
			var factor1 = AlgebraicFactor.FromSymbol("m").Divide(AlgebraicFactor.FromSymbol("s").Multiply(AlgebraicFactor.FromSymbol("s")));
			// kg*s/V
			var factor2 = AlgebraicFactor.FromSymbol("kg").Multiply(AlgebraicFactor.FromSymbol("s")).Divide(AlgebraicFactor.FromSymbol("V"));
			var result = factor1.Divide(factor2);

			Assert.AreEqual(2, result.Numerator.Count);
			Assert.AreEqual(2, result.Denominator.Count);
			Assert.IsTrue(result.Numerator.ContainsKey("m"));
			Assert.IsTrue(result.Numerator.ContainsKey("V"));
			Assert.IsTrue(result.Denominator.ContainsKey("kg"));
			Assert.IsTrue(result.Denominator.ContainsKey("s"));
			Assert.AreEqual(1, result.Numerator["m"]);
			Assert.AreEqual(1, result.Numerator["V"]);
			Assert.AreEqual(1, result.Denominator["kg"]);
			Assert.AreEqual(3, result.Denominator["s"]);
		}

		[TestMethod]
		public void WithTwoFractionalAlgebraicFactors_TestEquals_ExpectedTrue()
		{
			var factor1 = AlgebraicFactor.FromSymbol("s").Divide(AlgebraicFactor.FromSymbol("km"));
			var factor2 = AlgebraicFactor.FromSymbol("s").Divide(AlgebraicFactor.FromSymbol("km"));

			Assert.IsTrue(factor1.Equals(factor2));
		}

		[TestMethod]
		public void WithTwoFractionalAlgebraicFactors_TestEquals_ExpectedFalse()
		{
			var factor1 = AlgebraicFactor.FromSymbol("s").Divide(AlgebraicFactor.FromSymbol("km"));
			var factor2 = AlgebraicFactor.FromSymbol("km").Divide(AlgebraicFactor.FromSymbol("s"));

			Assert.IsFalse(factor1.Equals(factor2));
		}

		[TestMethod]
		public void WithTwoAlgebraicFactors_TestEquals_ExpectedTrue()
		{
			var factor1 = AlgebraicFactor.FromSymbol("s").Multiply(AlgebraicFactor.FromSymbol("km"));
			var factor2 = AlgebraicFactor.FromSymbol("km").Multiply(AlgebraicFactor.FromSymbol("s"));

			Assert.IsTrue(factor1.Equals(factor2));
		}

		[TestMethod]
		public void WithTwoAlgebraicFactors_TestEquals_ExpectedFalse()
		{
			var factor1 = AlgebraicFactor.FromSymbol("s").Multiply(AlgebraicFactor.FromSymbol("km"));
			var factor2 = AlgebraicFactor.FromSymbol("km").Multiply(AlgebraicFactor.FromSymbol("h"));

			Assert.IsFalse(factor1.Equals(factor2));
		}

		[TestMethod]
		public void WithAlgebraicFactor_TestInvert_ExpectedRightValue()
		{
			var factor = AlgebraicFactor.FromSymbol("km");
			var result = factor.Inverse();

			Assert.IsTrue(!result.Numerator.Any());
			Assert.IsTrue(result.Denominator.Count == 1);
			Assert.AreEqual("km", result.Denominator.First().Key);
			Assert.AreEqual(1, result.Denominator.First().Value);
		}

		[TestMethod]
		public void WithSquareAlgebraicFactor_TestInvert_ExpectedRightValue()
		{
			var factor = AlgebraicFactor.FromSymbol("s").Multiply(AlgebraicFactor.FromSymbol("s"));
			var result = factor.Inverse();

			Assert.IsTrue(!result.Numerator.Any());
			Assert.IsTrue(result.Denominator.Count == 1);
			Assert.AreEqual("s", result.Denominator.First().Key);
			Assert.AreEqual(2, result.Denominator.First().Value);
		}
	}
}
