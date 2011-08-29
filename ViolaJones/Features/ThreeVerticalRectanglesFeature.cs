using System;
using System.Collections;
using System.Collections.Generic;

namespace ViolaJones
{
	public struct ThreeVerticalRectanglesFeature
		: IFeature
	{
		public const int minWidth = 1;
		public const int minHeight = 3;
		
		public Rectangle Frame { get; private set; }
		
		public ThreeVerticalRectanglesFeature(Rectangle Frame)
		{
			this.Frame = Frame;
		}
		
		public int ComputeValue(Point WinTopLeft, float SizeRatio, IntegralImage Image)
		{
			/*
			 * a ------- b
			 * -         -
			 * -   R1    -
			 * -         -
			 * c ------- d
			 * -         -
			 * -   R2    -
			 * -         -
			 * e ------- f
			 * -         -
			 * -   R3    -
			 * -         -
			 * g ------- h
			 * S(R1) = d - (b + c) + a
			 * S(R2) = f - (d + e) + c
			 * S(R3) = h - (f + g) + e
			 */
			
			var scaledFrame = this.Frame.Scale(SizeRatio);
			var topLeft = scaledFrame.TopLeft.NestedPoint(WinTopLeft);
			var rectsWidth = scaledFrame.Width;
			var rectsHeight = scaledFrame.Height / 3;
			
			var aCoords = topLeft;
			var bCoords = aCoords.Translate(rectsWidth, 0);
			
			var cCoords = aCoords.Translate(0, rectsHeight);
			var dCoords = cCoords.Translate(rectsWidth, 0);
			
			var eCoords = cCoords.Translate(0, rectsHeight);
			var fCoords = eCoords.Translate(rectsWidth, 0);
			
			var gCoords = eCoords.Translate(0, rectsHeight);
			var hCoords = gCoords.Translate(rectsWidth, 0);
			
			var a = Image.GetValue(aCoords);
			var b = Image.GetValue(bCoords);
			var c = Image.GetValue(cCoords);
			var d = Image.GetValue(dCoords);
			var e = Image.GetValue(eCoords);
			var f = Image.GetValue(fCoords);
			var g = Image.GetValue(gCoords);
			var h = Image.GetValue(hCoords);
			
			var sumR1 = d - (b + c) + a;
			var sumR2 = f - (d + e) + c;
			var sumR3 = h - (f + g) + e;
			
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
				yield return new ThreeVerticalRectanglesFeature(rect);
			}
		}
	}
}