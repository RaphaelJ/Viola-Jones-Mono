using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ViolaJones
{
	/// <summary>
	/// Défini une fenêtre d'itaration.
	/// </summary>
	public struct Window
	{
		public readonly Point TopLeft;
		
		public readonly float SizeRatio;
		
		public readonly int Deviation;
		
		public int Width {
			get {
				return (int) (Config.WindowWidth * SizeRatio);
			}
		}
		public int Height {
			get {
				return (int) (Config.WindowWidth * SizeRatio);
			}
		}
		
		public Window(Point TopLeft, float SizeRatio, IntegralImage Image, IntegralImage SquaredImage)
		{
			this.TopLeft = TopLeft;
			
			this.SizeRatio = SizeRatio;
			
			this.Deviation = 0;
			this.Deviation = this.GetDeviation(Image, SquaredImage);
		}
		public Window(Point TopLeft, IntegralImage Image, IntegralImage SquaredImage)
			: this(TopLeft, 1f, Image, SquaredImage)
		{
		}
		
		/// <summary>
		/// Retourne la dispertion de la valeur des pixels
		/// contenus dans la fenêtre/
		/// </summary>
		private int GetDeviation(IntegralImage Image, IntegralImage SquaredImage)
		{
			/*
			 * a -------- b
			 * -          -
			 * -  Window  -
			 * -          -
			 * c -------- d
			 * 
			 * sum = d - (b + c) + a
			 * avg = sum / nPixs
			 * deriv = sqrt(sum(pixs²) / nPixs - avg(pixs))
			 */
			
			var nPixs = this.Width * this.Height;
			
			var aCoords = this.TopLeft;
			var bCoords = aCoords.Translate(this.Width, 0);
			
			var cCoords = aCoords.Translate(0, this.Height);
			var dCoords = cCoords.Translate(this.Width, 0);
			
			var a = Image.GetValue(aCoords);
			var b = Image.GetValue(bCoords);
			var c = Image.GetValue(cCoords);
			var d = Image.GetValue(dCoords);
			
			var squaredA = SquaredImage.GetValue(aCoords);
			var squaredB = SquaredImage.GetValue(bCoords);
			var squaredC = SquaredImage.GetValue(cCoords);
			var squaredD = SquaredImage.GetValue(dCoords);
			
			var sum = d - (b + c) + a;
			var squaredSum = squaredD - (squaredB + squaredC) + squaredA;
			
			var avg = sum / nPixs;
			
			var variance = squaredSum / nPixs - avg*avg;
			
			// Min 1 to remove division by zero
			if (variance > 0)
				return (int) Math.Sqrt(variance);
			else
				return 1;
		}
		
		public static IEnumerable<Window> ListWindows(IntegralImage Image, IntegralImage SquaredImage)
		{
			var maxX = Image.Width - Config.WindowWidth;
			var maxY = Image.Height - Config.WindowHeight;
			
			for (var x = 0; x <= maxX; x += Config.WindowDX) {
				for (var y = 0; y <= maxY; y += Config.WindowDY) {
					var maxWidth = Image.Width - x;
					var maxHeight = Image.Height - y;
					
					var width = Config.WindowWidth;
					var height = Config.WindowHeight;
					var ratio = 1f;
					
					while (width <= maxWidth && height <= maxHeight) {
						yield return new Window(new Point(x, y), ratio, Image, SquaredImage);
						
						ratio *= Config.WindowScale;
						width = (int) (Config.WindowWidth * ratio);
						height = (int) (Config.WindowHeight * ratio);
					}
				}
			}
		}
		
		#region Features generators
		/// <summary>
		/// Liste l'ensemble des positions auxquelles la caractéristique de taille
		/// donnée peut être placée dans une fenêtre d'apprentissage (24x24).
		/// </summary>
		public static IEnumerable<Rectangle> ListFeaturePositions(int minWidth, int minHeight)
		{
			var maxX = Config.WindowWidth - minWidth;
			var maxY = Config.WindowHeight - minHeight;
			
			for (var x = 0; x <= maxX; x += Config.FeatureDX) { // Each line
				for (var y = 0; y <= maxY; y += Config.FeatureDY) { // Each column
					int maxWidth = Config.WindowWidth - x;
					for (var width = minWidth; width <= maxWidth; width += minWidth) { // Each width
						int maxHeight = Config.WindowHeight - y;
						for (var height = minHeight; height <= maxHeight; height += minHeight) { // Each height
							yield return new Rectangle(new Point(x, y), width, height);
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Liste l'ensemble des caractéristiques pouvant être placée dans une
		/// fenêtre d'apprentissage (24x24).
		/// </summary>
		public static IEnumerable<IFeature> ListFeatures()
		{
			foreach (var feature in TwoHorizontalRectanglesFeature.ListFeatures())
				yield return feature;
			
			foreach (var feature in TwoVerticalRectanglesFeature.ListFeatures())
				yield return feature;
			
			foreach (var feature in ThreeHorizontalRectanglesFeature.ListFeatures())
				yield return feature;
			
			foreach (var feature in ThreeVerticalRectanglesFeature.ListFeatures())
				yield return feature;
			
			foreach (var feature in FourRectanglesFeature.ListFeatures())
				yield return feature;
		}
		#endregion
		
		public Rectangle ToRectangle()
		{
			return new Rectangle(this.TopLeft, this.Width, this.Height);
		}
	}
}