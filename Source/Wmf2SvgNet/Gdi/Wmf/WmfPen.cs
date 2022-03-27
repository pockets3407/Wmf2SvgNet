namespace Wmf2SvgNet.Gdi.Wmf
{

	public class WmfPen : WmfObject, IGdiPen
	{
        public WmfPen(ushort id, ushort style, short width, int color) : base(id) 
        {
            Style = style;
            Width = width;
            Color = color;
        }

        public ushort Style { get; }

        public short Width { get; }

        public int Color { get; }
    }
}