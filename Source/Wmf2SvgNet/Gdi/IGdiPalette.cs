namespace Wmf2SvgNet.Gdi
{

	public interface IGdiPalette : IGdiObject
	{
		ushort Version { get; }
		int[] Entries { get; }
	}
}