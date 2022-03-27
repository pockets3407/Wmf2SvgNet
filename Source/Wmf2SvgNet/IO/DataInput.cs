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
using System.IO;

namespace Wmf2SvgNet.IO
{

	public class DataInput
	{
		private Stream inputStream;
		private bool littleEndian = BitConverter.IsLittleEndian;

		private byte[] buf = new byte[4];
		private int count = 0;

		/// <summary>
		/// Create a DataInput class instance using by native order. 
		/// </summary>
		/// <param name="inputStream">the input stream that had better buffer by a BufferedInputStream.</param>
		public DataInput(Stream inputStream)
		{
			littleEndian = BitConverter.IsLittleEndian;
			this.inputStream = inputStream;
		}

		/// <summary>
		/// Create a DataInput class instance.
		/// </summary>
		/// <param name="inputStream">the input stream that had better buffer by a BufferedInputStream.</param>
		/// <param name="littleEndian">the endian of the input stream </param>
		public DataInput(Stream inputStream, bool littleEndian)
		{
			this.littleEndian = littleEndian;
			this.inputStream = inputStream;
		}

		/// <summary>
		/// Reads the next one byte of this input stream as a signed 8-bit integer.
		/// </summary>
		/// <returns>the <code>int</code> value as a signed 8-bit integer.</returns>
		/// <exception cref="EndOfStreamException"></exception>
		/// <exception cref="IOException"></exception>
		public byte ReadByte()
		{
			int b = inputStream.ReadByte();
			if (b < 0)
				throw new EndOfStreamException();
			count += 1;
			return (byte)b;
		}

		/// <summary>
		/// Reads the next two bytes of this input stream as a signed 16-bit integer.
		/// </summary>
		/// <returns>the <code>int</code> value as a signed 16-bit integer.</returns>
		/// <exception cref="EndOfStreamException"></exception>
		/// <exception cref="IOException"></exception>
		public short ReadInt16()
		{
			if (inputStream.Read(buf, 0, 2) == 2)
			{
				count += 2;
				if (BitConverter.IsLittleEndian && littleEndian)
					return BitConverter.ToInt16(buf, 0);
				else
				{
					short value = 0;
					if (!littleEndian)
					{
						value |= (short)(0xff & buf[1]);
						value |= (short)((0xff & buf[0]) << 8);
					}
					else
					{
						value |= (short)(0xff & buf[0]);
						value |= (short)((0xff & buf[1]) << 8);
					}
					return value;
				}
			}
			throw new EndOfStreamException();
		}

		/// <summary>
		/// Reads the next four bytes of this input stream as a signed 32-bit integer.
		/// </summary>
		/// <returns>the <code>int</code> value as a signed 32-bit integer.</returns>
		/// <exception cref="EndOfStreamException"></exception>
		/// <exception cref="IOException"></exception>
		public int ReadInt32()
		{
			if (inputStream.Read(buf, 0, 4) == 4) 
			{
				count += 4;
				if (BitConverter.IsLittleEndian && littleEndian)
					return BitConverter.ToInt32(buf, 0);
				else
				{
					int value = 0;
					if (!littleEndian)
					{
						value |= (0xff & buf[3]);
						value |= (0xff & buf[2]) << 8;
						value |= (0xff & buf[1]) << 16;
						value |= (0xff & buf[0]) << 24;
					}
					else
					{
						value |= (0xff & buf[0]);
						value |= (0xff & buf[1]) << 8;
						value |= (0xff & buf[2]) << 16;
						value |= (0xff & buf[3]) << 24;
					}
					return value;
				}
			}
			throw new EndOfStreamException();
		}

		/// <summary>
		/// Reads the next two bytes of this input stream as a unsigned 16-bit integer.
		/// </summary>
		/// <returns>the <code>int</code> value as a unsigned 16-bit integer.</returns>
		/// <exception cref="EndOfStreamException"></exception>
		/// <exception cref="IOException"></exception>
		public ushort ReadUint16()
		{
			if (inputStream.Read(buf, 0, 2) == 2) 
			{
				count += 2;
				if (BitConverter.IsLittleEndian && littleEndian)
					return BitConverter.ToUInt16(buf, 0);
				else
				{
					ushort value = 0;
					if (!littleEndian)
					{
						value |= (ushort)(0xff & buf[1]);
						value |= (ushort)((0xff & buf[0]) << 8);
					}
					else
					{
						value |= (ushort)(0xff & buf[0]);
						value |= (ushort)((0xff & buf[1]) << 8);
					}
					return value;
				}
			}
			throw new EndOfStreamException();
		}

		/// <summary>
		/// Reads the next four bytes of this input stream as a unsigned 32-bit integer.
		/// </summary>
		/// <returns>the <code>long</code> value as a unsigned 32-bit integer.</returns>
		/// <exception cref="EndOfStreamException"></exception>
		/// <exception cref="IOException"></exception>
		public uint ReadUint32()
		{
			if (inputStream.Read(buf, 0, 4) == 4)
			{
				count += 4;
				if (BitConverter.IsLittleEndian && littleEndian)
					return BitConverter.ToUInt32(buf, 0);
				else
				{
					uint value = 0;
					if (!littleEndian)
					{
						value |= (uint)(0xff & buf[3]);
						value |= (uint)((0xff & buf[2]) << 8);
						value |= (uint)((0xff & buf[1]) << 16);
						value |= (uint)((0xff & buf[0]) << 24);
					}
					else
					{
						value |= (uint)(0xff & buf[0]);
						value |= (uint)((0xff & buf[1]) << 8);
						value |= (uint)((0xff & buf[2]) << 16);
						value |= (uint)((0xff & buf[3]) << 24);
					}
					return value;
				}
			}
			throw new EndOfStreamException();
		}

		public byte[] ReadBytes(int n)
		{
			byte[] array = new byte[n];
			if (inputStream.Read(array,0,array.Length) == n) {
				count += n;
				return array;
			}
			throw new EndOfStreamException();
		}

		public void SetCount(int count) 
		{
			this.count = count;
		}

		public int GetCount() 
		{
			return count;
		}

		public void Close() 
		{
			try
			{
				inputStream.Close();
			}
			catch (IOException e)
			{
				if (e.StackTrace != null)
					Console.WriteLine("Error: " + e.Message + ": " + e.StackTrace.ToString());
				else
					Console.WriteLine("Error: " + e.Message);
			}
		}
	}
}