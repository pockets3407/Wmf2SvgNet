namespace Wmf2SvgNet.Gdi
{

	public class Point
	{
		public short X { get; set; }
		public short Y { get; set; }

		public Point(short x, short y)
		{
			X = x;
			Y = y;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + X;
			result = prime * result + Y;
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
			Point other = (Point)obj;
			if (X != other.X)
				return false;
			if (Y != other.Y)
				return false;
			return true;
		}

		public override string ToString()
		{
			return "Point [x=" + X + ", y=" + Y + "]";
		}
	}
}