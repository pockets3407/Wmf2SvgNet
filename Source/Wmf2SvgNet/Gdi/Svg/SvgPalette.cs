namespace Wmf2SvgNet.Gdi.Svg
{

	public class SvgPalette : SvgObject, IGdiPalette
	{
		public ushort Version { get; private set; }
		public int[] Entries { get; private set; }

		public SvgPalette(SvgGdi gdi, ushort version, int[] entries) : base(gdi)
		{
			Version = version;
			Entries = entries;
		}
	}
}