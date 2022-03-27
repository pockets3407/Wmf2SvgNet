using System.Xml.Linq;

namespace Wmf2SvgNet.Gdi.Svg
{

	abstract class SvgRegion : SvgObject, IGdiRegion
	{
		public SvgRegion(SvgGdi gdi) : base(gdi)
		{
		}

		public abstract XElement CreateElement();
	}
}
