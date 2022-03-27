/*
 * Copyright 2007-2008 Hidekatsu Izuno
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
 * either express or implied. See the License for the specific language
 * governing permissions and limitations under the License.
 */
using System.Text;
using System.Xml.Linq;

namespace Wmf2SvgNet.Gdi.Svg
{

	public class SvgBrush : SvgObject, IGdiBrush
	{
		private readonly XNamespace svg = "http://www.w3.org/2000/svg";

		public SvgBrush(SvgGdi gdi, ushort style, int color, ushort hatch) : base(gdi) 
		{
			Style = style;
			Color = color;
			Hatch = hatch;
		}

        public ushort Style { get; private set; }

        public int Color { get; private set; }

        public ushort Hatch { get; private set; }

        public XElement CreateFillPattern(string id) 
		{
			XElement pattern = null;

			if (Style == Constants.BS_HATCHED) 
			{
				pattern = new XElement(svg + "pattern",
					new XAttribute("id", id),
					new XAttribute("patternUnits", "userSpaceOnUse"),
					new XAttribute("x", ToRealSize(0)),
					new XAttribute("y", ToRealSize(0)),
					new XAttribute("width", ToRealSize(8)),
					new XAttribute("height", ToRealSize(8))
				);

				if (GDI.DC.BkMode == Constants.OPAQUE) 
				{
					XElement rect = new XElement(svg + "rect",
						new XAttribute("fill", ToColor(GDI.DC.BkColor)),
						new XAttribute("x", ToRealSize(0)),
						new XAttribute("y", ToRealSize(0)),
						new XAttribute("width", ToRealSize(8)),
						new XAttribute("height", ToRealSize(8))
					);
					pattern.Add(rect);
				}

				switch (Hatch) 
				{
					case Constants.HS_HORIZONTAL: 
						pattern.Add(new XElement(svg + "line",
							new XAttribute("stroke", ToColor(Color)),
							new XAttribute("x1", ToRealSize(0)),
							new XAttribute("y1", ToRealSize(4)),
							new XAttribute("x2", ToRealSize(8)),
							new XAttribute("y2", ToRealSize(4))
						));
						break;
					case Constants.HS_VERTICAL:
						pattern.Add(new XElement(svg + "line",
							new XAttribute("stroke", ToColor(Color)),
							new XAttribute("x1", ToRealSize(4)),
							new XAttribute("y1", ToRealSize(0)),
							new XAttribute("x2", ToRealSize(4)),
							new XAttribute("y2", ToRealSize(8))
						));
						break;
					case Constants.HS_FDIAGONAL: 
						pattern.Add(new XElement(svg + "line",
							new XAttribute("stroke", ToColor(Color)),
							new XAttribute("x1", ToRealSize(0)),
							new XAttribute("y1", ToRealSize(0)),
							new XAttribute("x2", ToRealSize(8)),
							new XAttribute("y2", ToRealSize(8))
						));
						break;
					case Constants.HS_BDIAGONAL:
						pattern.Add(new XElement(svg + "line",
							new XAttribute("stroke", ToColor(Color)),
							new XAttribute("x1", ToRealSize(0)),
							new XAttribute("y1", ToRealSize(8)),
							new XAttribute("x2", ToRealSize(8)),
							new XAttribute("y2", ToRealSize(0))
						));
						break;
					case Constants.HS_CROSS: 
						pattern.Add(new XElement(svg + "line",
							new XAttribute("stroke", ToColor(Color)),
							new XAttribute("x1", ToRealSize(0)),
							new XAttribute("y1", ToRealSize(4)),
							new XAttribute("x2", ToRealSize(8)),
							new XAttribute("y2", ToRealSize(4))
						));
						pattern.Add(new XElement(svg + "line",
							new XAttribute("stroke", ToColor(Color)),
							new XAttribute("x1", ToRealSize(4)),
							new XAttribute("y1", ToRealSize(0)),
							new XAttribute("x2", ToRealSize(4)),
							new XAttribute("y2", ToRealSize(8)))
						);
						break;
					case Constants.HS_DIAGCROSS:
							pattern.Add(new XElement(svg + "line",
								new XAttribute("stroke", ToColor(Color)),
								new XAttribute("x1", ToRealSize(0)),
								new XAttribute("y1", ToRealSize(0)),
								new XAttribute("x2", ToRealSize(8)),
								new XAttribute("y2", ToRealSize(8)))
							);
							pattern.Add(new XElement(svg + "line",
								new XAttribute("stroke", ToColor(Color)),
								new XAttribute("x1", ToRealSize(0)),
								new XAttribute("y1", ToRealSize(8)),
								new XAttribute("x2", ToRealSize(8)),
								new XAttribute("y2", ToRealSize(0)))
							);
						break;
				}
			}

			return pattern;
		}

		public override int GetHashCode() 
		{
			const int PRIME = 31;
			int result = 1;
			result = PRIME * result + Color;
			result = PRIME * result + Hatch;
			result = PRIME * result + Style;
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
			SvgBrush other = (SvgBrush)obj;
			if (Color != other.Color)
				return false;
			if (Hatch != other.Hatch)
				return false;
			if (Style != other.Style)
				return false;
			return true;
		}

		public XText CreateTextNode(string id) 
			=> new XText("." + id + " { " + ToString() + " }\n");

		public override string ToString() 
		{
			StringBuilder buffer = new StringBuilder();

			// fill
			switch (Style) 
			{
				case Constants.BS_SOLID:
					buffer.Append("fill: ").Append(ToColor(Color)).Append("; ");
					break;
				case Constants.BS_HATCHED:
					break;
				default:
					buffer.Append("fill: none; ");
					break;
			}

			return buffer.ToString();
		}
	}
}