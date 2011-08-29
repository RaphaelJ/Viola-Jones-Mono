using System;
using System.Collections;
using System.Collections.Generic;

namespace ViolaJones
{
	public struct ThreeHorizontalRectanglesFeature
		: IFeature
	{
		public const int minWidth = 3;
		public const int minHeight = 1;
		
		public Rectangle Frame { get; private set; }
		
		public ThreeHorizontalRectanglesFeature(Rectangle Frame)
		{
			this.Frame = Frame;
		}
		
		public int ComputeValue(Point WinTopLeft, float SizeRatio, IntegralImage Image)
		{
			/*
			 * a ------- b ------- c ------- d
			 * -         -         -         -
			 * -   R1    -    R2   -    R3   -
			 * -         -         -         -
			 * e ------- f ------- g ------- h
			 * S(R1) = f - (b + e) + a
			 * S(R2) = g - (c + f) + b
			 * S(R3) = h - (d + g) + c
			 */
			
			var scaledFrame = this.Frame.Scale(SizeRatio);
			var topLeft = scaledFrame.TopLeft.NestedPoint(WinTopLeft);
			var rectsWidth = scaledFrame.Width / 3;
			var rectsHeight = scaledFrame.Height;
			
			var aCoords = topLeft;
			var bCoords = aCoords.Translate(rectsWidth, 0);
			var cCoords = bCoords.Translate(rectsWidth, 0);
			var dCoords = cCoords.Translate(rectsWidth, 0);
			
			var eCoords = aCoords.Translate(0, rectsHeight);
			var fCoords = eCoords.Translate(rectsWidth, 0);
			var gCoords = fCoords.Translate(rectsWidth, 0);
			var hCoords = gCoords.Translate(rectsWidth, 0);
			
			var a = Image.GetValue(aCoords);
			var b = Image.GetValue(bCoords);
			var c = Image.GetValue(cCoords);
			var d = Image.GetValue(dCoords);
			var e = Image.GetValue(eCoords);
			var f = Image.GetValue(fCoords);
			var g = Image.GetValue(gCoords);
			var h = Image.GetValue(hCoords);
			
			var sumR1 = f - (b + e) + a;
			var sumR2 = g - (c + f) + b;
			var sumR3 = h - (d + g) + c;
			
			return (int) (sumR1 - sumR2 + sumR3);
		}
		public int ComputeValue(IntegralImage Image)
		{
			var topLeft = new Point(0, 0);
			return this.ComputeValue(topLeft, 1f, Image);
		}
		
		public static IEnumerable<IFeature> ListFeatures()
		{
			foreach (var rect in Window.ListFeaturePositions(minWidth, minHeight)) {
				yield return new ThreeHorizontalRectanglesFeature(rect);
			}
		}
	}
}