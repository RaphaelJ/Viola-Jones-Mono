using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViolaJones
{
	public class Detector
	{
		public readonly IntegralImage Image;
		public readonly IntegralImage SquaredImage;
		public readonly StrongClassifier Classifier;
		
		public Detector(IntegralImage Image, IntegralImage SquaredImage, StrongClassifier Classifier)
		{
			this.Image = Image;
			this.SquaredImage = SquaredImage;
			this.Classifier = Classifier;
		}
		public Detector(GreyPixbuf Image, StrongClassifier Classifier)
			: this(new IntegralImage(Image), new IntegralImage(Image, (pix) => (long) pix * pix),
			       Classifier) 
		{
		}
		public Detector(string Filename, StrongClassifier Classifier)
			: this(new GreyPixbuf(Filename), Classifier) 
		{
		}
		public Detector(Gdk.Pixbuf Image, StrongClassifier Classifier)
			: this(new GreyPixbuf(Image), Classifier) 
		{
		}
		
		/// <summary>
		/// Détecte les objets répondants aux critères du classifieur dans l'image.
		/// </summary>
		public IEnumerable<Rectangle> Detect()
		{
			Func<Window, bool> check = (win) =>
				this.Classifier.Check(win, this.Image);
			
			return Window.ListWindows(this.Image, this.SquaredImage)
			             .Where(check)
				         .Select((win) => win.ToRectangle());
		}
	}
}

