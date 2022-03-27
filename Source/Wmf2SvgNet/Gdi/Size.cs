namespace Wmf2SvgNet.Gdi
{

	public class Size 
	{
		public short Width { get; set; }
		public short Height { get; set; }

		public Size(short width, short height) 
		{
			Width = width;
			Height = height;
		}

		public override int GetHashCode() 
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + Height;
			result = prime * result + Width;
			return result;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
				return true;
			if (obj == null)
				return false;
			if (this.GetType() != obj.GetType())
				return false;
			Size other = (Size)obj;
			if (Height != other.Height)
				return false;
			if (Width != other.Width)
				return false;
			return true;
		}

		public override string ToString()
		{
			return "Size [width=" + Width + ", height=" + Height + "]";
		}
	}
}
