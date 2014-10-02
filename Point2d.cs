using System;
namespace AssemblyCSharp
{
	public class Point2d
	{
		public float X;
		public float Y;

		public Point2d (float x, float y)
		{
			X = x;
			Y = y;
		}

		public override string ToString()
		{
			return "(" + X + "," + Y + ")";
		}
	}
}