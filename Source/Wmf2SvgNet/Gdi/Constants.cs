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
    public static class Constants
    {
        public const short OPAQUE = 2;
        public const short TRANSPARENT = 1;

        public const short TA_BASELINE = 24;
        public const short TA_BOTTOM = 8;
        public const short TA_TOP = 0;
        public const short TA_CENTER = 6;
        public const short TA_LEFT = 0;
        public const short TA_RIGHT = 2;
        public const short TA_NOUPDATECP = 0;
        public const short TA_RTLREADING = 256;
        public const short TA_UPDATECP = 1;
        public const short VTA_BASELINE = 24;
        public const short VTA_CENTER = 6;

        public const ushort ETO_CLIPPED = 4;
        public const ushort ETO_NUMERICSLOCAL = 1024;
        public const ushort ETO_NUMERICSLATIN = 2048;
        public const ushort ETO_GLYPH_INDEX = 16;
        public const ushort ETO_OPAQUE = 2;
        public const ushort ETO_PDY = 8192;
        public const ushort ETO_RTLREADING = 128;
        public const ushort ETO_IGNORELANGUAGE = 4096;

        public const short MM_ANISOTROPIC = 8;
        public const short MM_HIENGLISH = 5;
        public const short MM_HIMETRIC = 3;
        public const short MM_ISOTROPIC = 7;
        public const short MM_LOENGLISH = 4;
        public const short MM_LOMETRIC = 2;
        public const short MM_TEXT = 1;
        public const short MM_TWIPS = 6;

        public const short STRETCH_ANDSCANS = 2;
        public const short STRETCH_DELETESCANS = 3;
        public const short STRETCH_HALFTONE = 4;
        public const short STRETCH_ORSCANS = 2;
        public const short BLACKONWHITE = 2;
        public const short COLORONCOLOR = 3;
        public const short HALFTONE = 4;
        public const short WHITEONBLACK = 2;

        public const short ALTERNATE = 1;
        public const short WINDING = 2;

        public const short R2_BLACK = 1;
        public const short R2_COPYPEN = 13;
        public const short R2_MASKNOTPEN = 3;
        public const short R2_MASKPEN = 9;
        public const short R2_MASKPENNOT = 5;
        public const short R2_MERGENOTPEN = 12;
        public const short R2_MERGEPEN = 15;
        public const short R2_MERGEPENNOT = 14;
        public const short R2_NOP = 11;
        public const short R2_NOT = 6;
        public const short R2_NOTCOPYPEN = 4;
        public const short R2_NOTMASKPEN = 8;
        public const short R2_NOTMERGEPEN = 2;
        public const short R2_NOTXORPEN = 10;
        public const short R2_WHITE = 16;
        public const short R2_XORPEN = 7;

        public const uint BLACKNESS = 66;
        public const uint DSTINVERT = 5570569;
        public const uint MERGECOPY = 12583114;
        public const uint MERGEPAINT = 12255782;
        public const uint NOTSRCCOPY = 3342344;
        public const uint NOTSRCERASE = 1114278;
        public const uint PATCOPY = 15728673;
        public const uint PATINVERT = 5898313;
        public const uint PATPAINT = 16452105;
        public const uint SRCAND = 8913094;
        public const uint SRCCOPY = 13369376;
        public const uint SRCERASE = 4457256;
        public const uint SRCINVERT = 6684742;
        public const uint SRCPAINT = 15597702;
        public const uint WHITENESS = 16711778;

        public const int DIB_RGB_COLORS = 0;
        public const int DIB_PAL_COLORS = 1;

        public const int LAYOUT_BITMAPORIENTATIONPRESERVED = 8;
        public const int LAYOUT_RTL = 1;

        public const int ABSOLUTE = 1;
        public const int RELATIVE = 2;

        public const int ASPECT_FILTERING = 1;

        public const int BS_DIBPATTERN = 5;
        public const int BS_DIBPATTERN8X8 = 8;
        public const int BS_DIBPATTERNPT = 6;
        public const int BS_HATCHED = 2;
        public const int BS_HOLLOW = 1;
        public const int BS_NULL = 1;
        public const int BS_PATTERN = 3;
        public const int BS_PATTERN8X8 = 7;
        public const int BS_SOLID = 0;

        public const ushort HS_HORIZONTAL = 0;
        public const ushort HS_VERTICAL = 1;
        public const ushort HS_FDIAGONAL = 2;
        public const ushort HS_BDIAGONAL = 3;
        public const ushort HS_CROSS = 4;
        public const ushort HS_DIAGCROSS = 5;

        public const short FW_DONTCARE = 0;
        public const short FW_THIN = 100;
        public const short FW_EXTRALIGHT = 200;
        public const short FW_ULTRALIGHT = 200;
        public const short FW_LIGHT = 300;
        public const short FW_NORMAL = 400;
        public const short FW_REGULAR = 400;
        public const short FW_MEDIUM = 500;
        public const short FW_SEMIBOLD = 600;
        public const short FW_DEMIBOLD = 600;
        public const short FW_BOLD = 700;
        public const short FW_EXTRABOLD = 800;
        public const short FW_ULTRABOLD = 800;
        public const short FW_HEAVY = 900;
        public const short FW_BLACK = 900;

        public const byte ANSI_CHARSET = 0;
        public const byte DEFAULT_CHARSET = 1;
        public const byte SYMBOL_CHARSET = 2;
        public const byte MAC_CHARSET = 77;
        public const byte SHIFTJIS_CHARSET = 128;
        public const byte HANGUL_CHARSET = 129;
        public const byte JOHAB_CHARSET = 130;
        public const byte GB2312_CHARSET = 134;
        public const byte CHINESEBIG5_CHARSET = 136;
        public const byte GREEK_CHARSET = 161;
        public const byte TURKISH_CHARSET = 162;
        public const byte VIETNAMESE_CHARSET = 163;
        public const byte ARABIC_CHARSET = 178;
        public const byte HEBREW_CHARSET = 177;
        public const byte BALTIC_CHARSET = 186;
        public const byte RUSSIAN_CHARSET = 204;
        public const byte THAI_CHARSET = 222;
        public const byte EASTEUROPE_CHARSET = 238;
        public const byte OEM_CHARSET = 255;

        public const int OUT_DEFAULT_PRECIS = 0;
        public const int OUT_STRING_PRECIS = 1;
        public const int OUT_CHARACTER_PRECIS = 2;
        public const int OUT_STROKE_PRECIS = 3;
        public const int OUT_TT_PRECIS = 4;
        public const int OUT_DEVICE_PRECIS = 5;
        public const int OUT_RASTER_PRECIS = 6;
        public const int OUT_TT_ONLY_PRECIS = 7;
        public const int OUT_OUTLINE_PRECIS = 8;
        public const int OUT_SCREEN_OUTLINE_PRECIS = 9;

        public const int CLIP_DEFAULT_PRECIS = 0;
        public const int CLIP_CHARACTER_PRECIS = 1;
        public const int CLIP_STROKE_PRECIS = 2;
        public const int CLIP_MASK = 15;
        public const int CLIP_LH_ANGLES = 16;
        public const int CLIP_TT_ALWAYS = 32;
        public const int CLIP_EMBEDDED = 128;

        public const int DEFAULT_QUALITY = 0;
        public const int DRAFT_QUALITY = 1;
        public const int PROOF_QUALITY = 2;
        public const int NONANTIALIASED_QUALITY = 3;
        public const int ANTIALIASED_QUALITY = 4;
        public const int CLEARTYPE_QUALITY = 5; // Windows XP only

        public const int DEFAULT_PITCH = 0;
        public const int FIXED_PITCH = 1;
        public const int VARIABLE_PITCH = 2;

        public const byte FF_DONTCARE = 0;
        public const byte FF_ROMAN = 16;
        public const byte FF_SWISS = 32;
        public const byte FF_MODERN = 48;
        public const byte FF_SCRIPT = 64;
        public const byte FF_DECORATIVE = 80;

        public const ushort PS_SOLID = 0;
        public const ushort PS_DASH = 1;
        public const ushort PS_DOT = 2;
        public const ushort PS_DASHDOT = 3;
        public const ushort PS_DASHDOTDOT = 4;
        public const ushort PS_NULL = 5;
        public const ushort PS_INSIDEFRAME = 6;

        public const int NULLREGION = 1;
        public const int SIMPLEREGION = 2;
        public const int COMPLEXREGION = 3;
        public const int ERROR = 0;
    }
}