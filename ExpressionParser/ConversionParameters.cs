namespace DXAppProto2
{
	public struct ConversionParameters
	{
		public ConversionParameters(double factor, double offset)
		{
			Factor = factor;
			Offset = offset;
		}

		public double Factor { get; }

		public double Offset { get; }
	}
}