using System;
using System.Text;

namespace Wmf2SvgNet.Gdi
{

	public class GdiUtils
	{
		public static string ConvertString(byte[] chars, byte charset)
		{
			int length = 0;
			while (length < chars.Length && chars[length] != 0)
				length++;

			try
			{
				return Encoding.GetEncoding(GetCharset(charset)).GetString(chars).Substring(0, length);
			}
			catch (Exception e)
			{
				try
				{
					return Encoding.GetEncoding("US-ASCII").GetString(chars).Substring(0, length);
				}
				catch (Exception e2)
				{
					throw new Exception("String conversion error", e2);
				}
			}
		}

		public static string GetCharset(byte charset)
		{
			switch (charset)
			{
				case Constants.ANSI_CHARSET:
					return "Cp1252";
				case Constants.SYMBOL_CHARSET:
					return "ISO-8859-1";
				case Constants.MAC_CHARSET:
					return "MacRoman";
				case Constants.SHIFTJIS_CHARSET:
					return "MS932";
				case Constants.HANGUL_CHARSET:
					return "MS949";
				case Constants.JOHAB_CHARSET:
					return "Johab";
				case Constants.GB2312_CHARSET:
					return "MS936";
				case Constants.CHINESEBIG5_CHARSET:
					return "MS950";
				case Constants.GREEK_CHARSET:
					return "Cp1253";
				case Constants.TURKISH_CHARSET:
					return "Cp1254";
				case Constants.VIETNAMESE_CHARSET:
					return "Cp1258";
				case Constants.HEBREW_CHARSET:
					return "Cp1255";
				case Constants.ARABIC_CHARSET:
					return "Cp1256";
				case Constants.BALTIC_CHARSET:
					return "Cp1257";
				case Constants.RUSSIAN_CHARSET:
					return "Cp1251";
				case Constants.THAI_CHARSET:
					return "MS874";
				case Constants.EASTEUROPE_CHARSET:
					return "Cp1250";
				case Constants.OEM_CHARSET:
					return "Cp1252";
				default:
					return "Cp1252";
			}
		}

		public static string GetLanguage(int charset)
		{
			switch (charset)
			{
				case Constants.ANSI_CHARSET:
					return "en";
				case Constants.SYMBOL_CHARSET:
					return "en";
				case Constants.MAC_CHARSET:
					return "en";
				case Constants.SHIFTJIS_CHARSET:
					return "ja";
				case Constants.HANGUL_CHARSET:
					return "ko";
				case Constants.JOHAB_CHARSET:
					return "ko";
				case Constants.GB2312_CHARSET:
					return "zh-CN";
				case Constants.CHINESEBIG5_CHARSET:
					return "zh-TW";
				case Constants.GREEK_CHARSET:
					return "el";
				case Constants.TURKISH_CHARSET:
					return "tr";
				case Constants.VIETNAMESE_CHARSET:
					return "vi";
				case Constants.HEBREW_CHARSET:
					return "iw";
				case Constants.ARABIC_CHARSET:
					return "ar";
				case Constants.BALTIC_CHARSET:
					return "bat";
				case Constants.RUSSIAN_CHARSET:
					return "ru";
				case Constants.THAI_CHARSET:
					return "th";
				case Constants.EASTEUROPE_CHARSET:
					return null;
				case Constants.OEM_CHARSET:
					return null;
				default:
					return null;
			}
		}

		private static int[][] FBA_SHIFT_JIS = new int[][] { new int[] { 0x81, 0x9F }, new int[] { 0xE0, 0xFC } };
		private static int[][] FBA_HANGUL_CHARSET = new int[][] { new int[] { 0x80, 0xFF } };
		private static int[][] FBA_JOHAB_CHARSET = new int[][] { new int[] { 0x80, 0xFF } };
		private static int[][] FBA_GB2312_CHARSET = new int[][] { new int[] { 0x80, 0xFF } };
		private static int[][] FBA_CHINESEBIG5_CHARSET = new int[][] { new int[] { 0xA1, 0xFE } };

		public static int[][] GetFirstByteArea(int charset)
		{
			switch (charset)
			{
				case Constants.SHIFTJIS_CHARSET:
					return FBA_SHIFT_JIS;
				case Constants.HANGUL_CHARSET:
					return FBA_HANGUL_CHARSET;
				case Constants.JOHAB_CHARSET:
					return FBA_JOHAB_CHARSET;
				case Constants.GB2312_CHARSET:
					return FBA_GB2312_CHARSET;
				case Constants.CHINESEBIG5_CHARSET:
					return FBA_CHINESEBIG5_CHARSET;
				default:
					return null;
			}
		}

		public static short[] FixTextDx(int charset, byte[] chars, short[] dx)
		{
			if (dx == null || dx.Length == 0)
			{
				return null;
			}

			int[][] area = GdiUtils.GetFirstByteArea(charset);
			if (area == null)
			{
				return dx;
			}

			int n = 0;
			bool skip = false;

			for (int i = 0; i < chars.Length && i < dx.Length; i++)
			{
				int c = (0xFF & chars[i]);

				if (skip)
				{
					dx[n - 1] += dx[i];
					skip = false;
					continue;
				}

				for (int j = 0; j < area.Length; j++)
				{
					if (area[j][0] <= c && c <= area[j][1])
					{
						skip = true;
						break;
					}
				}

				dx[n++] = dx[i];
			}

			short[] ndx = new short[n];
			Array.Copy(dx, 0, ndx, 0, n);

			return ndx;
		}
	}
}