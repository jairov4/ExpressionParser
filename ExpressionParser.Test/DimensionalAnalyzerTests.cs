﻿namespace ExpressionParser.Test
{
	using System.Linq;
	using DXAppProto2;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	[TestClass]
	public class DimensionalAnalyzerTests
	{
		[TestMethod]
		public void WithEmptyAnalyzer_AddFundamentalDimension_ExpectedAdded()
		{
			var dim = new DimensionalAnalyzer();
			dim.AddFundamentalDimension("length", "m");

			Assert.AreEqual(1, dim.FundamentalPhysicalDimensions.Count);
			Assert.AreEqual("length", dim.GetFundamentalPhysicalDimension("length").Name);
			Assert.AreEqual("m", dim.GetFundamentalPhysicalDimension("length").DefaultMeasurementUnit);
			Assert.AreEqual(1, dim.GetFundamentalPhysicalDimension("length").MeasurementUnits.Count);
			Assert.AreEqual("m", dim.GetFundamentalPhysicalDimension("length").MeasurementUnits.First());
			Assert.AreEqual(1, dim.GetFundamentalPhysicalDimension("length").Multiples.Count);
			Assert.AreEqual(1.0, dim.GetFundamentalPhysicalDimension("length").Multiples.First().Value.Factor);
			Assert.AreEqual(0.0, dim.GetFundamentalPhysicalDimension("length").Multiples.First().Value.Offset);
		}

		[TestMethod]
		public void WithFundamentalDimension_AddMultiplierUnit_ExpectedAdded()
		{
			var dim = new DimensionalAnalyzer();
			dim.AddFundamentalDimension("length", "m");
			dim.AddMultiplierMeasurementUnit("km", "m", new ConversionParameters(1000, 0));

			Assert.AreEqual(1, dim.FundamentalPhysicalDimensions.Count);
			Assert.AreEqual("length", dim.GetFundamentalPhysicalDimension("length").Name);
			Assert.AreEqual("m", dim.GetFundamentalPhysicalDimension("length").DefaultMeasurementUnit);
			Assert.AreEqual(2, dim.GetFundamentalPhysicalDimension("length").MeasurementUnits.Count);
			Assert.AreEqual("m", dim.GetFundamentalPhysicalDimension("length").MeasurementUnits.First());
			Assert.AreEqual(2, dim.GetFundamentalPhysicalDimension("length").Multiples.Count);
			Assert.AreEqual(1000.0, dim.GetFundamentalPhysicalDimension("length").Multiples["km"].Factor);
			Assert.AreEqual(0.0, dim.GetFundamentalPhysicalDimension("length").Multiples["km"].Offset);
		}

		[TestMethod]
		public void WithFundamentalDimensions_AddComposedDimension_ExpectedAdded()
		{
			var dim = new DimensionalAnalyzer();
			dim.AddFundamentalDimension("force", "N");
			dim.AddFundamentalDimension("area", "m2");

			var psi = AlgebraicFactor.FromSymbol("N").Divide(AlgebraicFactor.FromSymbol("m2"));
			dim.AddComposedDimension("pressure", "psi", psi, new ConversionParameters(6894.76, 0));

			Assert.AreEqual("psi", dim.GetComposedPhysicalDimension("pressure").DefaultMeasurementUnit);
			Assert.AreEqual(2, dim.FundamentalPhysicalDimensions.Count);
			Assert.AreEqual(1, dim.ComposedPhysicalDimensions.Count);
			Assert.AreEqual(6894.76, dim.GetComposedPhysicalDimension("pressure").ConversionParameters.Factor);
			Assert.AreEqual("force", dim.GetComposedPhysicalDimension("pressure").DimensionalDefinition.Numerator.First().Key);
			Assert.AreEqual("area", dim.GetComposedPhysicalDimension("pressure").DimensionalDefinition.Denominator.First().Key);
			Assert.AreEqual("N", dim.GetComposedPhysicalDimension("pressure").ReferenceFactor.Numerator.First().Key);
			Assert.AreEqual("m2", dim.GetComposedPhysicalDimension("pressure").ReferenceFactor.Denominator.First().Key);
			Assert.AreEqual("pressure", dim.GetPhysicalDimensionForMeasurementUnit("psi").Name);
		}

		[TestMethod]
		public void WithComposedDimensions_AddMultiplierUnit_ExpectedAdded()
		{
			var dim = new DimensionalAnalyzer();
			dim.AddFundamentalDimension("force", "N");
			dim.AddFundamentalDimension("area", "m2");

			var psi = AlgebraicFactor.FromSymbol("N").Divide(AlgebraicFactor.FromSymbol("m2"));
			dim.AddComposedDimension("pressure", "psi", psi, new ConversionParameters(6894.76, 0));

			dim.AddMultiplierMeasurementUnit("atm", "psi", new ConversionParameters(14.6959, 0));

			Assert.AreEqual(2, dim.FundamentalPhysicalDimensions.Count);
			Assert.AreEqual(1, dim.ComposedPhysicalDimensions.Count);
			Assert.AreEqual(2, dim.GetComposedPhysicalDimension("pressure").Multiples.Count);
			Assert.AreEqual(2, dim.GetComposedPhysicalDimension("pressure").MeasurementUnits.Count);
			Assert.AreEqual("pressure", dim.GetPhysicalDimensionForMeasurementUnit("atm").Name);
		}

		[TestMethod]
		public void WithTwoMultiples_TestDimensionalEquivalence_ExpectedTrue()
		{
			var dim = new DimensionalAnalyzer();
			dim.AddFundamentalDimension("force", "N");
			dim.AddFundamentalDimension("area", "m2");

			var psi = AlgebraicFactor.FromSymbol("N").Divide(AlgebraicFactor.FromSymbol("m2"));
			dim.AddComposedDimension("pressure", "psi", psi, new ConversionParameters(6894.76, 0));

			dim.AddMultiplierMeasurementUnit("atm", "psi", new ConversionParameters(14.6959, 0));

			Assert.IsTrue(dim.AreUnitFactorsDimensionallyEquivalent(AlgebraicFactor.FromSymbol("atm"), psi));
		}

		[TestMethod]
		public void WithFundamentalUnit_TestGetConversionParameters_ExpectedRightValue()
		{
			var dim = new DimensionalAnalyzer();
			dim.AddFundamentalDimension("distance", "m");
			dim.AddMultiplierMeasurementUnit("km", "m", new ConversionParameters(1000, 0));

			var result = dim.GetConversionParameters(AlgebraicFactor.FromSymbol("m"), AlgebraicFactor.FromSymbol("km"));

			Assert.AreEqual(0.001, result.Factor);
			Assert.AreEqual(0.000, result.Offset);
		}

		[TestMethod]
		public void WithMultiplierUnit_TestGetConversionParameters_ExpectedRightValue()
		{
			var dim = new DimensionalAnalyzer();
			dim.AddFundamentalDimension("distance", "m");
			dim.AddMultiplierMeasurementUnit("km", "m", new ConversionParameters(1000, 0));

			var result = dim.GetConversionParameters(AlgebraicFactor.FromSymbol("km"), AlgebraicFactor.FromSymbol("m"));

			Assert.AreEqual(1000, result.Factor);
			Assert.AreEqual(0.000, result.Offset);
		}

		[TestMethod]
		public void WithFractionalMultiplierUnit_TestGetConversionParameters_ExpectedRightValue()
		{
			var dim = new DimensionalAnalyzer();
			dim.AddFundamentalDimension("temperature", "C");
			dim.AddMultiplierMeasurementUnit("F", "C", new ConversionParameters(5.0/9, -160.0/9));

			var result = dim.GetConversionParameters(AlgebraicFactor.FromSymbol("F"), AlgebraicFactor.FromSymbol("C"));

			Assert.AreEqual(5.0/9, result.Factor, 1e-6);
			Assert.AreEqual(-160.0/9, result.Offset, 1e-6);
		}

		[TestMethod]
		public void WithFractionalMultiplierUnit_TestGetConversionParameters_ExpectedRightValue2()
		{
			var dim = new DimensionalAnalyzer();
			dim.AddFundamentalDimension("temperature", "C");
			dim.AddMultiplierMeasurementUnit("F", "C", new ConversionParameters(5.0/9, -160.0/9));

			var result = dim.GetConversionParameters(AlgebraicFactor.FromSymbol("C"), AlgebraicFactor.FromSymbol("F"));

			Assert.AreEqual(9.0/5, result.Factor, 1e-6);
			Assert.AreEqual(32.0, result.Offset, 1e-6);
		}

		[TestMethod]
		public void WithSimpleConversionParameters_TestApplyConversion_ExpectedRightValue()
		{
			var dim = new DimensionalAnalyzer();
			var conversionParameters = new ConversionParameters(1000.0, 5);
			var result = dim.ApplyConversion(5, conversionParameters);

			Assert.AreEqual(5005.0, result, 1e-6);
		}

		[TestMethod]
		public void WithUnits_TestAreDimensionallyEquivalent_ExpectedTrue()
		{
			var dim = new DimensionalAnalyzer();
			dim.AddFundamentalDimension("distance", "m");
			dim.AddFundamentalDimension("time", "h");
			dim.AddComposedDimension("speed", "mph",
				AlgebraicFactor.FromSymbol("m").Divide(AlgebraicFactor.FromSymbol("h")), new ConversionParameters(1, 0));

			var result = dim.AreDimensionalFactorsDimensionallyEquivalent(
				AlgebraicFactor.FromSymbol("speed"),
				AlgebraicFactor.FromSymbol("distance").Divide(AlgebraicFactor.FromSymbol("time")));

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void WithExponentialUnits_TestAreDimensionallyEquivalent_ExpectedTrue()
		{
			var dim = new DimensionalAnalyzer();
			dim.AddFundamentalDimension("distance", "m");
			dim.AddFundamentalDimension("time", "h");
			dim.AddComposedDimension("acceleration", "mph2",
				AlgebraicFactor.FromSymbol("m").Divide(AlgebraicFactor.FromSymbol("h").Multiply(AlgebraicFactor.FromSymbol("h"))),
				new ConversionParameters(1, 0));

			var result = dim.AreDimensionalFactorsDimensionallyEquivalent(
				AlgebraicFactor.FromSymbol("acceleration"),
				AlgebraicFactor.FromSymbol("distance").Divide(AlgebraicFactor.FromSymbol("time", 2)));

			Assert.IsTrue(result);
		}

		[TestMethod]
		public void WithExponentialUnits_TestAreDimensionallyEquivalent_ExpectedFalse()
		{
			var dim = new DimensionalAnalyzer();
			dim.AddFundamentalDimension("distance", "m");
			dim.AddFundamentalDimension("time", "h");
			dim.AddComposedDimension("acceleration", "mph2",
				AlgebraicFactor.FromSymbol("m").Divide(AlgebraicFactor.FromSymbol("h").Multiply(AlgebraicFactor.FromSymbol("h"))),
				new ConversionParameters(1, 0));

			var result = dim.AreDimensionalFactorsDimensionallyEquivalent(
				AlgebraicFactor.FromSymbol("time"),
				AlgebraicFactor.FromSymbol("distance").Divide(AlgebraicFactor.FromSymbol("time")));

			Assert.IsFalse(result);
		}
	}
}