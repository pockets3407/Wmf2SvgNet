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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace Wmf2SvgNet.Gdi.Svg
{

	public class SvgFont : SvgObject, IGdiFont
	{
        private double heightMultiply = 1.0;

        public SvgFont(
			SvgGdi gdi,
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
			byte[] faceName) : base(gdi)
		{

			Height = height;
			Width = width;
			Escapement = escapement;
			Orientation = orientation;
			Weight = weight;
			IsItalic = italic;
			IsUnderlined = underline;
			IsStrikedOut = strikeout;
			OutPrecision = outPrecision;
			ClipPrecision = clipPrecision;
			Quality = quality;
			PitchAndFamily = pitchAndFamily;
			FaceName = GdiUtils.ConvertString(faceName, charset);

			string altCharset = gdi.GetProperty("font-charset." + FaceName);
			Charset = altCharset != null ? byte.Parse(altCharset) : charset;

			// xml:lang
			this.Lang = GdiUtils.GetLanguage(charset);

			string emheight = gdi.GetProperty("font-emheight." + FaceName);
			if (emheight == null)
			{
				string alter = gdi.GetProperty("alternative-font." + FaceName);
				if (alter != null)
					emheight = gdi.GetProperty("font-emheight." + alter);
			}

			if (emheight != null)
				this.heightMultiply = double.Parse(emheight, CultureInfo.InvariantCulture);
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

        public string Lang { get; private set; }

        public short FontSize {  get => Math.Abs((short)GDI.DC.ToRelativeY(Height * heightMultiply)); }

		public override int GetHashCode()
		{
			const int PRIME = 31;
			int result = 1;
			result = PRIME * result + Charset;
			result = PRIME * result + ClipPrecision;
			result = PRIME * result + Escapement;
			result = PRIME * result + ((FaceName == null) ? 0 : FaceName.GetHashCode());
			result = PRIME * result + Height;
			result = PRIME * result + (IsItalic ? 1231 : 1237);
			result = PRIME * result + Orientation;
			result = PRIME * result + OutPrecision;
			result = PRIME * result + PitchAndFamily;
			result = PRIME * result + Quality;
			result = PRIME * result + (IsStrikedOut ? 1231 : 1237);
			result = PRIME * result + (IsUnderlined ? 1231 : 1237);
			result = PRIME * result + Weight;
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
			SvgFont other = (SvgFont)obj;
			if (Charset != other.Charset)
				return false;
			if (ClipPrecision != other.ClipPrecision)
				return false;
			if (Escapement != other.Escapement)
				return false;
			if (FaceName == null)
			{
				if (other.FaceName != null)
					return false;
			}
			else if (!FaceName.Equals(other.FaceName))
				return false;
			if (Height != other.Height)
				return false;
			if (IsItalic != other.IsItalic)
				return false;
			if (Orientation != other.Orientation)
				return false;
			if (OutPrecision != other.OutPrecision)
				return false;
			if (PitchAndFamily != other.PitchAndFamily)
				return false;
			if (Quality != other.Quality)
				return false;
			if (IsStrikedOut != other.IsStrikedOut)
				return false;
			if (IsUnderlined != other.IsUnderlined)
				return false;
			if (Weight != other.Weight)
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

			// font-style
			if (IsItalic)
				buffer.Append("font-style: italic; ");

			// font-weight
			if (Weight != Constants.FW_DONTCARE && Weight != Constants.FW_NORMAL)
			{
				if (Weight < 100)
					Weight = 100;
				else if (Weight > 900)
					Weight = 900;
				else
					Weight = (short)((Weight / 100) * 100);

				if (Weight == Constants.FW_BOLD)
					buffer.Append("font-weight: bold; ");
				else
					buffer.Append("font-weight: " + Weight + "; ");
			}

			int fontSize = FontSize;
			if (fontSize != 0) buffer.Append("font-size: ").Append(fontSize).Append("px; ");

			// font-family
			List<string> fontList = new List<string>();
			if (FaceName.Length != 0)
			{
				string fontFamily = FaceName;
				if (FaceName[0] == '@') fontFamily = FaceName.Substring(1);
				fontList.Add(fontFamily);

				string altfont = GDI.GetProperty("alternative-font." + fontFamily);
				if (altfont != null && altfont.Length != 0)
					fontList.Add(altfont);
			}

			// int pitch = pitchAndFamily & 0x00000003;
			int family = PitchAndFamily & 0x000000F0;
			switch (family)
			{
				case Constants.FF_DECORATIVE:
					fontList.Add("fantasy");
					break;
				case Constants.FF_MODERN:
					fontList.Add("monospace");
					break;
				case Constants.FF_ROMAN:
					fontList.Add("serif");
					break;
				case Constants.FF_SCRIPT:
					fontList.Add("cursive");
					break;
				case Constants.FF_SWISS:
					fontList.Add("sans-serif");
					break;
			}

			if (fontList.Count > 0)
			{
				buffer.Append("font-family:");
				bool first = true;
				foreach (var font in fontList)
				{
					if (font.Contains(" "))
						buffer.Append(" \"" + font + "\"");
					else
						buffer.Append(" " + font);

					if (!first)
						buffer.Append(",");
					first = false;
				}
				buffer.Append("; ");
			}

			// text-decoration
			if (IsUnderlined || IsStrikedOut)
			{
				buffer.Append("text-decoration:");
				if (IsUnderlined)
					buffer.Append(" underline");
				if (IsStrikedOut)
					buffer.Append(" overline");
				buffer.Append("; ");
			}

			return buffer.ToString();
		}
	}
}