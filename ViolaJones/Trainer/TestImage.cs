using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ViolaJones
{
	/// <summary>
	/// Représente une image de test utilisée lors des intérations
	/// de l'apprentissage. L'ensemble des valeurs des caractéristiques
	/// est returnue.
	/// </summary>
	public struct TestImage
	{
		public readonly IntegralImage Image;
		public readonly bool Valid;
		public readonly int Derivation;
		public double Weight;
		
		public TestImage(GreyPixbuf Image, double Weight, bool Valid)
		{
			this.Image = new IntegralImage(Image);
			
			this.Derivation = this.Image.GetDeviation();
			
			this.Weight = Weight;
			this.Valid = Valid;
		}
	}
}