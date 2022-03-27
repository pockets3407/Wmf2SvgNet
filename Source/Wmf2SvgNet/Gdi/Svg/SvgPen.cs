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

	public class SvgPen : SvgObject, IGdiPen
	{

		public ushort Style { get; private set; }
		public short Width { get; private set; }
		public int Color { get; private set; }

		public SvgPen(SvgGdi gdi, ushort style, short width, int color) : base(gdi)
		{
			Style = style;
			Width = (width > 0) ? width : (short)1;
			Color = color;
		}

		public override int GetHashCode() 
		{
			const int PRIME = 31;
			int result = 1;
			result = PRIME * result + Color;
			result = PRIME * result + Style;
			result = PRIME * result + Width;
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
			SvgPen other = (SvgPen)obj;
			if (Color != other.Color)
				return false;
			if (Style != other.Style)
				return false;
			if (Width != other.Width)
				return false;
			return true;
		}

		public XText CreateTextNode(string id) 
			=> new XText("." + id + " { " + ToString() + " }\n");

		public override string ToString() 
		{
			StringBuilder buffer = new StringBuilder();

			if (Style == Constants.PS_NULL) 
			{
				buffer.Append("stroke: none; ");
			}
			else
			{
				// stroke
				buffer.Append("stroke: " + ToColor(Color) + "; ");

				// stroke-width
				buffer.Append("stroke-width: " + Width + "; ");

				// stroke-linejoin
				buffer.Append("stroke-linejoin: round; ");

				// stroke-dasharray
				if (Width == 1 && Constants.PS_DASH <= Style && Style <= Constants.PS_DASHDOTDOT) {
					buffer.Append("stroke-dasharray: ");
					switch (Style) {
						case Constants.PS_DASH:
							buffer.Append(
								ToRealSize(18) + "," + ToRealSize(6));
							break;
						case Constants.PS_DOT:
							buffer.Append(ToRealSize(3) + "," + ToRealSize(3));
							break;
						case Constants.PS_DASHDOT:
							buffer.Append(
									ToRealSize(9)
									+ ","
									+ ToRealSize(3)
									+ ","
									+ ToRealSize(3)
									+ ","
									+ ToRealSize(3));
							break;
						case Constants.PS_DASHDOTDOT:
							buffer.Append(
									ToRealSize(9)
									+ ","
									+ ToRealSize(3)
									+ ","
									+ ToRealSize(3)
									+ ","
									+ ToRealSize(3)
									+ ","
									+ ToRealSize(3)
									+ ","
									+ ToRealSize(3));
							break;
					}
					buffer.Append("; ");
				}
			}

			return buffer.ToString();
		}
	}
}