namespace Wmf2SvgNet.Gdi.Wmf
{

	public class WmfPatternBrush : WmfObject, IGdiPatternBrush
	{
        public WmfPatternBrush(ushort id, byte[] image) : base(id)
		{
			this.Pattern = image;
		}

        public byte[] Pattern { get; private set; }
    }
}