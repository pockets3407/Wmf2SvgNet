/*
 * Copyright 2007-2012 Hidekatsu Izuno, Shunsuke Mori
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
 * 
 * NOTICE:
 * This file has changed from the original file.
 * Change:
 *     Added option to use style inside element, instead of css. 
 *     DevExpress's SVG engine does do not support composite class definitions.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Wmf2SvgNet.Gdi.Svg
{

    public class SvgGdi : IGdi
    {
        private readonly XNamespace svg = "http://www.w3.org/2000/svg";
        private readonly XNamespace xlink = "http://www.w3.org/1999/xlink";
        private readonly XNamespace xml = "http://www.w3.org/XML/1998/namespace";

        private bool compatible;
        private readonly bool useStyle;

        private Dictionary<string, string> props = null;
        private SvgDc dc;
        private LinkedList<SvgDc> saveDC = new LinkedList<SvgDc>();
        private XDocument doc = null;
        private XElement parentNode = null;
        private XElement styleNode = null;
        private XElement defsNode = null;

        private int brushNo = 0;
        private int fontNo = 0;
        private int penNo = 0;
        private int patternNo = 0;
        private int rgnNo = 0;
        private int clipPathNo = 0;
        private int maskNo = 0;

        private Dictionary<IGdiObject, string> nameMap = new Dictionary<IGdiObject, string>();
        private SvgBrush defaultBrush;
        private SvgPen defaultPen;
        private SvgFont defaultFont;

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="SvgGdiException"/>
        public SvgGdi() : this(false, true) { }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="SvgGdiException"/>
        public SvgGdi(bool compatible, bool useStyle)
        {
            this.compatible = compatible;
            this.useStyle = useStyle;
            doc = new XDocument(new XElement(svg + "svg"));
            props = SvgGdiProperties.GetDefaults();
        }

        public void Write(Stream outputStream)
        {
            doc.Save(outputStream, SaveOptions.None);
            outputStream.Flush();
        }

        public void SetCompatible(bool flag)
        {
            compatible = flag;
        }

        public bool IsCompatible { get => compatible; }

        public bool ReplaceSymbolFont { get; set; } = false;

        public SvgDc DC { get => dc; }

        public string GetProperty(string key)
            => props.TryGetValue(key, out string value) ? value : "";

        public XDocument Document { get => doc; }

        public XElement DefsElement { get => defsNode; }

        public XElement StyleElement { get => styleNode; }

        public void PlaceableHeader(short wsx, short wsy, short wex, short wey, ushort dpi)
        {
            if (parentNode == null)
            {
                Init();
            }

            dc.SetWindowExtEx((short)Math.Abs(wex - wsx), (short)Math.Abs(wey - wsy), null);
            dc.Dpi = dpi;

            var width = (Math.Abs(wex - wsx) / (double)dc.Dpi) + "in";
            var height = (Math.Abs(wey - wsy) / (double)dc.Dpi) + "in";


            XAttribute widthAttribute = doc.Root.Attribute("width");
            if (widthAttribute == null)
            {
                doc.Root.Add(new XAttribute("width", width));
            }
            else
            {
                widthAttribute.Value = width;
            }

            XAttribute heightAttribute = doc.Root.Attribute("height");
            if (heightAttribute == null)
            {
                doc.Root.Add(new XAttribute("height", height));
            }
            else
            {
                widthAttribute.Value = height;
            }
        }

        public void Header()
        {
            if (parentNode == null)
            {
                Init();
            }
        }

        private void Init()
        {
            dc = new SvgDc(this);

            XElement root = doc.Root;

            defsNode = new XElement(svg + "defs");
            root.Add(defsNode);

            if (useStyle)
            {
                styleNode = new XElement(svg + "style", new XAttribute("type", "text/css"));
                root.Add(styleNode);
            }

            parentNode = new XElement(svg + "g");
            doc.Root.Add(parentNode);

            defaultBrush = (SvgBrush)CreateBrushIndirect(Constants.BS_SOLID, 0x00FFFFFF, 0);
            defaultPen = (SvgPen)CreatePenIndirect(Constants.PS_SOLID, 1, 0x00000000);
            defaultFont = null;

            dc.Brush = defaultBrush;
            dc.Pen = defaultPen;
            dc.Font = defaultFont;
        }

        public void AnimatePalette(IGdiPalette palette, int startIndex, int[] entries)
        {
            // TODO
            Console.WriteLine("not implemented: animatePalette");
        }

        public void Arc(short sxr, short syr, short exr, short eyr, short sxa, short sya, short exa, short eya)
        {
            double rx = Math.Abs(exr - sxr) / 2.0;
            double ry = Math.Abs(eyr - syr) / 2.0;
            if (rx <= 0 || ry <= 0)
            {
                return;
            }

            double cx = Math.Min(sxr, exr) + rx;
            double cy = Math.Min(syr, eyr) + ry;

            XElement elem = null;
            if (sxa == exa && sya == eya)
            {
                if (rx == ry)
                {
                    elem = new XElement(svg + "circle",
                        new XAttribute("cx", dc.ToAbsoluteX(cx)),
                        new XAttribute("cy", dc.ToAbsoluteY(cy)),
                        new XAttribute("r", dc.ToRelativeX(rx))
                    );
                }
                else
                {
                    elem = new XElement(svg + "ellipse",
                        new XAttribute("cx", dc.ToAbsoluteX(cx)),
                        new XAttribute("cy", dc.ToAbsoluteY(cy)),
                        new XAttribute("rx", dc.ToRelativeX(rx)),
                        new XAttribute("ry", dc.ToRelativeY(ry))
                    );
                }
            }
            else
            {
                double sa = Math.Atan2((sya - cy) * rx, (sxa - cx) * ry);
                double sx = rx * Math.Cos(sa);
                double sy = ry * Math.Sin(sa);

                double ea = Math.Atan2((eya - cy) * rx, (exa - cx) * ry);
                double ex = rx * Math.Cos(ea);
                double ey = ry * Math.Sin(ea);

                double a = Math.Atan2((ex - sx) * (-sy) - (ey - sy) * (-sx), (ex - sx) * (-sx) + (ey - sy) * (-sy));

                elem = new XElement(svg + "path",
                    new XAttribute("d", "M " + dc.ToAbsoluteX(sx + cx) + "," + dc.ToAbsoluteY(sy + cy)
                        + " A " + dc.ToRelativeX(rx) + "," + dc.ToRelativeY(ry)
                        + " 0 " + (a > 0 ? "1" : "0") + " 0"
                        + " " + dc.ToAbsoluteX(ex + cx) + "," + dc.ToAbsoluteY(ey + cy)));
            }

            if (dc.Pen != null)
            {
                if (useStyle)
                {
                    elem.Add(new XAttribute("class", GetClassString(dc.Pen)));
                }
                else
                {
                    elem.Add(new XAttribute("style", GetStyle(dc.Pen)));
                }
            }

            elem.Add(new XAttribute("fill", "none"));
            parentNode.Add(elem);
        }

        public void BitBlt(byte[] image, short dx, short dy, short dw, short dh, short sx, short sy, uint rop)
        {
            BmpToSvg(image, dx, dy, dw, dh, sx, sy, dw, dh, Constants.DIB_RGB_COLORS, rop);
        }

        public void Chord(short sxr, short syr, short exr, short eyr, short sxa, short sya, short exa, short eya)
        {
            double rx = Math.Abs(exr - sxr) / 2.0;
            double ry = Math.Abs(eyr - syr) / 2.0;
            if (rx <= 0 || ry <= 0)
            {
                return;
            }

            double cx = Math.Min(sxr, exr) + rx;
            double cy = Math.Min(syr, eyr) + ry;

            XElement elem = null;
            if (sxa == exa && sya == eya)
            {
                if (rx == ry)
                {
                    elem = new XElement(svg + "circle",
                        new XAttribute("cx", dc.ToAbsoluteX(cx)),
                        new XAttribute("cy", dc.ToAbsoluteY(cy)),
                        new XAttribute("r", dc.ToRelativeX(rx))
                    );
                }
                else
                {
                    elem = new XElement(svg + "ellipse",
                        new XAttribute("cx", dc.ToAbsoluteX(cx)),
                        new XAttribute("cy", dc.ToAbsoluteY(cy)),
                        new XAttribute("rx", dc.ToRelativeX(rx)),
                        new XAttribute("ry", dc.ToRelativeY(ry))
                    );
                }
            }
            else
            {
                double sa = Math.Atan2((sya - cy) * rx, (sxa - cx) * ry);
                double sx = rx * Math.Cos(sa);
                double sy = ry * Math.Sin(sa);

                double ea = Math.Atan2((eya - cy) * rx, (exa - cx) * ry);
                double ex = rx * Math.Cos(ea);
                double ey = ry * Math.Sin(ea);

                double a = Math.Atan2((ex - sx) * (-sy) - (ey - sy) * (-sx), (ex - sx) * (-sx) + (ey - sy) * (-sy));

                elem = new XElement(svg + "path",
                    new XAttribute("d", "M " + dc.ToAbsoluteX(sx + cx) + "," + dc.ToAbsoluteY(sy + cy)
                        + " A " + dc.ToRelativeX(rx) + "," + dc.ToRelativeY(ry)
                        + " 0 " + (a > 0 ? "1" : "0") + " 0"
                        + " " + dc.ToAbsoluteX(ex + cx) + "," + dc.ToAbsoluteY(ey + cy) + " Z"));
            }

            if (dc.Pen != null || dc.Brush != null)
            {
                if (useStyle)
                {
                    elem.Add(new XAttribute("class", GetClassString(dc.Pen, dc.Brush)));
                }
                else
                {
                    elem.Add(new XAttribute("style", GetStyle(dc.Pen, dc.Brush)));
                }

                if (dc.Brush != null && dc.Brush.Style == Constants.BS_HATCHED)
                {
                    string id = "pattern" + (patternNo++);
                    elem.Add(new XAttribute("fill", "url(#" + id + ")"));
                    defsNode.Add(dc.Brush.CreateFillPattern(id));
                }
            }

            parentNode.Add(elem);
        }

        public IGdiBrush CreateBrushIndirect(ushort style, int color, ushort hatch)
        {
            SvgBrush brush = new SvgBrush(this, style, color, hatch);
            if (!nameMap.ContainsKey(brush))
            {
                string name = "brush" + (brushNo++);
                nameMap.Add(brush, name);
                if (useStyle)
                {
                    styleNode.Add(brush.CreateTextNode(name));
                }
            }
            return brush;
        }

        public IGdiFont CreateFontIndirect(short height, short width, short escapement, short orientation, short weight, bool italic, bool underline, bool strikeout, byte charset, byte outPrecision, byte clipPrecision, byte quality, byte pitchAndFamily, byte[] faceName)
        {
            SvgFont font = new SvgFont(this, height, width, escapement,
                    orientation, weight, italic, underline, strikeout, charset,
                    outPrecision, clipPrecision, quality, pitchAndFamily, faceName);
            if (!nameMap.ContainsKey(font))
            {
                string name = "font" + (fontNo++);
                nameMap.Add(font, name);
                if (useStyle)
                {
                    styleNode.Add(font.CreateTextNode(name));
                }
            }
            return font;
        }

        public IGdiPalette CreatePalette(ushort version, int[] entries)
        {
            return new SvgPalette(this, version, entries);
        }

        public IGdiPatternBrush CreatePatternBrush(byte[] image)
        {
            return new SvgPatternBrush(this, image);
        }

        public IGdiPen CreatePenIndirect(ushort style, short width, int color)
        {
            SvgPen pen = new SvgPen(this, style, width, color);
            if (!nameMap.ContainsKey(pen))
            {
                string name = "pen" + (penNo++);
                nameMap.Add(pen, name);
                if (useStyle)
                {
                    styleNode.Add(pen.CreateTextNode(name));
                }
            }
            return pen;
        }

        public IGdiRegion CreateRectRgn(short left, short top, short right, short bottom)
        {
            SvgRectRegion rgn = new SvgRectRegion(this, left, top, right, bottom);
            if (!nameMap.ContainsKey(rgn))
            {
                nameMap.Add(rgn, "rgn" + (rgnNo++));
                defsNode.Add(rgn.CreateElement());
            }
            return rgn;
        }

        public void DeleteObject(IGdiObject obj)
        {
            if (dc.Brush == obj)
            {
                dc.Brush = defaultBrush;
            }
            else if (dc.Font == obj)
            {
                dc.Font = defaultFont;
            }
            else if (dc.Pen == obj)
            {
                dc.Pen = defaultPen;
            }
        }

        public void DibBitBlt(byte[] image, short dx, short dy, short dw, short dh, short sx, short sy, uint rop)
        {
            BitBlt(image, dx, dy, dw, dh, sx, sy, rop);
        }

        public IGdiPatternBrush DibCreatePatternBrush(byte[] image, int usage)
        {
            // TODO usage
            return new SvgPatternBrush(this, image);
        }

        public void DibStretchBlt(byte[] image, short dx, short dy, short dw, short dh, short sx, short sy, short sw, short sh, uint rop)
        {
            this.StretchDIBits(dx, dy, dw, dh, sx, sy, sw, sh, image, Constants.DIB_RGB_COLORS, rop);
        }

        public void Ellipse(short sx, short sy, short ex, short ey)
        {
            XElement elem = new XElement(svg + "ellipse");

            if (dc.Pen != null || dc.Brush != null)
            {
                if (useStyle)
                {
                    elem.Add(new XAttribute("class", GetClassString(dc.Pen, dc.Brush)));
                }
                else
                {
                    elem.Add(new XAttribute("style", GetStyle(dc.Pen, dc.Brush)));
                }

                if (dc.Brush != null && dc.Brush.Style == Constants.BS_HATCHED)
                {
                    string id = "pattern" + (patternNo++);
                    elem.Add(new XAttribute("fill", "url(#" + id + ")"));
                    defsNode.Add(dc.Brush.CreateFillPattern(id));
                }
            }

            elem.Add(new XAttribute("cx", (int)dc.ToAbsoluteX((sx + ex) / 2)));
            elem.Add(new XAttribute("cy", (int)dc.ToAbsoluteY((sy + ey) / 2)));
            elem.Add(new XAttribute("rx", (int)dc.ToRelativeX((ex - sx) / 2)));
            elem.Add(new XAttribute("ry", (int)dc.ToRelativeY((ey - sy) / 2)));
            parentNode.Add(elem);
        }

        public void Escape(byte[] data)
        {
        }

        public int ExcludeClipRect(short left, short top, short right, short bottom)
        {
            XElement mask = dc.Mask;
            if (mask != null)
            {
                mask = new XElement(mask);
                string name = "mask" + (maskNo++);
                mask.Add(new XAttribute("id", name));
                defsNode.Add(mask);

                XElement unclip = new XElement(svg + "rect",
                    new XAttribute("x", (int)dc.ToAbsoluteX(left)),
                    new XAttribute("y", (int)dc.ToAbsoluteY(top)),
                    new XAttribute("width", (int)dc.ToRelativeX(right - left)),
                    new XAttribute("height", (int)dc.ToRelativeY(bottom - top)),
                    new XAttribute("fill", "black")
                );
                mask.Add(unclip);
                dc.Mask = mask;

                // TODO
                return Constants.COMPLEXREGION;
            }
            else
            {
                return Constants.NULLREGION;
            }
        }

        public void ExtFloodFill(short x, short y, int color, ushort type)
        {
            // TODO
            Console.WriteLine("not implemented: extFloodFill");
        }

        public void ExtTextOut(short x, short y, ushort options, short[] rect, byte[] text, short[] dx)
        {
            XElement elem = new XElement(svg + "text");

            int escapement = 0;
            bool vertical = false;
            if (dc.Font != null)
            {
                if (useStyle)
                {
                    elem.Add(new XAttribute("class", GetClassString(dc.Font)));
                }
                else
                {
                    elem.Add(new XAttribute("style", GetStyle(dc.Font)));
                }

                if (dc.Font.FaceName.StartsWith("@"))
                {
                    vertical = true;
                    escapement = dc.Font.Escapement - 2700;
                }
                else
                {
                    escapement = dc.Font.Escapement;
                }
            }
            elem.Add(new XAttribute("fill", SvgObject.ToColor(dc.TextColor)));

            // style
            StringBuilder styleTag = new StringBuilder();
            short align = dc.TextAlign;

            if ((align & (Constants.TA_LEFT | Constants.TA_CENTER | Constants.TA_RIGHT)) == Constants.TA_RIGHT)
            {
                styleTag.Append("text-anchor: end; ");
            }
            else if ((align & (Constants.TA_LEFT | Constants.TA_CENTER | Constants.TA_RIGHT)) == Constants.TA_CENTER)
            {
                styleTag.Append("text-anchor: middle; ");
            }

            if (compatible)
            {
                styleTag.Append("dominant-baseline: alphabetic; ");
            }
            else
            {
                if (vertical)
                {
                    elem.Add(new XAttribute("writing-mode", "tb"));
                }
                else
                {
                    if ((align & (Constants.TA_BOTTOM | Constants.TA_TOP | Constants.TA_BASELINE)) == Constants.TA_BASELINE)
                    {
                        styleTag.Append("dominant-baseline: alphabetic; ");
                    }
                    else
                    {
                        styleTag.Append("dominant-baseline: text-before-edge; ");
                    }
                }
            }

            if ((align & Constants.TA_RTLREADING) == Constants.TA_RTLREADING || (options & Constants.ETO_RTLREADING) > 0)
            {
                styleTag.Append("unicode-bidi: bidi-override; direction: rtl; ");
            }

            if (dc.TextSpace > 0)
            {
                styleTag.Append("word-spacing: ").Append(dc.TextSpace).Append("; ");
            }

            if (styleTag.Length > 0)
            {
                elem.Add(new XAttribute("style", styleTag.ToString()));
            }

            elem.Add(new XAttribute("stroke", "none"));

            if ((align & (Constants.TA_NOUPDATECP | Constants.TA_UPDATECP)) == Constants.TA_UPDATECP)
            {
                x = dc.CurrentX;
                y = dc.CurrentY;
            }

            // x
            int ax = (int)dc.ToAbsoluteX(x);
            short width = 0;
            if (vertical)
            {
                elem.Add(new XAttribute("x", ax));
                if (dc.Font != null)
                {
                    width = Math.Abs(dc.Font.FontSize);
                }
            }
            else
            {
                if (dc.Font != null)
                {
                    dx = GdiUtils.FixTextDx(dc.Font.Charset, text, dx);
                }

                if (dx != null && dx.Length > 0)
                {
                    for (int i = 0; i < dx.Length; i++)
                    {
                        width += dx[i];
                    }

                    short tx = x;

                    if ((align & (Constants.TA_LEFT | Constants.TA_CENTER | Constants.TA_RIGHT)) == Constants.TA_RIGHT)
                    {
                        tx -= (short)(width - dx[dx.Length - 1]);
                    }
                    else if ((align & (Constants.TA_LEFT | Constants.TA_CENTER | Constants.TA_RIGHT)) == Constants.TA_CENTER)
                    {
                        tx -= (short)((width - dx[dx.Length - 1]) / 2);
                    }

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < dx.Length; i++)
                    {
                        if (i > 0)
                        {
                            sb.Append(" ");
                        }

                        sb.Append((int)dc.ToAbsoluteX(tx));
                        tx += dx[i];
                    }
                    if ((align & (Constants.TA_NOUPDATECP | Constants.TA_UPDATECP)) == Constants.TA_UPDATECP)
                    {
                        dc.MoveToEx(tx, y, null);
                    }
                    elem.Add(new XAttribute("x", sb.ToString()));
                }
                else
                {
                    if (dc.Font != null)
                    {
                        width = (short)(Math.Abs(dc.Font.FontSize * text.Length) / 2);
                    }

                    elem.Add(new XAttribute("x", ax));
                }
            }

            // y
            int ay = (int)dc.ToAbsoluteY(y);
            short height = 0;
            if (vertical)
            {
                if (dc.Font != null)
                {
                    dx = GdiUtils.FixTextDx(dc.Font.Charset, text, dx);
                }

                StringBuilder sb = new StringBuilder();
                if (align == 0)
                {
                    sb.Append(ay + (int)dc.ToRelativeY(Math.Abs(dc.Font.Height)));
                }
                else
                {
                    sb.Append(ay);
                }

                if (dx != null && dx.Length > 0)
                {
                    for (int i = 0; i < dx.Length - 1; i++)
                    {
                        height += dx[i];
                    }

                    short ty = y;

                    if ((align & (Constants.TA_LEFT | Constants.TA_CENTER | Constants.TA_RIGHT)) == Constants.TA_RIGHT)
                    {
                        ty -= (short)(height - dx[dx.Length - 1]);
                    }
                    else if ((align & (Constants.TA_LEFT | Constants.TA_CENTER | Constants.TA_RIGHT)) == Constants.TA_CENTER)
                    {
                        ty -= (short)((height - dx[dx.Length - 1]) / 2);
                    }

                    for (int i = 0; i < dx.Length; i++)
                    {
                        sb.Append(" ");
                        sb.Append((int)dc.ToAbsoluteY(ty));
                        ty += dx[i];
                    }

                    if ((align & (Constants.TA_NOUPDATECP | Constants.TA_UPDATECP)) == Constants.TA_UPDATECP)
                    {
                        dc.MoveToEx(x, ty, null);
                    }
                }
                else
                {
                    if (dc.Font != null)
                    {
                        height = (short)(Math.Abs(dc.Font.FontSize * text.Length) / 2);
                    }
                }
                elem.Add(new XAttribute("y", sb.ToString()));
            }
            else
            {
                if (dc.Font != null)
                {
                    height = Math.Abs(dc.Font.FontSize);
                }

                if (compatible)
                {
                    if ((align & (Constants.TA_BOTTOM | Constants.TA_TOP | Constants.TA_BASELINE)) == Constants.TA_TOP)
                    {
                        elem.Add(new XAttribute("y", ay + (int)dc.ToRelativeY(height * 0.88)));
                    }
                    else if ((align & (Constants.TA_BOTTOM | Constants.TA_TOP | Constants.TA_BASELINE)) == Constants.TA_BOTTOM)
                    {
                        elem.Add(new XAttribute("y", ay + rect[3] - rect[1] + (int)dc.ToRelativeY(height * 0.88)));
                    }
                    else
                    {
                        elem.Add(new XAttribute("y", ay));
                    }
                }
                else
                {
                    if ((align & (Constants.TA_BOTTOM | Constants.TA_TOP | Constants.TA_BASELINE)) == Constants.TA_BOTTOM && rect != null)
                    {
                        elem.Add(new XAttribute("y", ay + rect[3] - rect[1] - (int)dc.ToRelativeY(height)));
                    }
                    else
                    {
                        elem.Add(new XAttribute("y", ay));
                    }
                }
            }

            XElement bk = null;
            if (dc.BkMode == Constants.OPAQUE || (options & Constants.ETO_OPAQUE) > 0)
            {
                if (rect == null && dc.Font != null)
                {
                    rect = new short[4];
                    if (vertical)
                    {
                        if ((align & (Constants.TA_BOTTOM | Constants.TA_TOP | Constants.TA_BASELINE)) == Constants.TA_BOTTOM)
                        {
                            rect[0] = (short)(x - width);
                        }
                        else if ((align & (Constants.TA_BOTTOM | Constants.TA_TOP | Constants.TA_BASELINE)) == Constants.TA_BASELINE)
                        {
                            rect[0] = (short)(x - (int)(width * 0.85));
                        }
                        else
                        {
                            rect[0] = x;
                        }

                        if ((align & (Constants.TA_LEFT | Constants.TA_RIGHT | Constants.TA_CENTER)) == Constants.TA_RIGHT)
                        {
                            rect[1] = (short)(y - height);
                        }
                        else if ((align & (Constants.TA_LEFT | Constants.TA_RIGHT | Constants.TA_CENTER)) == Constants.TA_CENTER)
                        {
                            rect[1] = (short)(y - height / 2);
                        }
                        else
                        {
                            rect[1] = y;
                        }
                    }
                    else
                    {
                        if ((align & (Constants.TA_LEFT | Constants.TA_RIGHT | Constants.TA_CENTER)) == Constants.TA_RIGHT)
                        {
                            rect[0] = (short)(x - width);
                        }
                        else if ((align & (Constants.TA_LEFT | Constants.TA_RIGHT | Constants.TA_CENTER)) == Constants.TA_CENTER)
                        {
                            rect[0] = (short)(x - width / 2);
                        }
                        else
                        {
                            rect[0] = x;
                        }

                        if ((align & (Constants.TA_BOTTOM | Constants.TA_TOP | Constants.TA_BASELINE)) == Constants.TA_BOTTOM)
                        {
                            rect[1] = (short)(y - height);
                        }
                        else if ((align & (Constants.TA_BOTTOM | Constants.TA_TOP | Constants.TA_BASELINE)) == Constants.TA_BASELINE)
                        {
                            rect[1] = (short)(y - (int)(height * 0.85));
                        }
                        else
                        {
                            rect[1] = y;
                        }
                    }
                    rect[2] = (short)(rect[0] + width);
                    rect[3] = (short)(rect[1] + height);
                }
                bk = new XElement(svg + "rect",
                    new XAttribute("x", (int)dc.ToAbsoluteX(rect[0])),
                    new XAttribute("y", (int)dc.ToAbsoluteY(rect[1])),
                    new XAttribute("width", (int)dc.ToRelativeX(rect[2] - rect[0])),
                    new XAttribute("height", (int)dc.ToRelativeY(rect[3] - rect[1])),
                    new XAttribute("fill", SvgObject.ToColor(dc.BkColor))
                );
            }

            XElement clip = null;
            if ((options & Constants.ETO_CLIPPED) > 0)
            {
                string name = "clipPath" + (clipPathNo++);
                clip = new XElement(svg + "clipPath", new XAttribute("id", name));

                XElement clipRect = new XElement(svg + "rect",
                    new XAttribute("x", (int)dc.ToAbsoluteX(rect[0])),
                    new XAttribute("y", (int)dc.ToAbsoluteY(rect[1])),
                    new XAttribute("width", (int)dc.ToRelativeX(rect[2] - rect[0])),
                    new XAttribute("height", (int)dc.ToRelativeY(rect[3] - rect[1]))
                );

                clip.Add(clipRect);
                elem.Add(new XAttribute("clip-path", "url(#" + name + ")"));
            }

            string str = null;
            if (dc.Font != null)
            {
                str = GdiUtils.ConvertString(text, dc.Font.Charset);
            }
            else
            {
                str = GdiUtils.ConvertString(text, Constants.DEFAULT_CHARSET);
            }

            if (dc.Font != null && dc.Font.Lang != null)
            {
                elem.Add(new XAttribute(xml + "lang", dc.Font.Lang));
            }

            elem.Add(new XAttribute(xml + "space", "preserve"));
            AppendText(elem, str);

            if (bk != null || clip != null)
            {
                XElement g = new XElement(svg + "g", bk, clip);
                g.Add(elem);
                elem = g;
            }

            if (escapement != 0)
            {
                elem.Add(new XAttribute("transform", "rotate(" + (-escapement / 10.0) + ", " + ax + ", " + ay + ")"));
            }

            parentNode.Add(elem);
        }

        public void FillRgn(IGdiRegion rgn, IGdiBrush brush)
        {
            if (rgn == null)
            {
                return;
            }

            XElement elem = new XElement(svg + "use",
                new XAttribute("xlink:href", "url(#" + nameMap[rgn] + ")"));
            if (useStyle)
            {
                elem.Add(new XAttribute("class", GetClassString(brush)));
            }
            else
            {
                elem.Add(new XAttribute("style", GetStyle(brush)));
            }

            SvgBrush sbrush = (SvgBrush)brush;
            if (sbrush.Style == Constants.BS_HATCHED)
            {
                string id = "pattern" + (patternNo++);
                elem.Add(new XAttribute("fill", "url(#" + id + ")"));
                defsNode.Add(sbrush.CreateFillPattern(id));
            }
            parentNode.Add(elem);
        }

        public void FloodFill(short x, short y, int color)
        {
            // TODO
            Console.WriteLine("not implemented: floodFill");
        }

        public void FrameRgn(IGdiRegion rgn, IGdiBrush brush, short width, short height)
        {
            // TODO
            Console.WriteLine("not implemented: frameRgn");
        }

        public void IntersectClipRect(short left, short top, short right, short bottom)
        {
            // TODO
            Console.WriteLine("not implemented: intersectClipRect");
        }

        public void InvertRgn(IGdiRegion rgn)
        {
            if (rgn == null)
            {
                return;
            }

            string ropFilter = dc.GetRopFilter(Constants.DSTINVERT);

            XElement elem = new XElement(svg + "use",
                new XAttribute(xlink + "href", "url(#" + nameMap[rgn] + ")"),
                ropFilter != null ? new XAttribute("filter", ropFilter) : null
            );

            parentNode.Add(elem);
        }

        public void LineTo(short ex, short ey)
        {
            XElement elem = new XElement(svg + "line",
                new XAttribute("fill", "none"),
                new XAttribute("x1", (int)dc.ToAbsoluteX(dc.CurrentX)),
                new XAttribute("y1", (int)dc.ToAbsoluteY(dc.CurrentY)),
                new XAttribute("x2", (int)dc.ToAbsoluteX(ex)),
                new XAttribute("y2", (int)dc.ToAbsoluteY(ey))
            );

            if (dc.Pen != null)
            {
                if (useStyle)
                {
                    elem.Add(new XAttribute("class", GetClassString(dc.Pen)));
                }
                else
                {
                    elem.Add(new XAttribute("style", GetStyle(dc.Pen)));
                }
            }

            parentNode.Add(elem);

            dc.MoveToEx(ex, ey, null);
        }

        public void MoveToEx(short x, short y, Point old)
        {
            dc.MoveToEx(x, y, old);
        }

        public void OffsetClipRgn(short x, short y)
        {
            dc.OffsetClipRgn(x, y);
            XElement mask = dc.Mask;
            if (mask != null)
            {
                mask = new XElement(mask);
                string name = "mask" + (maskNo++);
                mask.Add(new XAttribute("id", name));
                if (dc.OffsetClipX != 0 || dc.OffsetClipY != 0)
                {
                    mask.Add(new XAttribute("transform", "translate(" + dc.OffsetClipX + "," + dc.OffsetClipY + ")"));
                }

                defsNode.Add(mask);

                if (!parentNode.HasElements)
                {
                    parentNode.Remove();
                }

                parentNode = new XElement(svg + "g", new XAttribute("mask", name));
                doc.Root.Add(parentNode);

                dc.Mask = mask;
            }
        }

        public void OffsetViewportOrgEx(short x, short y, Point point)
        {
            dc.OffsetViewportOrgEx(x, y, point);
        }

        public void OffsetWindowOrgEx(short x, short y, Point point)
        {
            dc.OffsetWindowOrgEx(x, y, point);
        }

        public void PaintRgn(IGdiRegion rgn)
        {
            FillRgn(rgn, dc.Brush);
        }

        public void PatBlt(short x, short y, short width, short height, uint rop)
        {
            // TODO
            Console.WriteLine("not implemented: patBlt");
        }

        public void Pie(short sxr, short syr, short exr, short eyr, short sxa, short sya, short exa, short eya)
        {
            double rx = Math.Abs(exr - sxr) / 2.0;
            double ry = Math.Abs(eyr - syr) / 2.0;
            if (rx <= 0 || ry <= 0)
            {
                return;
            }

            double cx = Math.Min(sxr, exr) + rx;
            double cy = Math.Min(syr, eyr) + ry;

            XElement elem = null;
            if (sxa == exa && sya == eya)
            {
                if (rx == ry)
                {
                    elem = new XElement(svg + "circle",
                        new XAttribute("cx", dc.ToAbsoluteX(cx)),
                        new XAttribute("cy", dc.ToAbsoluteY(cy)),
                        new XAttribute("r", dc.ToRelativeX(rx))
                    );
                }
                else
                {
                    elem = new XElement(svg + "ellipse",
                        new XAttribute("cx", dc.ToAbsoluteX(cx)),
                        new XAttribute("cy", dc.ToAbsoluteY(cy)),
                        new XAttribute("rx", dc.ToRelativeX(rx)),
                        new XAttribute("ry", dc.ToRelativeY(ry))
                    );
                }
            }
            else
            {
                double sa = Math.Atan2((sya - cy) * rx, (sxa - cx) * ry);
                double sx = rx * Math.Cos(sa);
                double sy = ry * Math.Sin(sa);

                double ea = Math.Atan2((eya - cy) * rx, (exa - cx) * ry);
                double ex = rx * Math.Cos(ea);
                double ey = ry * Math.Sin(ea);

                double a = Math.Atan2((ex - sx) * (-sy) - (ey - sy) * (-sx), (ex - sx) * (-sx) + (ey - sy) * (-sy));

                elem = new XElement(svg + "path",
                new XAttribute("d", "M " + dc.ToAbsoluteX(cx) + "," + dc.ToAbsoluteY(cy)
                        + " L " + dc.ToAbsoluteX(sx + cx) + "," + dc.ToAbsoluteY(sy + cy)
                        + " A " + dc.ToRelativeX(rx) + "," + dc.ToRelativeY(ry)
                        + " 0 " + (a > 0 ? "1" : "0") + " 0"
                        + " " + dc.ToAbsoluteX(ex + cx) + "," + dc.ToAbsoluteY(ey + cy) + " Z"));
            }

            if (dc.Pen != null || dc.Brush != null)
            {
                if (useStyle)
                {
                    elem.Add(new XAttribute("class", GetClassString(dc.Pen, dc.Brush)));
                }
                else
                {
                    elem.Add(new XAttribute("style", GetStyle(dc.Pen, dc.Brush)));
                }

                if (dc.Brush?.Style == Constants.BS_HATCHED)
                {
                    string id = "pattern" + (patternNo++);
                    elem.Add(new XAttribute("fill", "url(#" + id + ")"));
                    defsNode.Add(dc.Brush.CreateFillPattern(id));
                }
            }
            parentNode.Add(elem);
        }

        public void Polygon(Point[] points)
        {
            XElement elem = new XElement(svg + "polygon");

            if (dc.Pen != null || dc.Brush != null)
            {
                if (useStyle)
                {
                    elem.Add(new XAttribute("class", GetClassString(dc.Pen, dc.Brush)));
                }
                else
                {
                    elem.Add(new XAttribute("style", GetStyle(dc.Pen, dc.Brush)));
                }

                if (dc.Brush?.Style == Constants.BS_HATCHED)
                {
                    string id = "pattern" + (patternNo++);
                    elem.Add(new XAttribute("fill", "url(#" + id + ")"));
                    defsNode.Add(dc.Brush.CreateFillPattern(id));
                }

                if (dc.PolyFillMode == Constants.WINDING)
                {
                    elem.Add(new XAttribute("fill-rule", "nonzero"));
                }
            }

            StringBuilder pointsTag = new StringBuilder();
            for (int i = 0; i < points.Length; i++)
            {
                if (i != 0)
                {
                    pointsTag.Append(" ");
                }

                pointsTag.Append((int)dc.ToAbsoluteX(points[i].X))
                    .Append(",")
                    .Append((int)dc.ToAbsoluteY(points[i].Y));
            }
            elem.Add(new XAttribute("points", pointsTag.ToString()));
            parentNode.Add(elem);
        }

        public void Polyline(Point[] points)
        {
            StringBuilder pointsTag = new StringBuilder();
            for (int i = 0; i < points.Length; i++)
            {
                if (i != 0)
                {
                    pointsTag.Append(" ");
                }

                pointsTag.Append((int)dc.ToAbsoluteX(points[i].X))
                    .Append(",")
                    .Append((int)dc.ToAbsoluteY(points[i].Y));
            }

            XElement elem = new XElement(svg + "polyline",
                new XAttribute("fill", "none"),
                new XAttribute("points", pointsTag.ToString())
            );

            if (dc.Pen != null)
            {
                if (useStyle)
                {
                    elem.Add(new XAttribute("class", GetClassString(dc.Pen)));
                }
                else
                {
                    elem.Add(new XAttribute("style", GetStyle(dc.Pen)));
                }
            }

            parentNode.Add(elem);
        }

        public void PolyPolygon(Point[][] points)
        {
            XElement elem = new XElement(svg + "path");

            if (dc.Pen != null || dc.Brush != null)
            {
                if (useStyle)
                {
                    elem.Add(new XAttribute("class", GetClassString(dc.Pen, dc.Brush)));
                }
                else
                {
                    elem.Add(new XAttribute("style", GetStyle(dc.Pen, dc.Brush)));
                }

                if (dc.Brush?.Style == Constants.BS_HATCHED)
                {
                    string id = "pattern" + (patternNo++);
                    elem.Add(new XAttribute("fill", "url(#" + id + ")"));
                    defsNode.Add(dc.Brush.CreateFillPattern(id));
                }

                if (dc.PolyFillMode == Constants.WINDING)
                {
                    elem.Add(new XAttribute("fill-rule", "nonzero"));
                }
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < points.Length; i++)
            {
                if (i != 0)
                {
                    sb.Append(" ");
                }

                for (int j = 0; j < points[i].Length; j++)
                {
                    if (j == 0)
                    {
                        sb.Append("M ");
                    }
                    else if (j == 1)
                    {
                        sb.Append(" L ");
                    }

                    sb.Append((int)dc.ToAbsoluteX(points[i][j].X))
                        .Append(",")
                        .Append((int)dc.ToAbsoluteY(points[i][j].Y))
                        .Append(" ");
                    if (j == points[i].Length - 1)
                    {
                        sb.Append("z");
                    }
                }
            }
            elem.Add(new XAttribute("d", sb.ToString()));
            parentNode.Add(elem);
        }

        public void RealizePalette()
        {
            // TODO
            Console.WriteLine("not implemented: realizePalette");
        }

        public void RestoreDC(short savedDC)
        {
            int limit = (savedDC < 0) ? -savedDC : saveDC.Count() - savedDC;
            for (int i = 0; i < limit; i++)
            {
                dc = (SvgDc)saveDC.Last();
                saveDC.RemoveLast();
            }

            if (!parentNode.HasElements)
            {
                parentNode.Remove();
            }

            parentNode = new XElement(svg + "g", dc.Mask != null ? new XAttribute("mask", "url(#" + dc.Mask.Attribute("id") + ")") : null);
            doc.Root.Add(parentNode);
        }

        public void Rectangle(short sx, short sy, short ex, short ey)
        {
            XElement elem = new XElement(svg + "rect");

            if (dc.Pen != null || dc.Brush != null)
            {
                if (useStyle)
                {
                    elem.Add(new XAttribute("class", GetClassString(dc.Pen, dc.Brush)));
                }
                else
                {
                    elem.Add(new XAttribute("style", GetStyle(dc.Pen, dc.Brush)));
                }

                if (dc.Brush?.Style == Constants.BS_HATCHED)
                {
                    string id = "pattern" + (patternNo++);
                    elem.Add(new XAttribute("fill", "url(#" + id + ")"));
                    defsNode.Add(dc.Brush.CreateFillPattern(id));
                }
            }

            elem.Add(new XAttribute("x", (int)dc.ToAbsoluteX(sx)));
            elem.Add(new XAttribute("y", (int)dc.ToAbsoluteY(sy)));
            elem.Add(new XAttribute("width", (int)dc.ToRelativeX(ex - sx)));
            elem.Add(new XAttribute("height", (int)dc.ToRelativeY(ey - sy)));
            parentNode.Add(elem);
        }

        public void ResizePalette(IGdiPalette palette)
        {
            // TODO
            Console.WriteLine("not implemented: ResizePalette");
        }

        public void RoundRect(short sx, short sy, short ex, short ey, short rw, short rh)
        {
            XElement elem = new XElement(svg + "rect");

            if (dc.Pen != null || dc.Brush != null)
            {
                if (useStyle)
                {
                    elem.Add(new XAttribute("class", GetClassString(dc.Pen, dc.Brush)));
                }
                else
                {
                    elem.Add(new XAttribute("style", GetStyle(dc.Pen, dc.Brush)));
                }

                if (dc.Brush?.Style == Constants.BS_HATCHED)
                {
                    string id = "pattern" + patternNo++;
                    elem.Add(new XAttribute("fill", "url(#" + id + ")"));
                    defsNode.Add(dc.Brush.CreateFillPattern(id));
                }
            }

            elem.Add(new XAttribute("x", (int)dc.ToAbsoluteX(sx)));
            elem.Add(new XAttribute("y", (int)dc.ToAbsoluteY(sy)));
            elem.Add(new XAttribute("width", (int)dc.ToRelativeX(ex - sx)));
            elem.Add(new XAttribute("height", (int)dc.ToRelativeY(ey - sy)));
            elem.Add(new XAttribute("rx", (int)dc.ToRelativeX(rw)));
            elem.Add(new XAttribute("ry", (int)dc.ToRelativeY(rh)));
            parentNode.Add(elem);
        }

        public void SaveDC()
        {
            saveDC.AddLast((SvgDc)dc.Clone());
        }

        public void ScaleViewportExtEx(short x, short xd, short y, short yd, Size old)
        {
            dc.ScaleViewportExtEx(x, xd, y, yd, old);
        }

        public void ScaleWindowExtEx(short x, short xd, short y, short yd, Size old)
        {
            dc.ScaleWindowExtEx(x, xd, y, yd, old);
        }

        public void SelectClipRgn(IGdiRegion rgn)
        {
            if (!parentNode.HasElements)
            {
                parentNode.Remove();
            }

            parentNode = new XElement(svg + "g");

            if (rgn != null)
            {
                XElement mask = new XElement(svg + "mask", new XAttribute("id", "mask" + maskNo++));

                if (dc.OffsetClipX != 0 || dc.OffsetClipY != 0)
                {
                    mask.Add(new XAttribute("transform", "translate(" + dc.OffsetClipX + "," + dc.OffsetClipY + ")"));
                }

                defsNode.Add(mask);

                XElement clip = new XElement(svg + "use",
                    new XAttribute("xlink:href", "url(#" + nameMap[rgn] + ")"),
                    new XAttribute("fill", "white")
                );

                mask.Add(clip);

                parentNode.Add(new XAttribute("mask", "url(#" + mask.Attribute("id") + ")"));
            }

            doc.Root.Add(parentNode);
        }

        public void SelectObject(IGdiObject obj)
        {
            if (obj is SvgBrush)
            {
                dc.Brush = (SvgBrush)obj;
            }
            else if (obj is SvgFont)
            {
                dc.Font = (SvgFont)obj;
            }
            else if (obj is SvgPen)
            {
                dc.Pen = (SvgPen)obj;
            }
        }

        public void SelectPalette(IGdiPalette palette, bool mode)
        {
            // TODO
            Console.WriteLine("not implemented: selectPalette");
        }

        public void SetBkColor(int color)
        {
            dc.BkColor = color;
        }

        public void SetBkMode(short mode)
        {
            dc.BkMode = mode;
        }

        public void SetDIBitsToDevice(short dx, short dy, short dw, short dh, short sx, short sy, ushort startscan, ushort scanlines, byte[] image, ushort colorUse)
        {
            StretchDIBits(dx, dy, dw, dh, sx, sy, dw, dh, image, colorUse, (uint)Constants.SRCCOPY);
        }

        public void SetLayout(uint layout)
        {
            dc.Layout = layout;
        }

        public void SetMapMode(short mode)
        {
            dc.MapMode = mode;
        }

        public void SetMapperFlags(uint flags)
        {
            dc.MapperFlags = flags;
        }

        public void SetPaletteEntries(IGdiPalette palette, ushort startIndex, int[] entries)
        {
            // TODO
            Console.WriteLine("not implemented: setPaletteEntries");
        }

        public void SetPixel(short x, short y, int color)
        {
            XElement elem = new XElement(svg + "rect",
                new XAttribute("stroke", "none"),
                new XAttribute("fill", SvgPen.ToColor(color)),
                new XAttribute("x", (int)dc.ToAbsoluteX(x)),
                new XAttribute("y", (int)dc.ToAbsoluteY(y)),
                new XAttribute("width", (int)dc.ToRelativeX(1)),
                new XAttribute("height", (int)dc.ToRelativeY(1))
            );
            parentNode.Add(elem);
        }

        public void SetPolyFillMode(short mode)
        {
            dc.PolyFillMode = mode;
        }

        public void SetRelAbs(short mode)
        {
            dc.RelAbs = mode;
        }

        public void SetROP2(short mode)
        {
            dc.ROP2 = mode;
        }

        public void SetStretchBltMode(short mode)
        {
            dc.StretchBltMode = mode;
        }

        public void SetTextAlign(short align)
        {
            dc.TextAlign = align;
        }

        public void SetTextCharacterExtra(short extra)
        {
            dc.TextCharacterExtra = extra;
        }

        public void SetTextColor(int color)
        {
            dc.TextColor = color;
        }

        public void SetTextJustification(short breakExtra, short breakCount)
        {
            if (breakCount > 0)
            {
                dc.TextSpace = (short)(Math.Abs((int)dc.ToRelativeX(breakExtra)) / breakCount);
            }
        }

        public void SetViewportExtEx(short x, short y, Size old)
        {
            dc.SetViewportExtEx(x, y, old);
        }

        public void SetViewportOrgEx(short x, short y, Point old)
        {
            dc.SetViewportOrgEx(x, y, old);
        }

        public void SetWindowExtEx(short width, short height, Size old)
        {
            dc.SetWindowExtEx(width, height, old);
        }

        public void SetWindowOrgEx(short x, short y, Point old)
        {
            dc.SetWindowOrgEx(x, y, old);
        }

        public void StretchBlt(byte[] image, short dx, short dy, short dw, short dh, short sx, short sy, short sw, short sh, uint rop)
        {
            DibStretchBlt(image, dx, dy, dw, dh, sx, sy, sw, sh, rop);
        }

        public void StretchDIBits(short dx, short dy, short dw, short dh, short sx, short sy, short sw, short sh, byte[] image, ushort usage, uint rop)
        {
            BmpToSvg(image, dx, dy, dw, dh, sx, sy, sw, sh, usage, rop);
        }

        public void TextOut(short x, short y, byte[] text)
        {
            XElement elem = new XElement(svg + "text");

            short escapement = 0;
            bool vertical = false;
            if (dc.Font != null)
            {
                if (useStyle)
                {
                    elem.Add(new XAttribute("class", GetClassString(dc.Font)));
                }
                else
                {
                    elem.Add(new XAttribute("style", GetStyle(dc.Font)));
                }

                if (dc.Font.FaceName.StartsWith("@"))
                {
                    vertical = true;
                    escapement = (short)(dc.Font.Escapement - 2700);
                }
                else
                {
                    escapement = dc.Font.Escapement;
                }
            }
            elem.Add(new XAttribute("fill", SvgObject.ToColor(dc.TextColor)));

            // style
            StringBuilder styleTag = new StringBuilder();
            int align = dc.TextAlign;

            if ((align & (Constants.TA_LEFT | Constants.TA_RIGHT | Constants.TA_CENTER)) == Constants.TA_RIGHT)
            {
                styleTag.Append("text-anchor: end; ");
            }
            else if ((align & (Constants.TA_LEFT | Constants.TA_RIGHT | Constants.TA_CENTER)) == Constants.TA_CENTER)
            {
                styleTag.Append("text-anchor: middle; ");
            }

            if (vertical)
            {
                elem.Add(new XAttribute("writing-mode", "tb"));
                styleTag.Append("dominant-baseline: ideographic; ");
            }
            else
            {
                if ((align & (Constants.TA_BOTTOM | Constants.TA_TOP | Constants.TA_BASELINE)) == Constants.TA_BASELINE)
                {
                    styleTag.Append("dominant-baseline: alphabetic; ");
                }
                else
                {
                    styleTag.Append("dominant-baseline: text-before-edge; ");
                }
            }

            if ((align & Constants.TA_RTLREADING) == Constants.TA_RTLREADING)
            {
                styleTag.Append("unicode-bidi: bidi-override; direction: rtl; ");
            }

            if (dc.TextSpace > 0)
            {
                styleTag.Append("word-spacing: " + dc.TextSpace + "; ");
            }

            if (styleTag.Length > 0)
            {
                elem.Add(new XAttribute("style", styleTag.ToString()));
            }

            elem.Add(new XAttribute("stroke", "none"));

            int ax = (int)dc.ToAbsoluteX(x);
            int ay = (int)dc.ToAbsoluteY(y);
            elem.Add(new XAttribute("x", ax));
            elem.Add(new XAttribute("y", ay));

            if (escapement != 0)
            {
                elem.Add(new XAttribute("transform", "rotate(" + (-escapement / 10.0) + ", " + ax + ", " + ay + ")"));
            }

            string str = null;
            if (dc.Font != null)
            {
                str = GdiUtils.ConvertString(text, dc.Font.Charset);
            }
            else
            {
                str = GdiUtils.ConvertString(text, Constants.DEFAULT_CHARSET);
            }

            if (dc.TextCharacterExtra != 0)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < str.Length - 1; i++)
                {
                    if (i != 0)
                    {
                        sb.Append(" ");
                    }

                    sb.Append((int)dc.ToRelativeX(dc.TextCharacterExtra));
                }

                elem.Add(new XAttribute("dx", sb.ToString()));
            }

            if (!string.IsNullOrEmpty(dc.Font?.Lang))
            {
                elem.Add(new XAttribute(xml + "lang", dc.Font.Lang));
            }

            elem.Add(new XAttribute(xml + "space", "preserve"));
            AppendText(elem, str);
            parentNode.Add(elem);
        }

        public void Footer()
        {
            XElement root = doc.Root;
            if (root.Attribute("width") == null && dc.WindowWidth != 0)
            {
                root.Add(new XAttribute("width", Math.Abs(dc.WindowWidth)));
            }

            if (root.Attribute("height") == null && dc.WindowHeight != 0)
            {
                root.Add(new XAttribute("height", Math.Abs(dc.WindowHeight)));
            }

            if (dc.WindowWidth != 0 && dc.WindowHeight != 0)
            {
                root.Add(new XAttribute("viewBox", "0 0 " + Math.Abs(dc.WindowWidth) + " " + Math.Abs(dc.WindowHeight)));
                root.Add(new XAttribute("preserveAspectRatio", "xMidYMid meet"));
            }
            root.Add(new XAttribute("stroke-linecap", "round"));
            root.Add(new XAttribute("fill-rule", "evenodd"));

            if (useStyle)
            {
                if (!styleNode.HasElements && string.IsNullOrEmpty(styleNode.Value))
                {
                    styleNode.Remove();
                }
                else
                {
                    styleNode.AddFirst(new XText("\n"));
                }
            }

            if (!defsNode.HasElements && !defsNode.HasAttributes && string.IsNullOrEmpty(defsNode.Value))
            {
                defsNode.Remove();
            }
        }

        private string GetClassString(IGdiObject obj1, IGdiObject obj2)
        {
            string name1 = GetClassString(obj1);
            string name2 = GetClassString(obj2);
            if (name1 != null && name2 != null)
            {
                return name1 + " " + name2;
            }

            if (name1 != null)
            {
                return name1;
            }

            if (name2 != null)
            {
                return name2;
            }

            return "";
        }

        private string GetStyle(params IGdiObject[] args)
        {
            var builder = new StringBuilder();
            foreach (var arg in args)
            {
                if (arg is SvgObject style)
                {
                    builder.Append(style.ToString());
                }
            }

            return builder.ToString();
        }

        private string GetClassString(IGdiObject style)
        {
            if (style == null)
            {
                return "";
            }

            if (nameMap.TryGetValue(style, out string str))
            {
                return str;
            }
            else
            {
                return "";
            }
        }

        private void AppendText(XElement elem, string str)
        {
            if (compatible)
            {
                str = Regex.Replace(str, "\\r\\n", "\u00A0");
                str = Regex.Replace(str, "\\t\\r\\n", "\u00A0");
            }
            SvgFont font = dc.Font;
            if (ReplaceSymbolFont && font != null)
            {
                if ("Symbol".Equals(font.FaceName))
                {
                    int state = 0; // 0: default, 1: serif, 2: sans-serif
                    int start = 0;
                    char[] ca = str.ToCharArray();
                    for (int i = 0; i < ca.Length; i++)
                    {
                        int nstate = state;
                        switch (ca[i])
                        {
                            case '"': ca[i] = '\u2200'; nstate = 1; break;
                            case '$': ca[i] = '\u2203'; nstate = 1; break;
                            case '\'': ca[i] = '\u220D'; nstate = 1; break;
                            case '*': ca[i] = '\u2217'; nstate = 1; break;
                            case '-': ca[i] = '\u2212'; nstate = 1; break;
                            case '@': ca[i] = '\u2245'; nstate = 1; break;
                            case 'A': ca[i] = '\u0391'; nstate = 1; break;
                            case 'B': ca[i] = '\u0392'; nstate = 1; break;
                            case 'C': ca[i] = '\u03A7'; nstate = 1; break;
                            case 'D': ca[i] = '\u0394'; nstate = 1; break;
                            case 'E': ca[i] = '\u0395'; nstate = 1; break;
                            case 'F': ca[i] = '\u03A6'; nstate = 1; break;
                            case 'G': ca[i] = '\u0393'; nstate = 1; break;
                            case 'H': ca[i] = '\u0397'; nstate = 1; break;
                            case 'I': ca[i] = '\u0399'; nstate = 1; break;
                            case 'J': ca[i] = '\u03D1'; nstate = 1; break;
                            case 'K': ca[i] = '\u039A'; nstate = 1; break;
                            case 'L': ca[i] = '\u039B'; nstate = 1; break;
                            case 'M': ca[i] = '\u039C'; nstate = 1; break;
                            case 'N': ca[i] = '\u039D'; nstate = 1; break;
                            case 'O': ca[i] = '\u039F'; nstate = 1; break;
                            case 'P': ca[i] = '\u03A0'; nstate = 1; break;
                            case 'Q': ca[i] = '\u0398'; nstate = 1; break;
                            case 'R': ca[i] = '\u03A1'; nstate = 1; break;
                            case 'S': ca[i] = '\u03A3'; nstate = 1; break;
                            case 'T': ca[i] = '\u03A4'; nstate = 1; break;
                            case 'U': ca[i] = '\u03A5'; nstate = 1; break;
                            case 'V': ca[i] = '\u03C3'; nstate = 1; break;
                            case 'W': ca[i] = '\u03A9'; nstate = 1; break;
                            case 'X': ca[i] = '\u039E'; nstate = 1; break;
                            case 'Y': ca[i] = '\u03A8'; nstate = 1; break;
                            case 'Z': ca[i] = '\u0396'; nstate = 1; break;
                            case '\\': ca[i] = '\u2234'; nstate = 1; break;
                            case '^': ca[i] = '\u22A5'; nstate = 1; break;
                            case '`': ca[i] = '\uF8E5'; nstate = 1; break;
                            case 'a': ca[i] = '\u03B1'; nstate = 1; break;
                            case 'b': ca[i] = '\u03B2'; nstate = 1; break;
                            case 'c': ca[i] = '\u03C7'; nstate = 1; break;
                            case 'd': ca[i] = '\u03B4'; nstate = 1; break;
                            case 'e': ca[i] = '\u03B5'; nstate = 1; break;
                            case 'f': ca[i] = '\u03C6'; nstate = 1; break;
                            case 'g': ca[i] = '\u03B3'; nstate = 1; break;
                            case 'h': ca[i] = '\u03B7'; nstate = 1; break;
                            case 'i': ca[i] = '\u03B9'; nstate = 1; break;
                            case 'j': ca[i] = '\u03D5'; nstate = 1; break;
                            case 'k': ca[i] = '\u03BA'; nstate = 1; break;
                            case 'l': ca[i] = '\u03BB'; nstate = 1; break;
                            case 'm': ca[i] = '\u03BC'; nstate = 1; break;
                            case 'n': ca[i] = '\u03BD'; nstate = 1; break;
                            case 'o': ca[i] = '\u03BF'; nstate = 1; break;
                            case 'p': ca[i] = '\u03C0'; nstate = 1; break;
                            case 'q': ca[i] = '\u03B8'; nstate = 1; break;
                            case 'r': ca[i] = '\u03C1'; nstate = 1; break;
                            case 's': ca[i] = '\u03C3'; nstate = 1; break;
                            case 't': ca[i] = '\u03C4'; nstate = 1; break;
                            case 'u': ca[i] = '\u03C5'; nstate = 1; break;
                            case 'v': ca[i] = '\u03D6'; nstate = 1; break;
                            case 'w': ca[i] = '\u03C9'; nstate = 1; break;
                            case 'x': ca[i] = '\u03BE'; nstate = 1; break;
                            case 'y': ca[i] = '\u03C8'; nstate = 1; break;
                            case 'z': ca[i] = '\u03B6'; nstate = 1; break;
                            case '~': ca[i] = '\u223C'; nstate = 1; break;
                            case '\u00A0': ca[i] = '\u20AC'; nstate = 1; break;
                            case '\u00A1': ca[i] = '\u03D2'; nstate = 1; break;
                            case '\u00A2': ca[i] = '\u2032'; nstate = 1; break;
                            case '\u00A3': ca[i] = '\u2264'; nstate = 1; break;
                            case '\u00A4': ca[i] = '\u2044'; nstate = 1; break;
                            case '\u00A5': ca[i] = '\u221E'; nstate = 1; break;
                            case '\u00A6': ca[i] = '\u0192'; nstate = 1; break;
                            case '\u00A7': ca[i] = '\u2663'; nstate = 1; break;
                            case '\u00A8': ca[i] = '\u2666'; nstate = 1; break;
                            case '\u00A9': ca[i] = '\u2665'; nstate = 1; break;
                            case '\u00AA': ca[i] = '\u2660'; nstate = 1; break;
                            case '\u00AB': ca[i] = '\u2194'; nstate = 1; break;
                            case '\u00AC': ca[i] = '\u2190'; nstate = 1; break;
                            case '\u00AD': ca[i] = '\u2191'; nstate = 1; break;
                            case '\u00AE': ca[i] = '\u2192'; nstate = 1; break;
                            case '\u00AF': ca[i] = '\u2193'; nstate = 1; break;
                            case '\u00B2': ca[i] = '\u2033'; nstate = 1; break;
                            case '\u00B3': ca[i] = '\u2265'; nstate = 1; break;
                            case '\u00B4': ca[i] = '\u00D7'; nstate = 1; break;
                            case '\u00B5': ca[i] = '\u221D'; nstate = 1; break;
                            case '\u00B6': ca[i] = '\u2202'; nstate = 1; break;
                            case '\u00B7': ca[i] = '\u2022'; nstate = 1; break;
                            case '\u00B8': ca[i] = '\u00F7'; nstate = 1; break;
                            case '\u00B9': ca[i] = '\u2260'; nstate = 1; break;
                            case '\u00BA': ca[i] = '\u2261'; nstate = 1; break;
                            case '\u00BB': ca[i] = '\u2248'; nstate = 1; break;
                            case '\u00BC': ca[i] = '\u2026'; nstate = 1; break;
                            case '\u00BD': ca[i] = '\u23D0'; nstate = 1; break;
                            case '\u00BE': ca[i] = '\u23AF'; nstate = 1; break;
                            case '\u00BF': ca[i] = '\u21B5'; nstate = 1; break;
                            case '\u00C0': ca[i] = '\u2135'; nstate = 1; break;
                            case '\u00C1': ca[i] = '\u2111'; nstate = 1; break;
                            case '\u00C2': ca[i] = '\u211C'; nstate = 1; break;
                            case '\u00C3': ca[i] = '\u2118'; nstate = 1; break;
                            case '\u00C4': ca[i] = '\u2297'; nstate = 1; break;
                            case '\u00C5': ca[i] = '\u2295'; nstate = 1; break;
                            case '\u00C6': ca[i] = '\u2205'; nstate = 1; break;
                            case '\u00C7': ca[i] = '\u2229'; nstate = 1; break;
                            case '\u00C8': ca[i] = '\u222A'; nstate = 1; break;
                            case '\u00C9': ca[i] = '\u2283'; nstate = 1; break;
                            case '\u00CA': ca[i] = '\u2287'; nstate = 1; break;
                            case '\u00CB': ca[i] = '\u2284'; nstate = 1; break;
                            case '\u00CC': ca[i] = '\u2282'; nstate = 1; break;
                            case '\u00CD': ca[i] = '\u2286'; nstate = 1; break;
                            case '\u00CE': ca[i] = '\u2208'; nstate = 1; break;
                            case '\u00CF': ca[i] = '\u2209'; nstate = 1; break;
                            case '\u00D0': ca[i] = '\u2220'; nstate = 1; break;
                            case '\u00D1': ca[i] = '\u2207'; nstate = 1; break;
                            case '\u00D2': ca[i] = '\u00AE'; nstate = 1; break;
                            case '\u00D3': ca[i] = '\u00A9'; nstate = 1; break;
                            case '\u00D4': ca[i] = '\u2122'; nstate = 1; break;
                            case '\u00D5': ca[i] = '\u220F'; nstate = 1; break;
                            case '\u00D6': ca[i] = '\u221A'; nstate = 1; break;
                            case '\u00D7': ca[i] = '\u22C5'; nstate = 1; break;
                            case '\u00D8': ca[i] = '\u00AC'; nstate = 1; break;
                            case '\u00D9': ca[i] = '\u2227'; nstate = 1; break;
                            case '\u00DA': ca[i] = '\u2228'; nstate = 1; break;
                            case '\u00DB': ca[i] = '\u21D4'; nstate = 1; break;
                            case '\u00DC': ca[i] = '\u21D0'; nstate = 1; break;
                            case '\u00DD': ca[i] = '\u21D1'; nstate = 1; break;
                            case '\u00DE': ca[i] = '\u21D2'; nstate = 1; break;
                            case '\u00DF': ca[i] = '\u21D3'; nstate = 1; break;
                            case '\u00E0': ca[i] = '\u25CA'; nstate = 1; break;
                            case '\u00E1': ca[i] = '\u3008'; nstate = 1; break;
                            case '\u00E2': ca[i] = '\u00AE'; nstate = 2; break;
                            case '\u00E3': ca[i] = '\u00A9'; nstate = 2; break;
                            case '\u00E4': ca[i] = '\u2122'; nstate = 2; break;
                            case '\u00E5': ca[i] = '\u2211'; nstate = 1; break;
                            case '\u00E6': ca[i] = '\u239B'; nstate = 1; break;
                            case '\u00E7': ca[i] = '\u239C'; nstate = 1; break;
                            case '\u00E8': ca[i] = '\u239D'; nstate = 1; break;
                            case '\u00E9': ca[i] = '\u23A1'; nstate = 1; break;
                            case '\u00EA': ca[i] = '\u23A2'; nstate = 1; break;
                            case '\u00EB': ca[i] = '\u23A3'; nstate = 1; break;
                            case '\u00EC': ca[i] = '\u23A7'; nstate = 1; break;
                            case '\u00ED': ca[i] = '\u23A8'; nstate = 1; break;
                            case '\u00EE': ca[i] = '\u23A9'; nstate = 1; break;
                            case '\u00EF': ca[i] = '\u23AA'; nstate = 1; break;
                            case '\u00F0': ca[i] = '\uF8FF'; nstate = 1; break;
                            case '\u00F1': ca[i] = '\u3009'; nstate = 1; break;
                            case '\u00F2': ca[i] = '\u222B'; nstate = 1; break;
                            case '\u00F3': ca[i] = '\u2320'; nstate = 1; break;
                            case '\u00F4': ca[i] = '\u23AE'; nstate = 1; break;
                            case '\u00F5': ca[i] = '\u2321'; nstate = 1; break;
                            case '\u00F6': ca[i] = '\u239E'; nstate = 1; break;
                            case '\u00F7': ca[i] = '\u239F'; nstate = 1; break;
                            case '\u00F8': ca[i] = '\u23A0'; nstate = 1; break;
                            case '\u00F9': ca[i] = '\u23A4'; nstate = 1; break;
                            case '\u00FA': ca[i] = '\u23A5'; nstate = 1; break;
                            case '\u00FB': ca[i] = '\u23A6'; nstate = 1; break;
                            case '\u00FC': ca[i] = '\u23AB'; nstate = 1; break;
                            case '\u00FD': ca[i] = '\u23AC'; nstate = 1; break;
                            case '\u00FE': ca[i] = '\u23AD'; nstate = 1; break;
                            case '\u00FF': ca[i] = '\u2192'; nstate = 1; break;
                            default: nstate = 0; break;
                        }

                        if (nstate != state)
                        {
                            if (start < i)
                            {
                                XText text = new XText(new string(ca).Substring(start, i - start));
                                if (state == 0)
                                {
                                    elem.Add(text);
                                }
                                else if (state == 1)
                                {
                                    elem.Add(new XElement(svg + "tspan", new XAttribute("font-family", "serif"), text));
                                }
                                else if (state == 2)
                                {
                                    elem.Add(new XElement(svg + "tspan", new XAttribute("font-family", "sans-serif"), text));
                                }

                                start = i;
                            }
                            state = nstate;
                        }
                    }

                    if (start < ca.Length)
                    {
                        XText text = new XText(new string(ca).Substring(start, ca.Length - start));
                        if (state == 0)
                        {
                            elem.Add(text);
                        }
                        else if (state == 1)
                        {
                            elem.Add(new XElement(svg + "tspan", new XAttribute("font-family", "serif"), text));
                        }
                        else if (state == 2)
                        {
                            elem.Add(new XElement(svg + "tspan", new XAttribute("font-family", "sans-serif"), text));
                        }
                    }
                    return;
                }
            }

            elem.Add(new XText(str));
        }

        private void BmpToSvg(byte[] image, short dx, short dy, short dw, short dh, short sx, short sy, short sw, short sh, int usage, uint rop)
        {
            if (image == null || image.Length == 0)
            {
                return;
            }

            Bitmap bmp1 = new Bitmap(new MemoryStream(DibToBmp(image)));
            if (dh < 0)
            {
                bmp1.RotateFlip(RotateFlipType.RotateNoneFlipY);
            }

            using (var convertedImage = new MemoryStream())
            {
                bmp1.Save(convertedImage, ImageFormat.Png);
                image = convertedImage.ToArray();
            }

            if (image == null || image.Length == 0)
            {
                return;
            }

            string data = "data:image/png;base64," + Convert.ToBase64String(image);
            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            XElement elem = new XElement(svg + "image");
            int x = (int)dc.ToAbsoluteX(dx);
            int y = (int)dc.ToAbsoluteY(dy);
            int width = (int)dc.ToRelativeX(dw);
            int height = (int)dc.ToRelativeY(dh);

            if (width < 0 && height < 0)
            {
                elem.Add(new XAttribute("transform", "scale(-1, -1) translate(" + -x + ", " + -y + ")"));
            }
            else if (width < 0)
            {
                elem.Add(new XAttribute("transform", "scale(-1, 1) translate(" + -x + ", " + y + ")"));
            }
            else if (height < 0)
            {
                elem.Add(new XAttribute("transform", "scale(1, -1) translate(" + x + ", " + -y + ")"));
            }
            else
            {
                elem.Add(new XAttribute("x", x));
                elem.Add(new XAttribute("y", y));
            }

            elem.Add(new XAttribute("width", Math.Abs(width)));
            elem.Add(new XAttribute("height", Math.Abs(height)));

            if (sx != 0 || sy != 0 || sw != dw || sh != dh)
            {
                elem.Add(new XAttribute("viewBox", sx + " " + sy + " " + sw + " " + sh));
                elem.Add(new XAttribute("preserveAspectRatio", "none"));
            }

            string ropFilter = dc.GetRopFilter(rop);
            if (ropFilter != null)
            {
                elem.Add(new XAttribute("filter", ropFilter));
            }

            elem.Add(new XAttribute(xlink + "href", data));
            parentNode.Add(elem);
        }

        private byte[] DibToBmp(byte[] dib)
        {
            byte[] data = new byte[14 + dib.Length];

            /* BitmapFileHeader */
            data[0] = 0x42; // 'B'
            data[1] = 0x4d; // 'M'

            long bfSize = data.Length;
            data[2] = (byte)(bfSize & 0xff);
            data[3] = (byte)((bfSize >> 8) & 0xff);
            data[4] = (byte)((bfSize >> 16) & 0xff);
            data[5] = (byte)((bfSize >> 24) & 0xff);

            // reserved 1
            data[6] = 0x00;
            data[7] = 0x00;

            // reserved 2
            data[8] = 0x00;
            data[9] = 0x00;

            // offset
            long bfOffBits = 14;

            /* BitmapInfoHeader */
            long biSize = (dib[0] & 0xff) + ((dib[1] & 0xff) << 8)
                    + ((dib[2] & 0xff) << 16) + ((dib[3] & 0xff) << 24);
            bfOffBits += biSize;

            int biBitCount = (dib[14] & 0xff) + ((dib[15] & 0xff) << 8);

            long clrUsed = (dib[32] & 0xff) + ((dib[33] & 0xff) << 8)
                    + ((dib[34] & 0xff) << 16) + ((dib[35] & 0xff) << 24);

            switch (biBitCount)
            {
                case 1:
                    bfOffBits += (clrUsed == 0L ? 2 : clrUsed) * 4;
                    break;
                case 4:
                    bfOffBits += (clrUsed == 0L ? 16 : clrUsed) * 4;
                    break;
                case 8:
                    bfOffBits += (clrUsed == 0L ? 256 : clrUsed) * 4;
                    break;
            }

            data[10] = (byte)(bfOffBits & 0xff);
            data[11] = (byte)((bfOffBits >> 8) & 0xff);
            data[12] = (byte)((bfOffBits >> 16) & 0xff);
            data[13] = (byte)((bfOffBits >> 24) & 0xff);

            Array.Copy(dib, 0, data, 14, dib.Length);

            return data;
        }
    }
}