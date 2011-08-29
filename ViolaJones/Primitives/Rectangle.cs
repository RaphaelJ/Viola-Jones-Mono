using System;

namespace ViolaJones
{
	/// <summary>
	/// Représente un rectangle.
	/// Utilisé dans les caractéristiques.
	/// </summary>
	public struct Rectangle
	{
		public readonly Point TopLeft;
		
		public readonly int Width;
		public readonly int Height;
		
		public Rectangle(Point TopLeft, int Width, int Height)
		{
			this.TopLeft = TopLeft;
			this.Width = Width;
			this.Height = Height;
		}
		
		/// <summary>
		/// Retourne un nouveau rectangle dont les coordonnées sont celles du
		/// rectangle en cours relative à celle d'un rectangle parent.
		/// </summary>
		public Rectangle NestedRectangle(Rectangle Parent)
		{
			return new Rectangle(this.TopLeft.NestedPoint(Parent.TopLeft),
			                     this.Width, this.Height);
		}
		
		/// <summary>
		/// Applique une transformation d'échelle au rectangle.
		/// </summary>
		public Rectangle Scale(float SizeRatio)
		{
			var scaledTopLeft = this.TopLeft.Scale(SizeRatio);
			return new Rectangle(scaledTopLeft, (int) (this.Width * SizeRatio),
			                     (int) (this.Height * SizeRatio));
			                    
		}
		
		/// <summary>
		/// Dessine le rectangle sur l'image
		/// </summary>
		public unsafe void Draw(Gdk.Pixbuf Image)
		{
			var nChannels = Image.NChannels;
			var lineWidth = Image.Rowstride;
			byte* pixs = (byte*) Image.Pixels;
			
			for (var ln = this.TopLeft.Y; ln <  this.TopLeft.Y + this.Height; ln++) {	
				for (var col = this.TopLeft.X; col <  this.TopLeft.X + this.Width; col++) {
					pixs[ln * lineWidth + col * nChannels] = 255;
				}
			}
		}
		
		public override string ToString ()
		{
			return string.Format("{0} Width : {1} Height : {2}",
			                     this.TopLeft, this.Width, this.Height);
		}
	}
}

