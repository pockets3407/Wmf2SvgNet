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

namespace Wmf2SvgNet.Gdi.Wmf
{

	public class WmfDc : ICloneable
	{

		// window offset
		private short wox = 0;
		private short woy = 0;

		// window scale
		private double wsx = 1.0;
		private double wsy = 1.0;

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

		// current location
		private short cx = 0;
		private short cy = 0;

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

		public void MoveToEx(short x, short y, Point old)
		{
			if (old != null)
			{
				old.X = cx;
				old.Y = cy;
			}
			cx = x;
			cy = y;
		}

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public short TextAlign { get; set; } = Constants.TA_TOP | Constants.TA_LEFT;

        public WmfBrush Brush { get; set; } = null;

        public WmfFont Font { get; set; } = null;

        public WmfPen Pen { get; set; } = null;
    }
}