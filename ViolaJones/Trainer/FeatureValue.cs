using System;
namespace ViolaJones
{
	/// <summary>
	/// Structure utilisée lors de l'entrainement du classifieur.
	/// </summary>
	public struct FeatureValue
		: IComparable<FeatureValue>
	{
		public short TestIndex;
		public int Value;
		
		public FeatureValue(short TestIndex, int Value)
		{
			this.TestIndex = TestIndex;
			this.Value = Value;
		}
		
		public int CompareTo(FeatureValue Other)
		{
			return this.Value - Other.Value;
		}
		
		/// <summary>
		/// Calcule les valeurs de la caractéristique pour tous les tests.
		/// </summary>
		public static FeatureValue[] ComputeAllValues(TestImage[] Tests, IFeature Feature)
		{
			var values = new FeatureValue[Tests.Length];
			
			for (var iTest = 0; iTest < Tests.Length; iTest++) {
				var featureValue = Feature.ComputeValue(Tests[iTest].Image);
				
				// Normalise the value by the deviation
				values[iTest]= new FeatureValue((short) iTest,
					Features.NormalizeFeature(featureValue, Tests[iTest].Derivation));
			} 
			
			return values;
		}
		
		/// <summary>
		/// Calcule les valeurs de la caractéristique pour tous les tests.
		/// Trie les valeurs par ordre croissant.
		/// </summary>
		public static FeatureValue[] ComputeAllValuesSorted(TestImage[] Tests, IFeature Feature)
		{
			var values = ComputeAllValues(Tests, Feature);
			
			Array.Sort(values);
			
			return values;
		}
	}
}

