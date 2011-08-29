using System;
namespace ViolaJones
{
	/// <summary>
	/// Structure contenant l'ensemble des valeurs calculées cachées
	/// des caractéristiques pour les tests d'apprentissage.
	/// </summary>
	public struct FeatureValues
	{
		public IFeature Feature;
		public FeatureValue[] Values;
		
		public FeatureValues(IFeature Feature, FeatureValue[] Values)
		{
			this.Feature = Feature;
			this.Values = Values;
		}
	}
}