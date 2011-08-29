using System;
using System.Collections;
using System.Collections.Generic;

namespace ViolaJones
{
	public struct FourRectanglesFeature
		: IFeature
	{
		public const int minWidth = 2;
		public const int minHeight = 2;
		
		public Rectangle Frame { get; private set; }
		
		public FourRectanglesFeature(Rectangle Frame)
		{
			this.Frame = Frame;
		}
		
		public int ComputeValue(Point WinTopLeft, float SizeRatio, IntegralImage Image)
		{
			/*
			 * a ------- b ------- c
			 * -         -         -
			 * -   R1    -    R2   -
			 * -         -         -
			 * d ------- e ------- f
			 * -         -         -
			 * -   R3    -    R4   -
			 * -         -         -
			 * g ------- h ------- i
			 * S(R1) = e - (b + d) + a
			 * S(R2) = f - (c + e) + b
			 * S(R3) = h - (e + g) + d
			 * S(R4) = i - (f + h) + e
			 */
			
			var scaledFrame = this.Frame.Scale(SizeRatio);
			var topLeft = scaledFrame.TopLeft.NestedPoint(WinTopLeft);
			var rectsWidth = scaledFrame.Width / 2;
			var rectsHeight = scaledFrame.Height / 2;
			
			var aCoords = topLeft;
			var bCoords = aCoords.Translate(rectsWidth, 0);
			var cCoords = bCoords.Translate(rectsWidth, 0);
			
			var dCoords = aCoords.Translate(0, rectsHeight);
			var eCoords = dCoords.Translate(rectsWidth, 0);
			var fCoords = eCoords.Translate(rectsWidth, 0);
			
			var gCoords = dCoords.Translate(0, rectsHeight);
			var hCoords = gCoords.Translate(rectsWidth, 0);
			var iCoords = hCoords.Translate(rectsWidth, 0);
			
			var a = Image.GetValue(aCoords);
			var b = Image.GetValue(bCoords);
			var c = Image.GetValue(cCoords);
			var d = Image.GetValue(dCoords);
			var e = Image.GetValue(eCoords);
			var f = Image.GetValue(fCoords);
			var g = Image.GetValue(gCoords);
			var h = Image.GetValue(hCoords);
			var i = Image.GetValue(iCoords);
			
			var sumR1 = e - (b + d) + a;
			var sumR2 = f - (c + e) + b;
			var sumR3 = h - (e + g) + d;
			var sumR4 = i - (f + h) + e;
			
			return (int) (sumR1 - sumR2 - sumR3 + sumR4);
		}
		public int ComputeValue(IntegralImage Image)
		{
			var topLeft = new Point(0, 0);
			return this.ComputeValue(topLeft, 1f, Image);
		}			
		
		public static IEnumerable<IFeature> ListFeatures()
		{
			foreach (var rect in Window.ListFeaturePositions(minWidth, minHeight)) {
				yield return new FourRectanglesFeature(rect);
			}
		}
	}
}