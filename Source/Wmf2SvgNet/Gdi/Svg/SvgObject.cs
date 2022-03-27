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
namespace Wmf2SvgNet.Gdi.Svg
{

	public abstract class SvgObject
	{
        public SvgObject(SvgGdi gdi)
		{
			this.GDI = gdi;
		}

        public SvgGdi GDI { get; private set; }

		public short ToRealSize(short px) => px;
		//public int ToRealSize(int px) => GDI.DC.Dpi * px / 90;

		public static string ToColor(int color)
		{
			if (color == 0x00000000 || (uint)color == 0xff000000)
				return "black";
			else if (color == 0x00ffffff || (uint)color == 0xffffffff)
				return "white";
			else if (color == 0x000000ff || (uint)color == 0xff0000ff)
				return "red";
			else if (color == 0x0000ff00 || (uint)color == 0xff00ff00)
				return "green";
			else if (color == 0x00ff0000 || (uint)color == 0xffff0000)
				return "blue";

			int b = (0x00FF0000 & color) >> 16;
			int g = (0x0000FF00 & color) >> 8;
			int r = (0x000000FF & color);

			return "rgb(" + r + "," + g + "," + b + ")";
		}
	}
}