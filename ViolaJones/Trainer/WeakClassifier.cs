using System;

namespace ViolaJones
{
	public struct WeakClassifier
	{
		/// <summary>
		/// Représente le poids du classificateur dans le classificateur
		/// fort.
		/// </summary>
		public readonly double Alpha;
		
		public readonly int Threshold;
		public readonly sbyte Parity;
		
		public readonly IFeature Feature;
		
		public WeakClassifier(double Alpha, int Threshold, sbyte Parity, IFeature Feature)
		{
			this.Alpha = Alpha;
			this.Threshold = Threshold;
			this.Parity = Parity;
			this.Feature = Feature;
		}
		
		/// <summary>
		/// Retourne true si le classificateur vérifie la fenêtre.
		/// </summary>
		public bool Check(Window Win, IntegralImage Image)
		{
			var featureValue = this.Feature.ComputeValue(Win.TopLeft, Win.SizeRatio, Image);
			var sizedValue = (int) (featureValue / (Win.SizeRatio * Win.SizeRatio));
			var normalizedValue	= Features.NormalizeFeature(sizedValue, Win.Deviation);
			
			return this.Parity * normalizedValue < this.Parity * this.Threshold;
		}
		
		/// <summary>
		/// Retourne le poids du classificateur si celui-ci vérifie
		/// la fenêtre, 0 sinon.
		/// </summary>
		public double GetValue(Window Win, IntegralImage Image)
		{
			if (this.Check(Win, Image))
				return this.Alpha;
			else
				return 0;
		}
	}
}