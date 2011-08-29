using System;
using System.Linq;

namespace ViolaJones
{				
	/// <summary>
	/// Structure utilisée pour les tests d'efficacité de la
	/// construction du classificateur faible.
	/// </summary>
	public struct TestWeakClassifier
	{
		public readonly FeatureValues Feature;
		
		public readonly int Threshold;
		public readonly sbyte Parity;
		
		public readonly double Errors;
		
		public TestWeakClassifier(FeatureValues Feature, int Threshold, sbyte Parity, double Errors)
		{
			this.Feature = Feature;
			
			this.Threshold = Threshold;
			this.Parity = Parity;
			
			this.Errors = Errors;
		}
		
		/// <summary>
		/// Retourne true si le classificateur valide le test.
		/// </summary>
		public unsafe bool Check(int FeatureValue)
		{
			return this.Parity * FeatureValue < this.Parity * this.Threshold;
		}
		
		/// <summary>
		/// Construit un classifieur à partir du classifieur de test.
		/// </summary>
		public WeakClassifier GetClassifier(double Alpha)
		{
			return new WeakClassifier(Alpha, this.Threshold, this.Parity, this.Feature.Feature);
		}
		/// <summary>
		/// Construit un classificateur de test en cherchant la meilleure
		/// valeur du seuil.
		/// FeatureIndex est le numero d'index de la caractéristique.
		/// </summary>
		public static TestWeakClassifier Train(TestImage[] Tests, double ValidWeight,
		                                       FeatureValues Feature)
		{
			if (Feature.Values == null) // Uncached values
				Feature.Values = FeatureValue.ComputeAllValuesSorted(Tests, Feature.Feature);
			
			// With the default values, the positive classifier says always
			// no. So it scores wrong for all valid tests.
			var positiveError = ValidWeight;
			
			// Iterate all feature's values, ascending
			var best = new TestWeakClassifier(Feature, Feature.Values[0].Value, 1, positiveError);
			
			// Select the threshold with the lowest error weight
			for (var iTest = 0; iTest < Feature.Values.Length; iTest++) {
				if (Tests[Feature.Values[iTest].TestIndex].Valid) {
					positiveError -= Tests[Feature.Values[iTest].TestIndex].Weight;
					
					if (positiveError < best.Errors) {
						best = new TestWeakClassifier(Feature, Feature.Values[iTest].Value + 1,
						                              1, positiveError);
					}
				} else {
					positiveError += Tests[Feature.Values[iTest].TestIndex].Weight;
					
					var negativeError = 1.0 - positiveError;
					
					if (negativeError < best.Errors) {
						best = new TestWeakClassifier(Feature, Feature.Values[iTest].Value - 1,
						                              -1, negativeError);
					}
				}
			}
			
			return best;
		}
	}
}

