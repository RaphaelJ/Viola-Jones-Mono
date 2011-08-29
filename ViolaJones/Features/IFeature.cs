using System;
using System.Collections;
using System.Collections.Generic;

namespace ViolaJones
{
	/// <summary>
	/// Interface dont dérivent les différentes caractéristiques.
	/// </summary>
	public interface IFeature
	{
		/// <summary>
		/// Position dans la fenêtre
		/// </summary>
		Rectangle Frame { get; }
		
		/// <summary>
		/// Calcule la valeur de la caractéristique dans une fenêtre et
		/// une image intégrale.
		/// </summary>
		int ComputeValue(Point WinTopLeft, float SizeRatio, IntegralImage Image);
		
		/// <summary>
		/// Calcule la valeur de la caractéristique dans une image
		/// d'apprentissage.
		/// </summary>
		int ComputeValue(IntegralImage Image);
	}
}