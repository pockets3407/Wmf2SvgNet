namespace Wmf2SvgNet.Gdi.Wmf
{

	public class WmfBrush : WmfObject, IGdiBrush
	{
		public ushort Style { get; private set; }
		public int Color { get; private set; }
		public ushort Hatch { get; private set; }

		public WmfBrush(ushort id, ushort style, int color, ushort hatch) : base(id)
		{
			Style = style;
			Color = color;
			Hatch = hatch;
		}
	}
}