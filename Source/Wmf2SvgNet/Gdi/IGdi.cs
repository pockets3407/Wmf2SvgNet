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
namespace Wmf2SvgNet.Gdi
{
    public interface IGdi
    {

        void PlaceableHeader(short vsx, short vsy, short vex, short vey, ushort dpi);
        void Header();
        void AnimatePalette(IGdiPalette palette, int startIndex, int[] entries);
        void Arc(short sxr, short syr, short exr, short eyr, short sxa, short sya, short exa, short eya);
        void BitBlt(byte[] image, short dx, short dy, short dw, short dh, short sx, short sy, uint rop);
        void Chord(short sxr, short syr, short exr, short eyr, short sxa, short sya, short exa, short eya);
        IGdiBrush CreateBrushIndirect(ushort style, int color, ushort hatch);
        IGdiFont CreateFontIndirect(short height, short width, short escapement,
                          short orientation, short weight,
                          bool italic, bool underline, bool strikeout,
                          byte charset, byte outPrecision, byte clipPrecision,
                          byte quality, byte pitchAndFamily, byte[] faceName);
        IGdiPalette CreatePalette(ushort version, int[] palEntry);
        IGdiPatternBrush CreatePatternBrush(byte[] image);
        IGdiPen CreatePenIndirect(ushort style, short width, int color);
        IGdiRegion CreateRectRgn(short left, short top, short right, short bottom);
        void DeleteObject(IGdiObject obj);
        void DibBitBlt(byte[] image, short dx, short dy, short dw, short dh, short sx, short sy, uint rop);
        IGdiPatternBrush DibCreatePatternBrush(byte[] image, int usage);
        void DibStretchBlt(byte[] image, short dx, short dy, short dw, short dh, short sx, short sy, short sw, short sh, uint rop);
        void Ellipse(short sx, short sy, short ex, short ey);
        void Escape(byte[] data);
        int ExcludeClipRect(short left, short top, short right, short bottom);
        void ExtFloodFill(short x, short y, int color, ushort type);
        void ExtTextOut(short x, short y, ushort options, short[] rect, byte[] text, short[] lpdx);
        void FillRgn(IGdiRegion rgn, IGdiBrush brush);
        void FloodFill(short x, short y, int color);
        void FrameRgn(IGdiRegion rgn, IGdiBrush brush, short w, short h);
        void IntersectClipRect(short left, short top, short right, short bottom);
        void InvertRgn(IGdiRegion rgn);
        void LineTo(short ex, short ey);
        void MoveToEx(short x, short y, Point old);
        void OffsetClipRgn(short x, short y);
        void OffsetViewportOrgEx(short x, short y, Point point);
        void OffsetWindowOrgEx(short x, short y, Point point);
        void PaintRgn(IGdiRegion rgn);
        void PatBlt(short x, short y, short width, short height, uint rop);
        void Pie(short sx, short sy, short ex, short ey, short sxr, short syr, short exr, short eyr);
        void Polygon(Point[] points);
        void Polyline(Point[] points);
        void PolyPolygon(Point[][] points);
        void RealizePalette();
        void RestoreDC(short savedDC);
        void Rectangle(short sx, short sy, short ex, short ey);
        void ResizePalette(IGdiPalette palette);
        void RoundRect(short sx, short sy, short ex, short ey, short rw, short rh);
        void SaveDC();
        void ScaleViewportExtEx(short x, short xd, short y, short yd, Size old);
        void ScaleWindowExtEx(short x, short xd, short y, short yd, Size old);
        void SelectClipRgn(IGdiRegion rgn);
        void SelectObject(IGdiObject obj);
        void SelectPalette(IGdiPalette palette, bool mode);
        void SetBkColor(int color);
        void SetBkMode(short mode);
        void SetDIBitsToDevice(short dx, short dy, short dw, short dh, short sx, short sy, ushort startscan, ushort scanlines, byte[] image, ushort colorUse);
        void SetLayout(uint layout);
        void SetMapMode(short mode);
        void SetMapperFlags(uint flags);
        void SetPaletteEntries(IGdiPalette palette, ushort startIndex, int[] entries);
        void SetPixel(short x, short y, int color);
        void SetPolyFillMode(short mode);
        void SetRelAbs(short mode);
        void SetROP2(short mode);
        void SetStretchBltMode(short mode);
        void SetTextAlign(short align);
        void SetTextCharacterExtra(short extra);
        void SetTextColor(int color);
        void SetTextJustification(short breakExtra, short breakCount);
        void SetViewportExtEx(short x, short y, Size old);
        void SetViewportOrgEx(short x, short y, Point old);
        void SetWindowExtEx(short width, short height, Size old);
        void SetWindowOrgEx(short x, short y, Point old);
        void StretchBlt(byte[] image, short dx, short dy, short dw, short dh, short sx, short sy, short sw, short sh, uint rop);
        void StretchDIBits(short dx, short dy, short dw, short dh, short sx, short sy, short sw, short sh, byte[] image, ushort usage, uint rop);
        void TextOut(short x, short y, byte[] text);
        void Footer();
    }
}