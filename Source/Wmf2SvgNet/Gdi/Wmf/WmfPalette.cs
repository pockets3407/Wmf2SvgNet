namespace Wmf2SvgNet.Gdi.Wmf
{

	public class WmfPalette : WmfObject, IGdiPalette
	{
        public WmfPalette(ushort id, ushort version, int[] entries) : base(id)
		{
			Version = version;
			Entries = entries;
		}

        public ushort Version { get; private set; }

        public int[] Entries { get; private set; }
    }
}