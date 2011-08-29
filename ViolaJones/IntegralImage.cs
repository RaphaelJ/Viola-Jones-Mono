using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ViolaJones
{
	/// <summary>
	/// Permet de manipuler une image intégrale.
	/// http://en.wikipedia.org/wiki/Summed_area_table
	/// </summary>
	public class IntegralImage
	{
		public readonly long[,] Table;
		public readonly int Width;
		public readonly int Height;
		
		public readonly GreyPixbuf Image;
		
		/// <summary>
		/// Type d'image intégrale (identitaire, carrée, ...)
		/// </summary>
		public readonly Func<byte, long> Type;
		
		/// <summary>
		/// Image de debug.
		/// </summary>
		public Gdk.Pixbuf Pixbuf {
			get {
				return this.ToPixbuf();
			}
		}
		
		#region Constructors
		public IntegralImage(GreyPixbuf Image, Func<byte, long> Type)
		{
			this.Width = Image.Width;
			this.Height = Image.Width;
			
			this.Image = Image;
			
			this.Table = new long[this.Height, this.Width];
			
			this.Type = Type;
			
			this.ComputeIntegralImage();
		}
		public IntegralImage(GreyPixbuf Image)
			: this(Image, (pix) => pix)
		{
		}
		public IntegralImage(Gdk.Pixbuf Image, Func<byte, long> Type)
			: this(new GreyPixbuf(Image), Type)
		{
		}
		public IntegralImage(Gdk.Pixbuf Image)
			: this(new GreyPixbuf(Image), (pix) => pix)
		{
		}
		public IntegralImage(string Path, Func<byte, long> Type)
			: this(new Gdk.Pixbuf(Path), Type)
		{
		}
		public IntegralImage(string Path)
			: this(new Gdk.Pixbuf(Path), (pix) => pix)
		{
		}
		#endregion
		
		/// <summary>
		/// Donne la valeur d'un point de la table.
		/// </summary>
		public long GetValue(Point Coords)
		{
			if (Coords.X == 0 || Coords.Y == 0)
				return 0L;
			else
				return this.Table[Coords.Y - 1, Coords.X - 1];
		}
				
		/// <summary>
		/// Calcule la dispertion des valeurs des pixels d'une image.
		/// </summary>
		public int GetDeviation()
		{
			// deriv = sqrt(sum(pixs²) / nPixs - avg(pixs))
			var nPixs = this.Width * this.Height;
			
			long squaredSum = 0L;
			foreach (var pixVal in this.Image.Pixels)
				squaredSum += pixVal*pixVal;
	
			var sum = this.GetValue(new Point(this.Width, this.Height));
			var avg = sum / nPixs;
			
			var variance = squaredSum / nPixs - avg*avg;
			
			// Min 1 to remove division by zero
			if (variance > 0)
				return (int) Math.Sqrt(variance);
			else
				return 1;
		}
		
		/// <summary>
		/// Génère un aperçu de la table sous la forme d'une image.
		/// En vert, la table.
		/// En blue, l'image.
		/// </summary>
		public unsafe Gdk.Pixbuf ToPixbuf()
		{
			var output = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, false, 8, this.Width, this.Height);
			
			fixed (long* pTable = this.Table) {	
				fixed (byte* pImage = this.Image.Pixels) {
					// Sum of pixels values (Table's last value)
					long sum = Math.Max(1 /* Division by 0 */, pTable[this.Table.Length - 1]);
					
					byte* pOutput = (byte*) output.Pixels;
					var outputNChannels = output.NChannels;
					var outputLineWidth = output.Rowstride;
					
					for (var ln = 0; ln < this.Width; ln++) {	
						for (var col = 0; col < this.Height; col++) {
							byte* ipOutput = pOutput + ln * outputLineWidth + col * outputNChannels;
							byte imageValue = pImage[ln * this.Width + col];
							byte tableValue = (byte) (pTable[ln * this.Width + col] * 255 / sum);
							
							ipOutput[1] = tableValue; // Green show the table
							ipOutput[2] = imageValue; // Blue show the image
							ipOutput[0] = 0;
						}
					}
				}
			}
				
			return output;
		}
		
		#region Integral image computation
		/// <summary>
		/// Génère une table représentant une image intégrale.
		/// Chaque element de la table représente la valeur du pixel additionée
		/// à la somme des valeur des pixels au dessus et à gauche.
		/// </summary>
		protected unsafe void ComputeIntegralImage()
		{
			fixed (long* pTable = this.Table) {
				fixed (byte* pImage = this.Image.Pixels) {
					// Top left pixel
					*pTable = this.Type(*pImage);
										
					// First line
					long* pLastTable = pTable + this.Width; // last + 1
					long* ipTable = pTable + 1;
					byte* ipImage = pImage + 1;
					while (ipTable < pLastTable) {
						*ipTable = *(ipTable - 1) + this.Type(*(ipImage));
						
						ipTable++;
						ipImage++;
					}
					
					// First column
					pLastTable = pTable + this.Table.Length; // last + 1
					ipTable = pTable + this.Width;
					ipImage = pImage + this.Width;
					while (ipTable < pLastTable) {
						*ipTable = *(ipTable - this.Width) + this.Type(*(ipImage));
						
						ipTable++;
						ipImage++;
					}
					
					// Others pixels
					for (var iLn = 1; iLn < this.Height; iLn++) {
						for (var iCol = 1; iCol < this.Width; iCol++) {
							pTable[iLn * this.Width + iCol] = pTable[iLn * this.Width + (iCol - 1)]
							                 + pTable[(iLn - 1) * this.Width + iCol]
							                 + this.Type(pImage[iLn * this.Width + iCol])
											 - pTable[(iLn - 1) * this.Width + iCol - 1];
						}
					}
				}
			}
		}
		#endregion
	}
}