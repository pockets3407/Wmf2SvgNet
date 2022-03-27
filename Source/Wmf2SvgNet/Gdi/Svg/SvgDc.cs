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
using System;
using System.Linq;
using System.Xml.Linq;

namespace Wmf2SvgNet.Gdi.Svg
{

	public class SvgDc : ICloneable
	{
		private readonly XNamespace svg = "http://www.w3.org/2000/svg";

		private SvgGdi gdi;

		private ushort dpi = 1440;

        // window offset
        private short wox = 0;
		private short woy = 0;

		// window scale
		private double wsx = 1.0;
		private double wsy = 1.0;

		// mapping scale
		private double mx = 1.0;
		private double my = 1.0;

		// viewport
		private short vx = 0;
		private short vy = 0;
		private short vw = 0;
		private short vh = 0;

		// viewport offset
		private short vox = 0;
		private short voy = 0;

		// viewport scale
		private double vsx = 1.0;
		private double vsy = 1.0;
        
		private short mapMode = Constants.MM_TEXT;

        public SvgBrush Brush { get; set; } = null;
		public SvgFont Font { get; set; } = null;
		public SvgPen Pen { get; set; } = null;

		public XElement Mask { get; set; } = null;

		public SvgDc(SvgGdi gdi)
		{
			this.gdi = gdi;
		}

		public ushort Dpi { get => dpi; set => dpi = (value > 0) ? value : (ushort)1440; }

		public void SetWindowOrgEx(short x, short y, Point old)
		{
			if (old != null)
			{
				old.X = WindowX;
				old.Y = WindowY;
			}
			WindowX = x;
			WindowY = y;
		}

		public void SetWindowExtEx(short width, short height, Size old)
		{
			if (old != null)
			{
				old.Width = WindowWidth;
				old.Height = WindowHeight;
			}
			WindowWidth = width;
			WindowHeight = height;
		}

		public void OffsetWindowOrgEx(short x, short y, Point old)
		{
			if (old != null)
			{
				old.X = wox;
				old.Y = woy;
			}
			wox += x;
			woy += y;
		}

		public void ScaleWindowExtEx(short x, short xd, short y, short yd, Size old)
		{
			// TODO
			wsx = (wsx * x) / xd;
			wsy = (wsy * y) / yd;
		}

        public short WindowX { get; private set; } = 0;

        public short WindowY { get; private set; } = 0;

        public short WindowWidth { get; private set; } = 0;

        public short WindowHeight { get; private set; } = 0;

        public void SetViewportOrgEx(short x, short y, Point old)
		{
			if (old != null)
			{
				old.X = vx;
				old.Y = vy;
			}
			vx = x;
			vy = y;
		}

		public void SetViewportExtEx(short width, short height, Size old)
		{
			if (old != null)
			{
				old.Width = vw;
				old.Height = vh;
			}
			vw = width;
			vh = height;
		}

		public void OffsetViewportOrgEx(short x, short y, Point old)
		{
			if (old != null)
			{
				old.X = vox;
				old.Y = voy;
			}
			vox = x;
			voy = y;
		}

		public void ScaleViewportExtEx(short x, short xd, short y, short yd, Size old)
		{
			// TODO
			vsx = (vsx * x) / xd;
			vsy = (vsy * y) / yd;
		}

		public void OffsetClipRgn(short x, short y)
		{
			OffsetClipX = x;
			OffsetClipY = y;
		}

		public short MapMode
		{
			get => mapMode;
			set
			{

				mapMode = value;
				switch (mapMode)
				{
					case Constants.MM_HIENGLISH:
						mx = 0.09;
						my = -0.09;
						break;
					case Constants.MM_LOENGLISH:
						mx = 0.9;
						my = -0.9;
						break;
					case Constants.MM_HIMETRIC:
						mx = 0.03543307;
						my = -0.03543307;
						break;
					case Constants.MM_LOMETRIC:
						mx = 0.3543307;
						my = -0.3543307;
						break;
					case Constants.MM_TWIPS:
						mx = 0.0625;
						my = -0.0625;
						break;
					default:
						mx = 1.0;
						my = 1.0;
						break;
				}
			}
		}

        public short CurrentX { get; private set; } = 0;

        public short CurrentY { get; private set; } = 0;

        public int OffsetClipX { get; private set; } = 0;

        public int OffsetClipY { get; private set; } = 0;

        public void MoveToEx(short x, short y, Point old)
		{
			if (old != null)
			{
				old.X = CurrentX;
				old.Y = CurrentY;
			}
			CurrentX = x;
			CurrentY = y;
		}

		public double ToAbsoluteX(double x)
		{
			// TODO Handle Viewport
			return ((WindowWidth >= 0) ? 1 : -1) * (mx * x - (WindowX + wox)) / wsx;
		}

		public double ToAbsoluteY(double y)
		{
			// TODO Handle Viewport
			return ((WindowHeight >= 0) ? 1 : -1) * (my * y - (WindowY + woy)) / wsy;
		}

		public double ToRelativeX(double x)
		{
			// TODO Handle Viewport
			return ((WindowWidth >= 0) ? 1 : -1) * (mx * x) / wsx;
		}

		public double ToRelativeY(double y)
		{
			// TODO Handle Viewport
			return ((WindowHeight >= 0) ? 1 : -1) * (my * y) / wsy;
		}

        public int BkColor { get; set; } = 0x00FFFFFF;

        public short BkMode { get; set; } = Constants.OPAQUE;

        public int TextColor { get; set; } = 0x00000000;

        public short PolyFillMode { get; set; } = Constants.ALTERNATE;

        public short RelAbs { get; set; } = 0;

        public short ROP2 { get; set; } = Constants.R2_COPYPEN;

        public short StretchBltMode { get; set; } = Constants.STRETCH_ANDSCANS;

        public short TextSpace { get; set; } = 0;

        public short TextAlign { get; set; } = Constants.TA_TOP | Constants.TA_LEFT;

        public short TextCharacterExtra { get; set; } = 0;

        public uint Layout { get; set; } = 0;

        public uint MapperFlags { get; set; } = 0;

        public string GetRopFilter(uint rop)
		{
			string name = null;
			XDocument doc = gdi.Document;

			if (rop == Constants.BLACKNESS)
			{
				name = "BLACKNESS_FILTER";
				XElement filter = doc.Descendants().FirstOrDefault(x => string.Equals(x.Name.LocalName, "filter") && string.Equals(x.Attribute("id"), name));
				if (filter == null)
				{
					filter = new XElement(svg + "filter", 
						new XAttribute("id", name),
						new XElement(svg + "feColorMatrix",
							new XAttribute("type", "matrix"),
							new XAttribute("in", "SourceGraphic"),
							new XAttribute("values", "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 0")
						)
					);

					gdi.DefsElement.Add(filter);
				}
			}
			else if (rop == Constants.NOTSRCERASE)
			{
				name = "NOTSRCERASE_FILTER";
				XElement filter = doc.Descendants().FirstOrDefault(x => string.Equals(x.Name.LocalName, "filter") && string.Equals(x.Attribute("id"), name));
				if (filter == null)
				{
					filter = new XElement(svg + "filter",
						new XAttribute("id", name),
						new XElement(svg + "feComposite",
							new XAttribute("in", "SourceGraphic"),
							new XAttribute("in2", "BackgroundImage"),
							new XAttribute("operator", "arithmetic"),
							new XAttribute("k1", "1"),
							new XAttribute("result", "result0")
						),
						new XElement(svg + "feColorMatrix",
							new XAttribute("in", "result0"),
							new XAttribute("values", "-1 0 0 0 1 0 -1 0 0 1 0 0 -1 0 1 0 0 0 1 0")
						)
					);

					gdi.DefsElement.Add(filter);
				}
			}
			else if (rop == Constants.NOTSRCCOPY)
			{
				name = "NOTSRCCOPY_FILTER";
				XElement filter = doc.Descendants().FirstOrDefault(x => string.Equals(x.Name.LocalName, "filter") && string.Equals(x.Attribute("id"), name));
				if (filter == null)
				{
					filter = new XElement(svg + "filter",
						new XAttribute("id", name),
						new XElement(svg + "feColorMatrix",
							new XAttribute("type", "matrix"),
							new XAttribute("in", "SourceGraphic"),
							new XAttribute("values", "-1 0 0 0 1 0 -1 0 0 1 0 0 -1 0 1 0 0 0 1 0")
						)
					);

					gdi.DefsElement.Add(filter);
				}
			}
			else if (rop == Constants.SRCERASE)
			{
				name = "SRCERASE_FILTER";
				XElement filter = doc.Descendants().FirstOrDefault(x => string.Equals(x.Name.LocalName, "filter") && string.Equals(x.Attribute("id"), name));
				if (filter == null)
				{
					filter = new XElement(svg + "filter",
						new XAttribute("id", name),
						new XElement(svg + "feColorMatrix",
							new XAttribute("type", "matrix"),
							new XAttribute("in", "BackgroundImage"),
							new XAttribute("values", "-1 0 0 0 1 0 -1 0 0 1 0 0 -1 0 1 0 0 0 1 0"),
							new XAttribute("result", "result0")
						),
						new XElement(svg + "feComposite",
							new XAttribute("in", "SourceGraphic"),
							new XAttribute("in2", "result0"),
							new XAttribute("operator", "arithmetic"),
							new XAttribute("k2", "1"),
							new XAttribute("k3", "1")
						)
					);

					gdi.DefsElement.Add(filter);
				}
			}
			else if (rop == Constants.PATINVERT)
			{
				// TODO
			}
			else if (rop == Constants.SRCINVERT)
			{
				// TODO
			}
			else if (rop == Constants.DSTINVERT)
			{
				name = "DSTINVERT_FILTER";
				XElement filter = doc.Descendants().FirstOrDefault(x => string.Equals(x.Name.LocalName, "filter") && string.Equals(x.Attribute("id"), name));
				if (filter == null)
				{
					filter = new XElement(svg + "filter",
						new XAttribute("id", name),
						new XElement(svg + "feColorMatrix",
							new XAttribute("type", "matrix"),
							new XAttribute("in", "BackgroundImage"),
							new XAttribute("values", "-1 0 0 0 1 0 -1 0 0 1 0 0 -1 0 1 0 0 0 1 0")
						)
					);

					gdi.DefsElement.Add(filter);
				}
			}
			else if (rop == Constants.SRCAND)
			{
				name = "SRCAND_FILTER";
				XElement filter = doc.Descendants().FirstOrDefault(x => string.Equals(x.Name.LocalName, "filter") && string.Equals(x.Attribute("id"), name));
				if (filter == null)
				{
					filter = new XElement(svg + "filter", 
						new XAttribute("id", name),
						new XElement(svg + "feComposite",
							new XAttribute("in", "SourceGraphic"),
							new XAttribute("in2", "BackgroundImage"),
							new XAttribute("operator", "arithmetic"),
							new XAttribute("k1", "1")
						)
					);

					gdi.DefsElement.Add(filter);
				}
			}
			else if (rop == Constants.MERGEPAINT)
			{
				name = "MERGEPAINT_FILTER";
				XElement filter = doc.Descendants().FirstOrDefault(x => string.Equals(x.Name.LocalName, "filter") && string.Equals(x.Attribute("id"), name));
				if (filter == null)
				{
					filter = new XElement(svg + "filter", 
						new XAttribute("id", name),
						new XElement(svg + "feColorMatrix",
							new XAttribute("type", "matrix"),
							new XAttribute("in", "SourceGraphic"),
							new XAttribute("values", "-1 0 0 0 1 0 -1 0 0 1 0 0 -1 0 1 0 0 0 1 0"),
							new XAttribute("result", "result0")
						),
						new XElement(svg + "feComposite",
							new XAttribute("in", "result0"),
							new XAttribute("in2", "BackgroundImage"),
							new XAttribute("operator", "arithmetic"),
							new XAttribute("k1", "1")
						)
					);

					gdi.DefsElement.Add(filter);
				}
			}
			else if (rop == Constants.MERGECOPY)
			{
				// TODO
			}
			else if (rop == Constants.SRCPAINT)
			{
				name = "SRCPAINT_FILTER";
				XElement filter = doc.Descendants().FirstOrDefault(x => string.Equals(x.Name.LocalName, "filter") && string.Equals(x.Attribute("id"), name));
				if (filter == null)
				{
					filter = new XElement(svg + "filter", 
						new XAttribute("id", name),
						new XElement(svg + "feComposite",
							new XAttribute("in", "SourceGraphic"),
							new XAttribute("in2", "BackgroundImage"),
							new XAttribute("operator", "arithmetic"),
							new XAttribute("k2", "1"),
							new XAttribute("k3", "1")
						)
					);

					gdi.DefsElement.Add(filter);
				}
			}
			else if (rop == Constants.PATCOPY)
			{
				// TODO
			}
			else if (rop == Constants.PATPAINT)
			{
				// TODO
			}
			else if (rop == Constants.WHITENESS)
			{
				name = "WHITENESS_FILTER";
				XElement filter = doc.Descendants().FirstOrDefault(x => string.Equals(x.Name.LocalName, "filter") && string.Equals(x.Attribute("id"), name));
				if (filter == null)
				{
					filter = new XElement(svg + "filter", 
						new XAttribute("id", name),
						new XElement(svg + "feColorMatrix",
							new XAttribute("type", "matrix"),
							new XAttribute("in", "SourceGraphic"),
							new XAttribute("values", "1 0 0 0 1 0 1 0 0 1 0 0 1 0 1 0 0 0 1 0")
						)
					);

					gdi.DefsElement.Add(filter);
				}
			}

			if (name != null)
			{
				if (doc.Root.Attribute("enable-background") == null)
					doc.Root.Add(new XAttribute("enable-background", "new"));

				return "url(#" + name + ")";
			}
			return null;
		}

		public object Clone()
			=> new SvgDc(gdi);

		public override string ToString()
		{
			return "SvgDc [gdi=" + gdi + ", dpi=" + dpi + ", wx=" + WindowX + ", wy="
					+ WindowY + ", ww=" + WindowWidth + ", wh=" + WindowHeight + ", wox=" + wox + ", woy="
					+ woy + ", wsx=" + wsx + ", wsy=" + wsy + ", mx=" + mx
					+ ", my=" + my + ", vx=" + vx + ", vy=" + vy + ", vw=" + vw
					+ ", vh=" + vh + ", vox=" + vox + ", voy=" + voy + ", vsx="
					+ vsx + ", vsy=" + vsy + ", cx=" + CurrentX + ", cy=" + CurrentY
					+ ", mapMode=" + mapMode + ", bkColor=" + BkColor + ", bkMode="
					+ BkMode + ", textColor=" + TextColor + ", textSpace="
					+ TextSpace + ", textAlign=" + TextAlign + ", textDx=" + TextCharacterExtra
					+ ", polyFillMode=" + PolyFillMode + ", relAbsMode="
					+ RelAbs + ", rop2Mode=" + ROP2 + ", stretchBltMode="
					+ StretchBltMode + ", brush=" + Brush + ", font=" + Font
					+ ", pen=" + Pen + "]";
		}
	}
}