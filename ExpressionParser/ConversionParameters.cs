namespace DXAppProto2
{
	public struct ConversionParameters
	{
		public ConversionParameters(double factor, double offset)
		{
			this.Factor = factor;
			this.Offset = offset;
		}

		public double Factor { get; }

		public double Offset { get; }
	}
}