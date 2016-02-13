namespace DXAppProto2
{
	/// <summary>
	/// Contains the conversion parameters need to apply a linear measurement units conversion
	/// </summary>
	public struct ConversionParameters
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ConversionParameters"/> struct.
		/// </summary>
		/// <param name="factor">The factor.</param>
		/// <param name="offset">The offset.</param>
		public ConversionParameters(double factor, double offset)
		{
			this.Factor = factor;
			this.Offset = offset;
		}

		/// <summary>
		/// Gets the factor.
		/// </summary>
		/// <value> The factor. </value>
		public double Factor { get; }

		/// <summary>
		/// Gets the offset.
		/// </summary>
		/// <value> The offset. </value>
		public double Offset { get; }
	}
}