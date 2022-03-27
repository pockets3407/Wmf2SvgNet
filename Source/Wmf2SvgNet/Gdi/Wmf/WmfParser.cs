/*
 * Copyright 2007-2015 Hidekatsu Izuno
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
using System.IO;

namespace Wmf2SvgNet.Gdi.Wmf
{

	public class WmfParser
	{

		public WmfParser() { }

		private IGdiObject[] objs;

		public void Parse(Stream inputStream, IGdi gdi)
		{
			IO.DataInput input = null;
			bool isEmpty = true;

			try
			{
				input = new IO.DataInput(inputStream, true);

				ushort mtType = 0;
				ushort mtHeaderSize = 0;

				uint key = input.ReadUint32();
				isEmpty = false;
				if (key == 0x9AC6CDD7)
				{
					input.ReadInt16(); // hmf
					short vsx = input.ReadInt16();
					short vsy = input.ReadInt16();
					short vex = input.ReadInt16();
					short vey = input.ReadInt16();
					ushort dpi = input.ReadUint16();
					input.ReadUint32(); // reserved
					input.ReadUint16(); // checksum

					gdi.PlaceableHeader(vsx, vsy, vex, vey, dpi);

					mtType = input.ReadUint16();
					mtHeaderSize = input.ReadUint16();
				}
				else
				{
					mtType = (ushort)(key & 0x0000FFFF);
					mtHeaderSize = (ushort)((key & 0xFFFF0000) >> 16);
				}

				input.ReadUint16(); // mtVersion
				input.ReadUint32(); // mtSize
				ushort mtNoObjects = input.ReadUint16();
				input.ReadUint32(); // mtMaxRecord
				input.ReadUint16(); // mtNoParameters

				if (mtType != 1 || mtHeaderSize != 9)
					throw new WmfParseException("invalid file format.");

				gdi.Header();

				objs = new IGdiObject[mtNoObjects];

				while (true)
				{
					uint size = input.ReadUint32() - 3;
					ushort recordType = input.ReadUint16();

					if (recordType == WmfConstants.RECORD_EOF)
						break; // Last record

					input.SetCount(0);

					switch (recordType)
					{
						case WmfConstants.RECORD_REALIZE_PALETTE:
							{
								gdi.RealizePalette();
								break;
							}
						case WmfConstants.RECORD_SET_PALETTE_ENTRIES:
							{
								int[] entries = new int[input.ReadUint16()];
								ushort startIndex = input.ReadUint16();
								ushort objID = input.ReadUint16();
								for (int i = 0; i < entries.Length; i++)
									entries[i] = input.ReadInt32();
								gdi.SetPaletteEntries((IGdiPalette)objs[objID], startIndex, entries);
								break;
							}
						case WmfConstants.RECORD_SET_BK_MODE:
							{
								short mode = input.ReadInt16();
								gdi.SetBkMode(mode);
								break;
							}
						case WmfConstants.RECORD_SET_MAP_MODE:
							{
								short mode = input.ReadInt16();
								gdi.SetMapMode(mode);
								break;
							}
						case WmfConstants.RECORD_SET_ROP2:
							{
								short mode = input.ReadInt16();
								gdi.SetROP2(mode);
								break;
							}
						case WmfConstants.RECORD_SET_REL_ABS:
							{
								short mode = input.ReadInt16();
								gdi.SetRelAbs(mode);
								break;
							}
						case WmfConstants.RECORD_SET_POLY_FILL_MODE:
							{
								short mode = input.ReadInt16();
								gdi.SetPolyFillMode(mode);
								break;
							}
						case WmfConstants.RECORD_SET_STRETCH_BLT_MODE:
							{
								short mode = input.ReadInt16();
								gdi.SetStretchBltMode(mode);
								break;
							}
						case WmfConstants.RECORD_SET_TEXT_CHARACTER_EXTRA:
							{
								short extra = input.ReadInt16();
								gdi.SetTextCharacterExtra(extra);
								break;
							}
						case WmfConstants.RECORD_RESTORE_DC:
							{
								short dc = input.ReadInt16();
								gdi.RestoreDC(dc);
								break;
							}
						case WmfConstants.RECORD_RESIZE_PALETTE:
							{
								ushort objID = input.ReadUint16();
								gdi.ResizePalette((IGdiPalette)objs[objID]);
								break;
							}
						case WmfConstants.RECORD_DIB_CREATE_PATTERN_BRUSH:
							{
								int usage = input.ReadInt32();
								byte[] image = input.ReadBytes((int)size * 2 - input.GetCount());

								for (int i = 0; i < objs.Length; i++)
								{
									if (objs[i] == null)
									{
										objs[i] = gdi.DibCreatePatternBrush(image, usage);
										break;
									}
								}
								break;
							}
						case WmfConstants.RECORD_SET_LAYOUT:
							{
								uint layout = input.ReadUint32();
								gdi.SetLayout(layout);
								break;
							}
						case WmfConstants.RECORD_SET_BK_COLOR:
							{
								int color = input.ReadInt32();
								gdi.SetBkColor(color);
								break;
							}
						case WmfConstants.RECORD_SET_TEXT_COLOR:
							{
								int color = input.ReadInt32();
								gdi.SetTextColor(color);
								break;
							}
						case WmfConstants.RECORD_OFFSET_VIEWPORT_ORG_EX:
							{
								short y = input.ReadInt16();
								short x = input.ReadInt16();
								gdi.OffsetViewportOrgEx(x, y, null);
								break;
							}
						case WmfConstants.RECORD_LINE_TO:
							{
								short ey = input.ReadInt16();
								short ex = input.ReadInt16();
								gdi.LineTo(ex, ey);
								break;
							}
						case WmfConstants.RECORD_MOVE_TO_EX:
							{
								short y = input.ReadInt16();
								short x = input.ReadInt16();
								gdi.MoveToEx(x, y, null);
								break;
							}
						case WmfConstants.RECORD_OFFSET_CLIP_RGN:
							{
								short y = input.ReadInt16();
								short x = input.ReadInt16();
								gdi.OffsetClipRgn(x, y);
								break;
							}
						case WmfConstants.RECORD_FILL_RGN:
							{
								ushort brushID = input.ReadUint16();
								ushort rgnID = input.ReadUint16();
								gdi.FillRgn((IGdiRegion)objs[rgnID], (IGdiBrush)objs[brushID]);
								break;
							}
						case WmfConstants.RECORD_SET_MAPPER_FLAGS:
							{
								uint flag = input.ReadUint32();
								gdi.SetMapperFlags(flag);
								break;
							}
						case WmfConstants.RECORD_SELECT_PALETTE:
							{
								bool mode = (input.ReadInt16() != 0);
								if ((size * 2 - input.GetCount()) > 0)
								{
									ushort objID = input.ReadUint16();
									gdi.SelectPalette((IGdiPalette)objs[objID], mode);
								}
								break;
							}
						case WmfConstants.RECORD_POLYGON:
							{
								Point[] points = new Point[input.ReadInt16()];
								for (int i = 0; i < points.Length; i++)
									points[i] = new Point(input.ReadInt16(), input.ReadInt16());
								gdi.Polygon(points);
								break;
							}
						case WmfConstants.RECORD_POLYLINE:
							{
								Point[] points = new Point[input.ReadInt16()];
								for (int i = 0; i < points.Length; i++)
									points[i] = new Point(input.ReadInt16(), input.ReadInt16());
								gdi.Polyline(points);
								break;
							}
						case WmfConstants.RECORD_SET_TEXT_JUSTIFICATION:
							{
								short breakCount = input.ReadInt16();
								short breakExtra = input.ReadInt16();
								gdi.SetTextJustification(breakExtra, breakCount);
								break;
							}
						case WmfConstants.RECORD_SET_WINDOW_ORG_EX:
							{
								short y = input.ReadInt16();
								short x = input.ReadInt16();
								gdi.SetWindowOrgEx(x, y, null);
								break;
							}
						case WmfConstants.RECORD_SET_WINDOW_EXT_EX:
							{
								short height = input.ReadInt16();
								short width = input.ReadInt16();
								gdi.SetWindowExtEx(width, height, null);
								break;
							}
						case WmfConstants.RECORD_SET_VIEWPORT_ORG_EX:
							{
								short y = input.ReadInt16();
								short x = input.ReadInt16();
								gdi.SetViewportOrgEx(x, y, null);
								break;
							}
						case WmfConstants.RECORD_SET_VIEWPORT_EXT_EX:
							{
								short y = input.ReadInt16();
								short x = input.ReadInt16();
								gdi.SetViewportExtEx(x, y, null);
								break;
							}
						case WmfConstants.RECORD_OFFSET_WINDOW_ORG_EX:
							{
								short y = input.ReadInt16();
								short x = input.ReadInt16();
								gdi.OffsetWindowOrgEx(x, y, null);
								break;
							}
						case WmfConstants.RECORD_SCALE_WINDOW_EXT_EX:
							{
								short yd = input.ReadInt16();
								short y = input.ReadInt16();
								short xd = input.ReadInt16();
								short x = input.ReadInt16();
								gdi.ScaleWindowExtEx(x, xd, y, yd, null);
								break;
							}
						case WmfConstants.RECORD_SCALE_VIEWPORT_EXT_EX:
							{
								short yd = input.ReadInt16();
								short y = input.ReadInt16();
								short xd = input.ReadInt16();
								short x = input.ReadInt16();
								gdi.ScaleViewportExtEx(x, xd, y, yd, null);
								break;
							}
						case WmfConstants.RECORD_EXCLUDE_CLIP_RECT:
							{
								short ey = input.ReadInt16();
								short ex = input.ReadInt16();
								short sy = input.ReadInt16();
								short sx = input.ReadInt16();
								gdi.ExcludeClipRect(sx, sy, ex, ey);
								break;
							}
						case WmfConstants.RECORD_INTERSECT_CLIP_RECT:
							{
								short ey = input.ReadInt16();
								short ex = input.ReadInt16();
								short sy = input.ReadInt16();
								short sx = input.ReadInt16();
								gdi.IntersectClipRect(sx, sy, ex, ey);
								break;
							}
						case WmfConstants.RECORD_ELLIPSE:
							{
								short ey = input.ReadInt16();
								short ex = input.ReadInt16();
								short sy = input.ReadInt16();
								short sx = input.ReadInt16();
								gdi.Ellipse(sx, sy, ex, ey);
								break;
							}
						case WmfConstants.RECORD_FLOOD_FILL:
							{
								int color = input.ReadInt32();
								short y = input.ReadInt16();
								short x = input.ReadInt16();
								gdi.FloodFill(x, y, color);
								break;
							}
						case WmfConstants.RECORD_FRAME_RGN:
							{
								short height = input.ReadInt16();
								short width = input.ReadInt16();
								ushort brushID = input.ReadUint16();
								ushort rgnID = input.ReadUint16();
								gdi.FrameRgn((IGdiRegion)objs[rgnID], (IGdiBrush)objs[brushID], width, height);
								break;
							}
						case WmfConstants.RECORD_ANIMATE_PALETTE:
							{
								int[] entries = new int[input.ReadUint16()];
								ushort startIndex = input.ReadUint16();
								ushort objID = input.ReadUint16();
								for (int i = 0; i < entries.Length; i++)
									entries[i] = input.ReadInt32();
								gdi.AnimatePalette((IGdiPalette)objs[objID], startIndex, entries);
								break;
							}
						case WmfConstants.RECORD_TEXT_OUT:
							{
								short count = input.ReadInt16();
								byte[] text = input.ReadBytes(count);
								if (count % 2 == 1)
									input.ReadByte();
								short y = input.ReadInt16();
								short x = input.ReadInt16();
								gdi.TextOut(x, y, text);
								break;
							}
						case WmfConstants.RECORD_POLY_POLYGON:
							{
								Point[][] points = new Point[input.ReadInt16()][];
								for (int i = 0; i < points.Length; i++)
								{
									points[i] = new Point[input.ReadInt16()];
								}
								for (int i = 0; i < points.Length; i++)
								{
									for (int j = 0; j < points[i].Length; j++)
									{
										points[i][j] = new Point(input.ReadInt16(), input.ReadInt16());
									}
								}
								gdi.PolyPolygon(points);
								break;
							}
						case WmfConstants.RECORD_EXT_FLOOD_FILL:
							{
								ushort type = input.ReadUint16();
								int color = input.ReadInt32();
								short y = input.ReadInt16();
								short x = input.ReadInt16();
								gdi.ExtFloodFill(x, y, color, type);
								break;
							}
						case WmfConstants.RECORD_RECTANGLE:
							{
								short ey = input.ReadInt16();
								short ex = input.ReadInt16();
								short sy = input.ReadInt16();
								short sx = input.ReadInt16();
								gdi.Rectangle(sx, sy, ex, ey);
								break;
							}
						case WmfConstants.RECORD_SET_PIXEL:
							{
								int color = input.ReadInt32();
								short y = input.ReadInt16();
								short x = input.ReadInt16();
								gdi.SetPixel(x, y, color);
								break;
							}
						case WmfConstants.RECORD_ROUND_RECT:
							{
								short rh = input.ReadInt16();
								short rw = input.ReadInt16();
								short ey = input.ReadInt16();
								short ex = input.ReadInt16();
								short sy = input.ReadInt16();
								short sx = input.ReadInt16();
								gdi.RoundRect(sx, sy, ex, ey, rw, rh);
								break;
							}
						case WmfConstants.RECORD_PAT_BLT:
							{
								uint rop = input.ReadUint32();
								short height = input.ReadInt16();
								short width = input.ReadInt16();
								short y = input.ReadInt16();
								short x = input.ReadInt16();
								gdi.PatBlt(x, y, width, height, rop);
								break;
							}
						case WmfConstants.RECORD_SAVE_DC:
							{
								gdi.SaveDC();
								break;
							}
						case WmfConstants.RECORD_PIE:
							{
								short eyr = input.ReadInt16();
								short exr = input.ReadInt16();
								short syr = input.ReadInt16();
								short sxr = input.ReadInt16();
								short ey = input.ReadInt16();
								short ex = input.ReadInt16();
								short sy = input.ReadInt16();
								short sx = input.ReadInt16();
								gdi.Pie(sx, sy, ex, ey, sxr, syr, exr, eyr);
								break;
							}
						case WmfConstants.RECORD_STRETCH_BLT:
							{
								uint rop = input.ReadUint32();
								short sh = input.ReadInt16();
								short sw = input.ReadInt16();
								short sy = input.ReadInt16();
								short sx = input.ReadInt16();
								short dh = input.ReadInt16();
								short dw = input.ReadInt16();
								short dy = input.ReadInt16();
								short dx = input.ReadInt16();

								byte[] image = input.ReadBytes((int)size * 2 - input.GetCount());

								gdi.StretchBlt(image, dx, dy, dw, dh, sx, sy, sw, sh, rop);
								break;
							}
						case WmfConstants.RECORD_ESCAPE:
							{
								byte[] data = input.ReadBytes(2 * (int)size);
								gdi.Escape(data);
								break;
							}
						case WmfConstants.RECORD_INVERT_RGN:
							{
								ushort rgnID = input.ReadUint16();
								gdi.InvertRgn((IGdiRegion)objs[rgnID]);
								break;
							}
						case WmfConstants.RECORD_PAINT_RGN:
							{
								ushort objID = input.ReadUint16();
								gdi.PaintRgn((IGdiRegion)objs[objID]);
								break;
							}
						case WmfConstants.RECORD_SELECT_CLIP_RGN:
							{
								ushort objID = input.ReadUint16();
								IGdiRegion rgn = (objID > 0) ? (IGdiRegion)objs[objID] : null;
								gdi.SelectClipRgn(rgn);
								break;
							}
						case WmfConstants.RECORD_SELECT_OBJECT:
							{
								ushort objID = input.ReadUint16();
								gdi.SelectObject(objs[objID]);
								break;
							}
						case WmfConstants.RECORD_SET_TEXT_ALIGN:
							{
								short align = input.ReadInt16();
								gdi.SetTextAlign(align);
								break;
							}
						case WmfConstants.RECORD_ARC:
							{
								short eya = input.ReadInt16();
								short exa = input.ReadInt16();
								short sya = input.ReadInt16();
								short sxa = input.ReadInt16();
								short eyr = input.ReadInt16();
								short exr = input.ReadInt16();
								short syr = input.ReadInt16();
								short sxr = input.ReadInt16();
								gdi.Arc(sxr, syr, exr, eyr, sxa, sya, exa, eya);
								break;
							}
						case WmfConstants.RECORD_CHORD:
							{
								short eya = input.ReadInt16();
								short exa = input.ReadInt16();
								short sya = input.ReadInt16();
								short sxa = input.ReadInt16();
								short eyr = input.ReadInt16();
								short exr = input.ReadInt16();
								short syr = input.ReadInt16();
								short sxr = input.ReadInt16();
								gdi.Chord(sxr, syr, exr, eyr, sxa, sya, exa, eya);
								break;
							}
						case WmfConstants.RECORD_BIT_BLT:
							{
								uint rop = input.ReadUint32();
								short sy = input.ReadInt16();
								short sx = input.ReadInt16();
								short height = input.ReadInt16();
								short width = input.ReadInt16();
								short dy = input.ReadInt16();
								short dx = input.ReadInt16();

								byte[] image = input.ReadBytes((int)size * 2 - input.GetCount());

								gdi.BitBlt(image, dx, dy, width, height, sx, sy, rop);
								break;
							}
						case WmfConstants.RECORD_EXT_TEXT_OUT:
							{
								uint rsize = size;

								short y = input.ReadInt16();
								short x = input.ReadInt16();
								short count = input.ReadInt16();
								ushort options = input.ReadUint16();
								rsize -= 4;

								short[] rect = null;
								if ((options & 0x0006) > 0)
								{
									rect = new short[] { input.ReadInt16(), input.ReadInt16(), input.ReadInt16(), input.ReadInt16() };
									rsize -= 4;
								}
								byte[] text = input.ReadBytes(count);
								if (count % 2 == 1)
								{
									input.ReadByte();
								}
								rsize -= (uint)((count + 1) / 2);

								short[] dx = null;
								if (rsize > 0)
								{
									dx = new short[rsize];
									for (int i = 0; i < dx.Length; i++)
										dx[i] = input.ReadInt16();
								}
								gdi.ExtTextOut(x, y, options, rect, text, dx);
								break;
							}
						case WmfConstants.RECORD_SET_DIBITS_TO_DEVICE:
							{
								ushort colorUse = input.ReadUint16();
								ushort scanlines = input.ReadUint16();
								ushort startscan = input.ReadUint16();
								short sy = input.ReadInt16();
								short sx = input.ReadInt16();
								short dh = input.ReadInt16();
								short dw = input.ReadInt16();
								short dy = input.ReadInt16();
								short dx = input.ReadInt16();

								byte[] image = input.ReadBytes((int)size * 2 - input.GetCount());

								gdi.SetDIBitsToDevice(dx, dy, dw, dh, sx, sy, startscan, scanlines, image, colorUse);
								break;
							}
						case WmfConstants.RECORD_DIB_BIT_BLT:
							{
								bool isRop = false;

								uint rop = input.ReadUint32();
								short sy = input.ReadInt16();
								short sx = input.ReadInt16();
								short height = input.ReadInt16();
								if (height == 0)
								{
									height = input.ReadInt16();
									isRop = true;
								}
								short width = input.ReadInt16();
								short dy = input.ReadInt16();
								short dx = input.ReadInt16();

								if (isRop)
								{
									gdi.DibBitBlt(null, dx, dy, width, height, sx, sy, rop);
								}
								else
								{
									byte[] image = input.ReadBytes((int)size * 2 - input.GetCount());

									gdi.DibBitBlt(image, dx, dy, width, height, sx, sy, rop);
								}
								break;
							}
						case WmfConstants.RECORD_DIB_STRETCH_BLT:
							{
								uint rop = input.ReadUint32();
								short sh = input.ReadInt16();
								short sw = input.ReadInt16();
								short sx = input.ReadInt16();
								short sy = input.ReadInt16();
								short dh = input.ReadInt16();
								short dw = input.ReadInt16();
								short dy = input.ReadInt16();
								short dx = input.ReadInt16();

								byte[] image = input.ReadBytes((int)size * 2 - input.GetCount());

								gdi.DibStretchBlt(image, dx, dy, dw, dh, sx, sy, sw, sh, rop);
								break;
							}
						case WmfConstants.RECORD_STRETCH_DIBITS:
							{
								uint rop = input.ReadUint32();
								ushort usage = input.ReadUint16();
								short sh = input.ReadInt16();
								short sw = input.ReadInt16();
								short sy = input.ReadInt16();
								short sx = input.ReadInt16();
								short dh = input.ReadInt16();
								short dw = input.ReadInt16();
								short dy = input.ReadInt16();
								short dx = input.ReadInt16();

								byte[] image = input.ReadBytes((int)size * 2 - input.GetCount());

								gdi.StretchDIBits(dx, dy, dw, dh, sx, sy, sw, sh, image, usage, rop);
								break;
							}
						case WmfConstants.RECORD_DELETE_OBJECT:
							{
								ushort objID = input.ReadUint16();
								gdi.DeleteObject(objs[objID]);
								objs[objID] = null;
								break;
							}
						case WmfConstants.RECORD_CREATE_PALETTE:
							{
								ushort version = input.ReadUint16();
								int[] entries = new int[input.ReadUint16()];
								for (int i = 0; i < entries.Length; i++)
									entries[i] = input.ReadInt32();
								AddToAvailableIndex(gdi.CreatePalette(version, entries));
								break;
							}
						case WmfConstants.RECORD_CREATE_PATTERN_BRUSH:
							{
								byte[] image = input.ReadBytes((int)size * 2 - input.GetCount());
								AddToAvailableIndex(gdi.CreatePatternBrush(image));
								break;
							}
						case WmfConstants.RECORD_CREATE_PEN_INDIRECT:
							{
								ushort style = input.ReadUint16();
								short width = input.ReadInt16();
								input.ReadInt16();
								int color = input.ReadInt32();
								AddToAvailableIndex(gdi.CreatePenIndirect(style, width, color));
								break;
							}
						case WmfConstants.RECORD_CREATE_FONT_INDIRECT:
							{
								short height = input.ReadInt16();
								short width = input.ReadInt16();
								short escapement = input.ReadInt16();
								short orientation = input.ReadInt16();
								short weight = input.ReadInt16();
								bool italic = (input.ReadByte() == 1);
								bool underline = (input.ReadByte() == 1);
								bool strikeout = (input.ReadByte() == 1);
								byte charset = input.ReadByte();
								byte outPrecision = input.ReadByte();
								byte clipPrecision = input.ReadByte();
								byte quality = input.ReadByte();
								byte pitchAndFamily = input.ReadByte();
								byte[] faceName = input.ReadBytes((int)size * 2 - input.GetCount());

								IGdiObject obj = gdi.CreateFontIndirect(height, width, escapement, orientation, weight, italic,
										underline, strikeout, charset, outPrecision, clipPrecision, quality, pitchAndFamily,
										faceName);

								AddToAvailableIndex(obj);
								break;
							}
						case WmfConstants.RECORD_CREATE_BRUSH_INDIRECT:
							{
								ushort style = input.ReadUint16();
								int color = input.ReadInt32();
								ushort hatch = input.ReadUint16();
								AddToAvailableIndex(gdi.CreateBrushIndirect(style, color, hatch));
								break;
							}
						case WmfConstants.RECORD_CREATE_RECT_RGN:
							{
								short ey = input.ReadInt16();
								short ex = input.ReadInt16();
								short sy = input.ReadInt16();
								short sx = input.ReadInt16();
								AddToAvailableIndex(gdi.CreateRectRgn(sx, sy, ex, ey));
								break;
							}
						default:
                            Console.WriteLine($"unsuppored id find: {recordType} (size={size})");
							break;
					}

					int rest = (int)size * 2 - input.GetCount();
					if (rest > 0)
						input.ReadBytes(rest);
				}
				input.Close();

				gdi.Footer();
			}
			catch (EndOfStreamException e)
			{
				if (isEmpty) throw new WmfParseException("input file size is zero.");
			}
			finally
            {
				objs = null;
            }
		}

		private void AddToAvailableIndex(IGdiObject gdiObject)
        {
			for (int i = 0; i < objs.Length; i++)
			{
				if (objs[i] == null)
				{
					objs[i] = gdiObject;
					break;
				}
			}
		}
	}
}