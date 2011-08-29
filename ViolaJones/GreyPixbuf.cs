using System;

namespace ViolaJones
{
	/// <summary>
	/// Stocke une image en niveaux de gris.
	/// </summary>
	public class GreyPixbuf
	{
		public byte[,] Pixels { get; protected set; }
		
		public int Width { get; protected set; }
		public int Height { get; protected set; }
		
		#region Constructors
		public GreyPixbuf(Gdk.Pixbuf Image)
		{
			this.LoadPixbuf(Image);
		}
		public GreyPixbuf(string Path) : this(new Gdk.Pixbuf(Path))
		{
		}
		#endregion
		
		protected unsafe static byte GreyValue(byte* Pix)
		{
			return (byte) (((int) Pix[0] + Pix[1] + Pix[2]) / 3);
		}
		
		/// <summary>
		/// Transforme le pixbuf couleur en image en niveaux de gris.
		/// </summary>
		protected unsafe void LoadPixbuf(Gdk.Pixbuf Image)
		{
			this.Width = Image.Width;
			this.Height = Image.Height;
			
			this.Pixels = new byte[this.Height, this.Width];
			
			fixed (byte* pDestPixs = this.Pixels) {
				byte* pSrcPixs = (byte*) Image.Pixels;
				var nChannels = Image.NChannels;
				var lineWidth = Image.Rowstride;
				
				for (var ln = 0; ln < this.Width; ln++) {	
					for (var col = 0; col < this.Height; col++) {
						pDestPixs[ln * this.Width + col]
							= GreyValue(pSrcPixs + ln * lineWidth + col * nChannels);
					}
				}
			}		
		}
		
//		/// <summary>
//		/// Applique la variance à l'image (en niveaux de gris).
//		/// </summary>
//		protected unsafe void NormaliseImage()
//		{
//			var nChannels = this.Image.NChannels;
//			var nPixels = this.Width * this.Height;
//			
//			byte* pixs = (byte*) this.Image.Pixels;
//			byte* lastPix = pixs + (nPixels * nChannels);
//			
//			// Variance := S [ (i - Avg)**2 ] / n
//			
//			// Compute avg
//			long sumPixsValues = 0;
//			for (byte* iPix = pixs; iPix <= lastPix; iPix += nChannels)
//				sumPixsValues += *iPix;
//			byte avg = sumPixsValues / nPixels;
//			
//			// Compute variance
//			int sumPixsDeviation = 0;
//			for (byte* iPix = pixs; iPix <= lastPix; iPix += nChannels)
//				sumPixsDeviations += Math.Pow(*iPix - avg, 2);
//			byte variance = sumPixsDeviation / nPixels;
//			
//			
//			
//		}
	}
}