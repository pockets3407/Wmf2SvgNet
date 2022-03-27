namespace Wmf2SvgNet.Gdi.Wmf
{

	public class WmfFont : WmfObject, IGdiFont
	{
        public WmfFont(ushort id,
			short height,
			short width,
			short escapement,
			short orientation,
			short weight,
			bool italic,
			bool underline,
			bool strikeout,
			byte charset,
			byte outPrecision,
			byte clipPrecision,
			byte quality,
			byte pitchAndFamily,
			byte[] faceName) : base(id)
		{
			Height = height;
			Width = width;
			Escapement = escapement;
			Orientation = orientation;
			Weight = weight;
			IsItalic = italic;
			IsUnderlined = underline;
			IsStrikedOut = strikeout;
			Charset = charset;
			OutPrecision = outPrecision;
			ClipPrecision = clipPrecision;
			Quality = quality;
			PitchAndFamily = pitchAndFamily;
			FaceName = GdiUtils.ConvertString(faceName, charset);
		}

        public short Height { get; private set; }

        public short Width { get; private set; }

        public short Escapement { get; private set; }

        public short Orientation { get; private set; }

        public short Weight { get; private set; }

        public bool IsItalic { get; private set; }

        public bool IsUnderlined { get; private set; }

        public bool IsStrikedOut { get; private set; }

        public byte Charset { get; private set; }

        public byte OutPrecision { get; private set; }

        public byte ClipPrecision { get; private set; }

        public byte Quality { get; private set; }

        public byte PitchAndFamily { get; private set; }

        public string FaceName { get; private set; }
    }
}