namespace Wmf2SvgNet.Gdi.Wmf
{

	public class WmfRectRegion : WmfObject, IGdiRegion
	{
        public WmfRectRegion(ushort id, short left, short top, short right, short bottom) : base(id)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

        public short Left { get; private set; }

        public short Top { get; private set; }

        public short Right { get; private set; }

        public short Bottom { get; private set; }
    }
}