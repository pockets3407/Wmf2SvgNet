using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Wmf2SvgNet.Gdi.Wmf
{

	public class WmfGdi : IGdi
	{
		private byte[] placeableHeader;
		private byte[] header;

		private List<IGdiObject> objects = new List<IGdiObject>();
		private List<byte[]> records = new List<byte[]>();

		private WmfDc dc = new WmfDc();

		private WmfBrush defaultBrush;

		private WmfPen defaultPen;

		private WmfFont defaultFont;

		public WmfGdi() 
		{
			defaultBrush = (WmfBrush)CreateBrushIndirect(Constants.BS_SOLID, 0x00FFFFFF, 0);
			defaultPen = (WmfPen)CreatePenIndirect(Constants.PS_SOLID, 1, 0x00000000);
			defaultFont = null;

			dc.Brush = defaultBrush;
			dc.Pen = defaultPen;
			dc.Font = defaultFont;
		}

		public void Write(Stream outputStream)
		{
			Footer();
			if (placeableHeader != null) outputStream.Write(placeableHeader, 0, placeableHeader.Length);
			if (header != null) outputStream.Write(header, 0, header.Length);

			foreach (var record in records)
				outputStream.Write(record, 0, record.Length);
			outputStream.Flush();
		}

		public void PlaceableHeader(short vsx, short vsy, short vex, short vey, ushort dpi)
		{
			byte[] record = new byte[22];
			int pos = 0;
			pos = SetUint32(record, pos, 0x9AC6CDD7);
			pos = SetInt16(record, pos, 0x0000);
			pos = SetInt16(record, pos, vsx);
			pos = SetInt16(record, pos, vsy);
			pos = SetInt16(record, pos, vex);
			pos = SetInt16(record, pos, vey);
			pos = SetUint16(record, pos, dpi);
			pos = SetUint32(record, pos, 0x00000000);

			ushort checksum = 0;
			for (int i = 0; i < record.Length - 2; i += 2)
				checksum ^= (ushort)((0xFF & record[i]) | ((0xFF & record[i + 1]) << 8));

			pos = SetUint16(record, pos, checksum);
			placeableHeader = record;
		}

		public void Header() {
			byte[] record = new byte[18];
			int pos = 0;
			pos = SetUint16(record, pos, (ushort)0x0001);
			pos = SetUint16(record, pos, (ushort)0x0009);
			pos = SetUint16(record, pos, (ushort)0x0300);
			pos = SetUint32(record, pos, (uint)0x0000); // dummy size
			pos = SetUint16(record, pos, (ushort)0x0000); // dummy noObjects
			pos = SetUint32(record, pos, (uint)0x0000); // dummy maxRecords
			pos = SetUint16(record, pos, (ushort)0x0000);
			header = record;
		}

		public void AnimatePalette(IGdiPalette palette, int startIndex, int[] entries)
		{
			byte[] record = new byte[22];
			int pos = 0;
			pos = SetUint32(record, pos, (uint)(record.Length / 2));
			pos = SetUint16(record, pos, (ushort)WmfConstants.RECORD_ANIMATE_PALETTE);
			pos = SetUint16(record, pos, (ushort)entries.Length);
			pos = SetUint16(record, pos, (ushort)startIndex);
			pos = SetUint16(record, pos, (ushort)((WmfPalette)palette).Id);
			for (int i = 0; i < entries.Length; i++)
				pos = SetInt32(record, pos, entries[i]);
			records.Add(record);
		}

		public void Arc(short sxr, short syr, short exr, short eyr, short sxa, short sya, short exa, short eya)
		{
			byte[] record = new byte[22];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_ARC);
			pos = SetInt16(record, pos, eya);
			pos = SetInt16(record, pos, exa);
			pos = SetInt16(record, pos, sya);
			pos = SetInt16(record, pos, sxa);
			pos = SetInt16(record, pos, eyr);
			pos = SetInt16(record, pos, exr);
			pos = SetInt16(record, pos, syr);
			pos = SetInt16(record, pos, sxr);
			records.Add(record);
		}

		public void BitBlt(byte[] image, short dx, short dy, short dw, short dh, short sx, short sy, uint rop)
		{
			byte[] record = new byte[22 + (image.Length + image.Length % 2)];
			int pos = 0;
			pos = SetUint32(record, pos, (uint)(record.Length / 2));
			pos = SetUint16(record, pos, (ushort)WmfConstants.RECORD_BIT_BLT);
			pos = SetUint32(record, pos, rop);
			pos = SetInt16(record, pos, sy);
			pos = SetInt16(record, pos, sx);
			pos = SetInt16(record, pos, dw);
			pos = SetInt16(record, pos, dh);
			pos = SetInt16(record, pos, dy);
			pos = SetInt16(record, pos, dx);
			pos = SetBytes(record, pos, image);
			if (image.Length % 2 == 1) pos = SetByte(record, pos, (byte)0);
			records.Add(record);
		}

		public void Chord(short sxr, short syr, short exr, short eyr, short sxa, short sya, short exa, short eya) 
		{
			byte[] record = new byte[22];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_CHORD);
			pos = SetInt16(record, pos, eya);
			pos = SetInt16(record, pos, exa);
			pos = SetInt16(record, pos, sya);
			pos = SetInt16(record, pos, sxa);
			pos = SetInt16(record, pos, eyr);
			pos = SetInt16(record, pos, exr);
			pos = SetInt16(record, pos, syr);
			pos = SetInt16(record, pos, sxr);
			records.Add(record);
		}

		public IGdiBrush CreateBrushIndirect(ushort style, int color, ushort hatch) 
		{
			byte[] record = new byte[14];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_CREATE_BRUSH_INDIRECT);
			pos = SetUint16(record, pos, style);
			pos = SetInt32(record, pos, color);
			pos = SetUint16(record, pos, hatch);
			records.Add(record);

			WmfBrush brush = new WmfBrush((ushort)objects.Count(), style, color, hatch);
			objects.Add(brush);
			return brush;
		}

		public IGdiFont CreateFontIndirect(short height, short width, short escapement, short orientation, short weight, bool italic, bool underline, bool strikeout, byte charset, byte outPrecision, byte clipPrecision, byte quality, byte pitchAndFamily, byte[] faceName)
		{
			byte[] record = new byte[24 + (faceName.Length + faceName.Length % 2)];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_CREATE_FONT_INDIRECT);
			pos = SetInt16(record, pos, height);
			pos = SetInt16(record, pos, width);
			pos = SetInt16(record, pos, escapement);
			pos = SetInt16(record, pos, orientation);
			pos = SetInt16(record, pos, weight);
			pos = SetByte(record, pos, italic ? (byte)0x01 : (byte)0x00);
			pos = SetByte(record, pos, underline ? (byte)0x01 : (byte)0x00);
			pos = SetByte(record, pos, strikeout ? (byte)0x01 : (byte)0x00);
			pos = SetByte(record, pos, charset);
			pos = SetByte(record, pos, outPrecision);
			pos = SetByte(record, pos, clipPrecision);
			pos = SetByte(record, pos, quality);
			pos = SetByte(record, pos, pitchAndFamily);
			pos = SetBytes(record, pos, faceName);
			if (faceName.Length % 2 == 1) pos = SetByte(record, pos, 0);
			records.Add(record);

			WmfFont font = new WmfFont((ushort)objects.Count(), height, width, escapement,
					orientation, weight, italic, underline, strikeout, charset, outPrecision,
					clipPrecision, quality, pitchAndFamily, faceName);
			objects.Add(font);
			return font;
		}

		public IGdiPalette CreatePalette(ushort version, int[] entries) 
		{
			byte[] record = new byte[10 + entries.Length * 4];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_CREATE_PALETTE);
			pos = SetUint16(record, pos, version);
			pos = SetUint16(record, pos, (ushort)entries.Length);
			for (int i = 0; i < entries.Length; i++) 
			{
				pos = SetInt32(record, pos, entries[i]);
			}
			records.Add(record);

			IGdiPalette palette = new WmfPalette((ushort)objects.Count(), version, entries);
			objects.Add(palette);
			return palette;
		}

		public IGdiPatternBrush CreatePatternBrush(byte[] image) 
		{
			byte[] record = new byte[6 + (image.Length + image.Length % 2)];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_CREATE_PATTERN_BRUSH);
			pos = SetBytes(record, pos, image);
			if (image.Length % 2 == 1) pos = SetByte(record, pos, 0);
			records.Add(record);

			IGdiPatternBrush brush = new WmfPatternBrush((ushort)objects.Count(), image);
			objects.Add(brush);
			return brush;
		}

		public IGdiPen CreatePenIndirect(ushort style, short width, int color) 
		{
			byte[] record = new byte[16];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_CREATE_PEN_INDIRECT);
			pos = SetUint16(record, pos, style);
			pos = SetInt16(record, pos, width);
			pos = SetInt16(record, pos, 0);
			pos = SetInt32(record, pos, color);
			records.Add(record);

			WmfPen pen = new WmfPen((ushort)objects.Count(), style, width, color);
			objects.Add(pen);
			return pen;
		}

		public IGdiRegion CreateRectRgn(short left, short top, short right, short bottom) 
		{
			byte[] record = new byte[14];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_CREATE_RECT_RGN);
			pos = SetInt16(record, pos, bottom);
			pos = SetInt16(record, pos, right);
			pos = SetInt16(record, pos, top);
			pos = SetInt16(record, pos, left);
			records.Add(record);

			WmfRectRegion rgn = new WmfRectRegion((ushort)objects.Count(), left, top, right, bottom);
			objects.Add(rgn);
			return rgn;
		}

		public void DeleteObject(IGdiObject obj) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_DELETE_OBJECT);
			pos = SetUint16(record, pos, ((WmfObject)obj).Id);
			records.Add(record);

			objects.Remove((WmfObject)obj);

			if (dc.Brush == obj)
				dc.Brush = defaultBrush;
			else if (dc.Font == obj)
				dc.Font = defaultFont;
			else if (dc.Pen == obj)
				dc.Pen = defaultPen;
		}

		public void DibBitBlt(byte[] image, short dx, short dy, short dw, short dh, short sx, short sy, uint rop) 
		{
			byte[] record = new byte[22 + (image.Length + image.Length % 2)];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_DIB_BIT_BLT);
			pos = SetUint32(record, pos, rop);
			pos = SetInt16(record, pos, sy);
			pos = SetInt16(record, pos, sx);
			pos = SetInt16(record, pos, dw);
			pos = SetInt16(record, pos, dh);
			pos = SetInt16(record, pos, dy);
			pos = SetInt16(record, pos, dx);
			pos = SetBytes(record, pos, image);
			if (image.Length % 2 == 1) pos = SetByte(record, pos, 0);
			records.Add(record);
		}

		public IGdiPatternBrush DibCreatePatternBrush(byte[] image, int usage) 
		{
			byte[] record = new byte[10 + (image.Length + image.Length % 2)];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_DIB_CREATE_PATTERN_BRUSH);
			pos = SetInt32(record, pos, usage);
			pos = SetBytes(record, pos, image);
			if (image.Length % 2 == 1) pos = SetByte(record, pos, 0);
			records.Add(record);

			// TODO usage
			IGdiPatternBrush brush = new WmfPatternBrush((ushort)objects.Count(), image);
			objects.Add(brush);
			return brush;
		}

		public void DibStretchBlt(byte[] image, short dx, short dy, short dw, short dh, short sx, short sy, short sw, short sh, uint rop) 
		{
			byte[] record = new byte[26 + (image.Length + image.Length % 2)];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_DIB_STRETCH_BLT);
			pos = SetUint32(record, pos, rop);
			pos = SetInt16(record, pos, sh);
			pos = SetInt16(record, pos, sw);
			pos = SetInt16(record, pos, sy);
			pos = SetInt16(record, pos, sx);
			pos = SetInt16(record, pos, dw);
			pos = SetInt16(record, pos, dh);
			pos = SetInt16(record, pos, dy);
			pos = SetInt16(record, pos, dx);
			pos = SetBytes(record, pos, image);
			if (image.Length % 2 == 1) pos = SetByte(record, pos, 0);
			records.Add(record);
		}

		public void Ellipse(short sx, short sy, short ex, short ey) 
		{
			byte[] record = new byte[14];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_ELLIPSE);
			pos = SetInt16(record, pos, ey);
			pos = SetInt16(record, pos, ex);
			pos = SetInt16(record, pos, sy);
			pos = SetInt16(record, pos, sx);
			records.Add(record);
		}

		public void Escape(byte[] data) 
		{
			byte[] record = new byte[10 + (data.Length + data.Length % 2)];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_ESCAPE);
			pos = SetBytes(record, pos, data);
			if (data.Length % 2 == 1) pos = SetByte(record, pos, 0);
			records.Add(record);
		}

		public int ExcludeClipRect(short left, short top, short right, short bottom) 
		{
			byte[] record = new byte[14];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_EXCLUDE_CLIP_RECT);
			pos = SetInt16(record, pos, bottom);
			pos = SetInt16(record, pos, right);
			pos = SetInt16(record, pos, top);
			pos = SetInt16(record, pos, left);
			records.Add(record);

			// TODO
			return Constants.COMPLEXREGION;
		}

		public void ExtFloodFill(short x, short y, int color, ushort type) 
		{
			byte[] record = new byte[16];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_EXT_FLOOD_FILL);
			pos = SetUint16(record, pos, type);
			pos = SetInt32(record, pos, color);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, x);
			records.Add(record);
		}

		public void ExtTextOut(short x, short y, ushort options, short[] rect, byte[] text, short[] dx)
		{
			if (rect != null && rect.Length != 4)
				throw new ArgumentException("rect must be 4 length.", nameof(rect));

			byte[] record = new byte[14 + ((rect != null) ? 8 : 0) + (text.Length + text.Length % 2) + (dx.Length * 2)];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_EXT_TEXT_OUT);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, x);
			pos = SetInt16(record, pos, (short)text.Length);
			pos = SetUint16(record, pos, options);
			if (rect != null)
			{
				pos = SetInt16(record, pos, rect[0]);
				pos = SetInt16(record, pos, rect[1]);
				pos = SetInt16(record, pos, rect[2]);
				pos = SetInt16(record, pos, rect[3]);
			}
			pos = SetBytes(record, pos, text);
			if (text.Length % 2 == 1) pos = SetByte(record, pos, 0);
			for (int i = 0; i < dx.Length; i++)
				pos = SetInt16(record, pos, dx[i]);
			records.Add(record);

			bool vertical = false;
			if (dc.Font != null)
			{
				if (dc.Font.FaceName.StartsWith("@"))
				{
					vertical = true;
				}
			}

			short align = dc.TextAlign;
			short width = 0;
			if (!vertical)
			{
				if (dc.Font != null)
					dx = GdiUtils.FixTextDx(dc.Font.Charset, text, dx);

				if (dx != null && dx.Length > 0)
				{
					for (int i = 0; i < dx.Length; i++)
						width += dx[i];

					short tx = x;

					if ((align & (Constants.TA_LEFT | Constants.TA_CENTER | Constants.TA_RIGHT)) == Constants.TA_RIGHT)
						tx -= (short)((width - dx[dx.Length - 1]));
					else if ((align & (Constants.TA_LEFT | Constants.TA_CENTER | Constants.TA_RIGHT)) == Constants.TA_CENTER)
						tx -= (short)((width - dx[dx.Length - 1]) / 2);

					for (int i = 0; i < dx.Length; i++)
						tx += dx[i];

					if ((align & (Constants.TA_NOUPDATECP | Constants.TA_UPDATECP)) == Constants.TA_UPDATECP)
						dc.MoveToEx(tx, y, null);
				}
			}
		}

		public void FillRgn(IGdiRegion rgn, IGdiBrush brush) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_FLOOD_FILL);
			pos = SetUint16(record, pos, ((WmfBrush)brush).Id);
			pos = SetUint16(record, pos, ((WmfRegion)rgn).Id);
			records.Add(record);
		}

		public void FloodFill(short x, short y, int color) 
		{
			byte[] record = new byte[16];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_FLOOD_FILL);
			pos = SetInt32(record, pos, color);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, x);
			records.Add(record);
		}

		public void FrameRgn(IGdiRegion rgn, IGdiBrush brush, short w, short h) 
		{
			byte[] record = new byte[14];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_FRAME_RGN);
			pos = SetInt16(record, pos, h);
			pos = SetInt16(record, pos, w);
			pos = SetUint16(record, pos, ((WmfBrush)brush).Id);
			pos = SetUint16(record, pos, ((WmfRegion)rgn).Id);
			records.Add(record);
		}

		public void IntersectClipRect(short left, short top, short right, short bottom) 
		{
			byte[] record = new byte[16];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_INTERSECT_CLIP_RECT);
			pos = SetInt16(record, pos, bottom);
			pos = SetInt16(record, pos, right);
			pos = SetInt16(record, pos, top);
			pos = SetInt16(record, pos, left);
			records.Add(record);
		}

		public void InvertRgn(IGdiRegion rgn) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_INVERT_RGN);
			pos = SetUint16(record, pos, ((WmfRegion)rgn).Id);
			records.Add(record);
		}

		public void LineTo(short ex, short ey) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_LINE_TO);
			pos = SetInt16(record, pos, ey);
			pos = SetInt16(record, pos, ex);
			records.Add(record);

			dc.MoveToEx(ex, ey, null);
		}

		public void MoveToEx(short x, short y, Point old) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_MOVE_TO_EX);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, x);
			records.Add(record);

			dc.MoveToEx(x, y, old);
		}

		public void OffsetClipRgn(short x, short y) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_OFFSET_CLIP_RGN);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, x);
			records.Add(record);
		}

		public void OffsetViewportOrgEx(short x, short y, Point point) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_OFFSET_VIEWPORT_ORG_EX);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, x);
			records.Add(record);

			dc.OffsetViewportOrgEx(x, y, point);
		}

		public void OffsetWindowOrgEx(short x, short y, Point point) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_OFFSET_WINDOW_ORG_EX);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, x);
			records.Add(record);

			dc.OffsetWindowOrgEx(x, y, point);
		}

		public void PaintRgn(IGdiRegion rgn) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_PAINT_RGN);
			pos = SetUint16(record, pos, ((WmfRegion)rgn).Id);
			records.Add(record);
		}

		public void PatBlt(short x, short y, short width, short height, uint rop) 
		{
			byte[] record = new byte[18];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_PAT_BLT);
			pos = SetUint32(record, pos, rop);
			pos = SetInt16(record, pos, height);
			pos = SetInt16(record, pos, width);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, x);
			records.Add(record);
		}

		public void Pie(short sx, short sy, short ex, short ey, short sxr, short syr, short exr, short eyr) 
		{
			byte[] record = new byte[22];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_PIE);
			pos = SetInt16(record, pos, eyr);
			pos = SetInt16(record, pos, exr);
			pos = SetInt16(record, pos, syr);
			pos = SetInt16(record, pos, sxr);
			pos = SetInt16(record, pos, ey);
			pos = SetInt16(record, pos, ex);
			pos = SetInt16(record, pos, sy);
			pos = SetInt16(record, pos, sx);
			records.Add(record);
		}

		public void Polygon(Point[] points) 
		{
			byte[] record = new byte[8 + points.Length * 4];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_POLYGON);
			pos = SetInt16(record, pos, (short)points.Length);
			for (int i = 0; i < points.Length; i++) 
			{
				pos = SetInt16(record, pos, points[i].X);
				pos = SetInt16(record, pos, points[i].Y);
			}
			records.Add(record);
		}

		public void Polyline(Point[] points) 
		{
			byte[] record = new byte[8 + points.Length * 4];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_POLYLINE);
			pos = SetInt16(record, pos, (short)points.Length);
			for (int i = 0; i < points.Length; i++) 
			{
				pos = SetInt16(record, pos, points[i].X);
				pos = SetInt16(record, pos, points[i].Y);
			}
			records.Add(record);
		}

		public void PolyPolygon(Point[][] points) 
		{
			int length = 8;
			for (int i = 0; i < points.Length; i++) 
				length += 2 + points[i].Length * 4;
			byte[] record = new byte[length];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_POLYLINE);
			pos = SetInt16(record, pos, (short)points.Length);
			for (int i = 0; i < points.Length; i++) 
				pos = SetInt16(record, pos, (short)points[i].Length);
			for (int i = 0; i < points.Length; i++) 
			{
				for (int j = 0; j < points[i].Length; j++) 
				{
					pos = SetInt16(record, pos, points[i][j].X);
					pos = SetInt16(record, pos, points[i][j].Y);
				}
			}
			records.Add(record);
		}

		public void RealizePalette() 
		{
			byte[] record = new byte[6];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_REALIZE_PALETTE);
			records.Add(record);
		}

		public void RestoreDC(short savedDC) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_RESTORE_DC);
			pos = SetInt16(record, pos, savedDC);
			records.Add(record);
		}

		public void Rectangle(short sx, short sy, short ex, short ey) 
		{
			byte[] record = new byte[14];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_RECTANGLE);
			pos = SetInt16(record, pos, ey);
			pos = SetInt16(record, pos, ex);
			pos = SetInt16(record, pos, sy);
			pos = SetInt16(record, pos, sx);
			records.Add(record);
		}

		public void ResizePalette(IGdiPalette palette) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_REALIZE_PALETTE);
			pos = SetUint16(record, pos, ((WmfPalette)palette).Id);
			records.Add(record);
		}

		public void RoundRect(short sx, short sy, short ex, short ey, short rw, short rh) 
		{
			byte[] record = new byte[18];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_ROUND_RECT);
			pos = SetInt16(record, pos, rh);
			pos = SetInt16(record, pos, rw);
			pos = SetInt16(record, pos, ey);
			pos = SetInt16(record, pos, ex);
			pos = SetInt16(record, pos, sy);
			pos = SetInt16(record, pos, sx);
			records.Add(record);
		}

		public void SaveDC() 
		{
			byte[] record = new byte[6];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SAVE_DC);
			records.Add(record);
		}

		public void ScaleViewportExtEx(short x, short xd, short y, short yd, Size old) 
		{
			byte[] record = new byte[14];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SCALE_VIEWPORT_EXT_EX);
			pos = SetInt16(record, pos, yd);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, xd);
			pos = SetInt16(record, pos, x);
			records.Add(record);

			dc.ScaleViewportExtEx(x, xd, y, yd, old);
		}

		public void ScaleWindowExtEx(short x, short xd, short y, short yd, Size old) 
		{
			byte[] record = new byte[14];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SCALE_WINDOW_EXT_EX);
			pos = SetInt16(record, pos, yd);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, xd);
			pos = SetInt16(record, pos, x);
			records.Add(record);

			dc.ScaleWindowExtEx(x, xd, y, yd, old);
		}

		public void SelectClipRgn(IGdiRegion rgn) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SELECT_CLIP_RGN);
			pos = SetUint16(record, pos, ((WmfRegion)rgn).Id);
			records.Add(record);
		}

		public void SelectObject(IGdiObject obj) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SELECT_OBJECT);
			pos = SetUint16(record, pos, ((WmfObject)obj).Id);
			records.Add(record);

			if (obj is WmfBrush)
				dc.Brush = (WmfBrush)obj;
			else if (obj is WmfFont)
				dc.Font = (WmfFont)obj;
			else if (obj is WmfPen)
				dc.Pen = (WmfPen)obj;
		}

		public void SelectPalette(IGdiPalette palette, bool mode) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SELECT_PALETTE);
			pos = SetInt16(record, pos, mode ? (short)1 : (short)0);
			pos = SetUint16(record, pos, ((WmfPalette)palette).Id);
			records.Add(record);
		}

		public void SetBkColor(int color) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_BK_COLOR);
			pos = SetInt32(record, pos, color);
			records.Add(record);
		}

		public void SetBkMode(short mode) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_BK_MODE);
			pos = SetInt16(record, pos, mode);
			records.Add(record);
		}

		public void SetDIBitsToDevice(short dx, short dy, short dw, short dh, short sx, short sy, ushort startscan, ushort scanlines, byte[] image, ushort colorUse)
		{
			byte[] record = new byte[24 + (image.Length + image.Length % 2)];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_DIBITS_TO_DEVICE);
			pos = SetUint16(record, pos, colorUse);
			pos = SetUint16(record, pos, scanlines);
			pos = SetUint16(record, pos, startscan);
			pos = SetInt16(record, pos, sy);
			pos = SetInt16(record, pos, sx);
			pos = SetInt16(record, pos, dw);
			pos = SetInt16(record, pos, dh);
			pos = SetInt16(record, pos, dy);
			pos = SetInt16(record, pos, dx);
			pos = SetBytes(record, pos, image);
			if (image.Length % 2 == 1) pos = SetByte(record, pos, 0);
			records.Add(record);
		}

		public void SetLayout(uint layout) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_LAYOUT);
			pos = SetUint32(record, pos, layout);
			records.Add(record);
		}

		public void SetMapMode(short mode) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_MAP_MODE);
			pos = SetInt16(record, pos, mode);
			records.Add(record);
		}

		public void SetMapperFlags(uint flags) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_MAPPER_FLAGS);
			pos = SetUint32(record, pos, flags);
			records.Add(record);
		}

		public void SetPaletteEntries(IGdiPalette palette, ushort startIndex, int[] entries) 
		{
			byte[] record = new byte[6 + entries.Length * 4];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_PALETTE_ENTRIES);
			pos = SetUint16(record, pos, ((WmfPalette)palette).Id);
			pos = SetUint16(record, pos, (ushort)entries.Length);
			pos = SetUint16(record, pos, startIndex);
			for (int i = 0; i < entries.Length; i++) 
				pos = SetInt32(record, pos, entries[i]);
			records.Add(record);
		}

		public void SetPixel(short x, short y, int color) 
		{
			byte[] record = new byte[14];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_PIXEL);
			pos = SetInt32(record, pos, color);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, x);
			records.Add(record);
		}

		public void SetPolyFillMode(short mode) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_POLY_FILL_MODE);
			pos = SetInt16(record, pos, mode);
			records.Add(record);
		}

		public void SetRelAbs(short mode) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_REL_ABS);
			pos = SetInt16(record, pos, mode);
			records.Add(record);
		}

		public void SetROP2(short mode) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_ROP2);
			pos = SetInt16(record, pos, mode);
			records.Add(record);
		}

		public void SetStretchBltMode(short mode) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_STRETCH_BLT_MODE);
			pos = SetInt16(record, pos, mode);
			records.Add(record);
		}

		public void SetTextAlign(short align) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_TEXT_ALIGN);
			pos = SetInt16(record, pos, align);
			records.Add(record);

			dc.TextAlign = align;
		}

		public void SetTextCharacterExtra(short extra) 
		{
			byte[] record = new byte[8];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_TEXT_CHARACTER_EXTRA);
			pos = SetInt16(record, pos, extra);
			records.Add(record);
		}

		public void SetTextColor(int color) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_TEXT_COLOR);
			pos = SetInt32(record, pos, color);
			records.Add(record);
		}

		public void SetTextJustification(short breakExtra, short breakCount) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_TEXT_COLOR);
			pos = SetInt16(record, pos, breakCount);
			pos = SetInt16(record, pos, breakExtra);
			records.Add(record);
		}

		public void SetViewportExtEx(short x, short y, Size old) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_VIEWPORT_EXT_EX);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, x);
			records.Add(record);
		}

		public void SetViewportOrgEx(short x, short y, Point old) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_VIEWPORT_ORG_EX);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, x);
			records.Add(record);
		}

		public void SetWindowExtEx(short width, short height, Size old) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_WINDOW_EXT_EX);
			pos = SetInt16(record, pos, height);
			pos = SetInt16(record, pos, width);
			records.Add(record);
		}

		public void SetWindowOrgEx(short x, short y, Point old) 
		{
			byte[] record = new byte[10];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_SET_WINDOW_ORG_EX);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, x);
			records.Add(record);
		}

		public void StretchBlt(byte[] image, short dx, short dy, short dw, short dh, short sx, short sy, short sw, short sh, uint rop) 
		{
			byte[] record = new byte[26 + (image.Length + image.Length % 2)];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_STRETCH_BLT);
			pos = SetUint32(record, pos, rop);
			pos = SetInt16(record, pos, sh);
			pos = SetInt16(record, pos, sw);
			pos = SetInt16(record, pos, sy);
			pos = SetInt16(record, pos, sx);
			pos = SetInt16(record, pos, dw);
			pos = SetInt16(record, pos, dh);
			pos = SetInt16(record, pos, dy);
			pos = SetInt16(record, pos, dx);
			pos = SetBytes(record, pos, image);
			if (image.Length % 2 == 1) pos = SetByte(record, pos, 0);
			records.Add(record);
		}

		public void StretchDIBits(short dx, short dy, short dw, short dh, short sx, short sy, short sw, short sh, byte[] image, ushort usage, uint rop) 
		{
			byte[] record = new byte[26 + (image.Length + image.Length % 2)];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_STRETCH_DIBITS);
			pos = SetUint32(record, pos, rop);
			pos = SetUint16(record, pos, usage);
			pos = SetInt16(record, pos, sh);
			pos = SetInt16(record, pos, sw);
			pos = SetInt16(record, pos, sy);
			pos = SetInt16(record, pos, sx);
			pos = SetInt16(record, pos, dw);
			pos = SetInt16(record, pos, dh);
			pos = SetInt16(record, pos, dy);
			pos = SetInt16(record, pos, dx);
			pos = SetBytes(record, pos, image);
			if (image.Length % 2 == 1) pos = SetByte(record, pos, 0);
			records.Add(record);
		}

		public void TextOut(short x, short y, byte[] text)
		{
			byte[] record = new byte[10 + text.Length + text.Length % 2];
			int pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, WmfConstants.RECORD_TEXT_OUT);
			pos = SetInt16(record, pos, (short)text.Length);
			pos = SetBytes(record, pos, text);
			if (text.Length % 2 == 1) pos = SetByte(record, pos, 0);
			pos = SetInt16(record, pos, y);
			pos = SetInt16(record, pos, x);
			records.Add(record);
		}

		public void Footer() 
		{
			int pos = 0;
			if (header != null) 
			{
				int size = header.Length;
				int maxRecordSize = 0;
				foreach(var record2 in records)
				{
					size += record2.Length;
					if (record2.Length > maxRecordSize) maxRecordSize = record2.Length;
				}

				pos = SetUint32(header, 6, size / 2);
				pos = SetUint16(header, pos, (ushort)(objects.Count()));
				pos = SetUint32(header, pos, maxRecordSize / 2);
			}

			byte[] record = new byte[6];
			pos = 0;
			pos = SetUint32(record, pos, record.Length / 2);
			pos = SetUint16(record, pos, 0x0000);
			records.Add(record);
		}

		private int SetByte(byte[] record, int pos, byte value)
		{
			record[pos] = (byte)(0xFF & value);
			return pos + 1;
		}

		private int SetBytes(byte[] record, int pos, byte[] data)
		{
			Array.Copy(data, 0, record, pos, data.Length);
			return pos + data.Length;
		}

		private int SetInt16(byte[] record, int pos, short value)
			=> SetBytes(record, pos, BitConverter.GetBytes(value));

		private int SetUint16(byte[] record, int pos, ushort value)
			=> SetBytes(record, pos, BitConverter.GetBytes(value));

		private int SetInt32(byte[] record, int pos, int value)
			=> SetBytes(record, pos, BitConverter.GetBytes(value));

		private int SetUint32(byte[] record, int pos, uint value)
			=> SetBytes(record, pos, BitConverter.GetBytes(value));

		private int SetUint32(byte[] record, int pos, int value)
			=> SetBytes(record, pos, BitConverter.GetBytes((uint)value));
	}
}
