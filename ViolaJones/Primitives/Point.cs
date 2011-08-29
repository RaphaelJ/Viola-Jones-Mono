using System;
namespace ViolaJones
{
	public struct Point
	{
		public readonly int X;
		public readonly int Y;
		
		public Point(int X, int Y)
		{
			this.X = X;
			this.Y = Y;
		}
		
		/// <summary>
		/// Retourne un nouveau point dont les coordonnées sont celles du
		/// point en cours relative à celle d'un point parent.
		/// </summary>
		public Point NestedPoint(Point Parent)
		{
			return new Point(this.X + Parent.X, this.Y + Parent.Y);
		}
		
		/// <summary>
		/// Retourne un nouveau point qui a subit la translation.
		/// </summary>
		public Point Translate(int DX, int DY)
		{
			return new Point(this.X + DX, this.Y + DY);
		}
		
		/// <summary>
		/// Applique une transformation d'échelle au point.
		/// </summary>
		public Point Scale(float SizeRatio)
		{
			return new Point((int) (this.X * SizeRatio), (int) (this.Y * SizeRatio));
		}
		
		public override string ToString ()
		{
			return string.Format ("X : {0} Y : {1}", this.X, this.Y);
		}
	}
}

