namespace Wmf2SvgNet.Gdi.Svg
{

	public class SvgPatternBrush : SvgObject, IGdiPatternBrush
	{
		public SvgPatternBrush(SvgGdi gdi, byte[] bmp) : base(gdi)
		{
			Pattern = bmp;
		}

		public byte[] Pattern { get; private set; }
	}
}