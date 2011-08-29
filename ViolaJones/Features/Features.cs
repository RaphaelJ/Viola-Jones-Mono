using System;
using System.Collections;
using System.Collections.Generic;

namespace ViolaJones
{
	/// <summary>
	/// Classe statique contenant les fonctions relatives aux
	/// caractéristiques.
	/// </summary>
	public static class Features
	{
		/// <summary>
		/// Adapte la valeur d'une caractéristique à une valeur de
		/// dispertion.
		/// </summary>
		public static int NormalizeFeature(int FeatureValue, int Derivation)
		{
			// 40 is the average value of the derivation
			return (FeatureValue * 40) / Derivation;
		}
	}
}