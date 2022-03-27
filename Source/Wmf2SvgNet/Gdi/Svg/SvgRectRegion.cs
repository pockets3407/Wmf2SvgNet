using System.Xml.Linq;

namespace Wmf2SvgNet.Gdi.Svg
{

	class SvgRectRegion : SvgRegion
	{
		private readonly XNamespace svg = "http://www.w3.org/2000/svg";

		public short Left { get; private set; }
		public short Top { get; private set; }
		public short Right { get; private set; }
		public short Bottom { get; private set; }

		public SvgRectRegion(SvgGdi gdi, short left, short top, short right, short bottom) : base(gdi)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public override XElement CreateElement()
			=> new XElement(svg + "rect",
				new XAttribute("x", (int)GDI.DC.ToAbsoluteX(Left)),
				new XAttribute("y", (int)GDI.DC.ToAbsoluteY(Top)),
				new XAttribute("width", (int)GDI.DC.ToRelativeX(Right - Left)),
				new XAttribute("height", (int)GDI.DC.ToRelativeY(Bottom - Top))
			);

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + Bottom;
			result = prime * result + Left;
			result = prime * result + Right;
			result = prime * result + Top;
			return result;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
				return true;
			if (obj == null)
				return false;
			if (GetType() != obj.GetType())
				return false;
			SvgRectRegion other = (SvgRectRegion)obj;
			if (Bottom != other.Bottom)
				return false;
			if (Left != other.Left)
				return false;
			if (Right != other.Right)
				return false;
			if (Top != other.Top)
				return false;
			return true;
		}
	}
}