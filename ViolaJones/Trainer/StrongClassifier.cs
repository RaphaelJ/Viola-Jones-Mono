using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ViolaJones
{
	public struct StrongClassifier
	{
		public readonly WeakClassifier[] Classifiers;
		
		public readonly double GlobalAlpha;
		
		public StrongClassifier(WeakClassifier[] Classifiers)
		{
			this.Classifiers = Classifiers;
			
			Func<double, WeakClassifier, double> sum = (acc, classifier) =>
				acc + classifier.Alpha;
			
			this.GlobalAlpha = this.Classifiers.Aggregate(0.0, sum);
		}
		
		/// <summary>
		/// Retourne true si le classificateur vérifie la fenêtre.
		/// La somme des poids des 
		/// </summary>
		public bool Check(Window Win, IntegralImage Image)
		{
			double sumValues = 0.0;
			foreach (var weakClassifier in this.Classifiers)
				sumValues += weakClassifier.GetValue(Win, Image);
			
			return sumValues >= this.GlobalAlpha / 2.0;
		}
		
		#region Saving and restoring from a file
		/// <summary>
		/// Enregistre le classifieur dans un fichier texte
		/// </summary>
		public void Save(string Path)
		{
			var f = File.CreateText(Path);
			
			foreach (var classifier in this.Classifiers)  {
				f.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7}",
				            classifier.Alpha, classifier.Threshold,
				            classifier.Parity, classifier.Feature.GetType(),
				            classifier.Feature.Frame.TopLeft.X,
				            classifier.Feature.Frame.TopLeft.Y, 
				            classifier.Feature.Frame.Width,	classifier.Feature.Frame.Height);
			}
			
			f.Close();
		}
		
		/// <summary>
		/// Charge la définition d'un classifieur depuis un fichier
		/// </summary>
		public static StrongClassifier LoadFromFile(string Path)
		{
			var asm = Assembly.GetExecutingAssembly();
			
			Func<string, WeakClassifier> RestoreClassifier = (str) => {
				string[] vals = str.Split(';');
				
				var alpha = double.Parse(vals[0]);
				var threshold = int.Parse(vals[1]);
				var parity = sbyte.Parse(vals[2]);
				
				var featureType = asm.GetType(vals[3]);
				//if (!(featureType is IFeature)) { Console.WriteLine(featureType);
				//	throw new Exception("Invalid feature type"); }
				
				var featureX = int.Parse(vals[4]);
				var featureY = int.Parse(vals[5]);
				var featureWidth = int.Parse(vals[6]);
				var featureHeight = int.Parse(vals[7]);
				var featureFrame = new Rectangle(new Point(featureX, featureY),
				                                 featureWidth, featureHeight);
				
				var feature = (IFeature) Activator.CreateInstance(featureType, featureFrame);
				
				return new WeakClassifier(alpha, threshold, parity, feature);
			};
			
			var classifiers = File.ReadAllLines(Path).Select(RestoreClassifier)
			                                         .ToArray();
			
			return new StrongClassifier(classifiers);
		}
		
		#endregion
		
		#region Trainer
		/// <summary>
		/// Construit un arbre de détection à partir d'un dossier
		/// contenant des images de test valides et non valides.
		/// </summary>
		public static StrongClassifier Train(string TestsDir)
		{
			Console.WriteLine("Init trainer ...");
			
			// Load default weights, integral images and compute features
			// values foreach test image of the tests dirs.
			// Training tests are resized to the window size.
			Console.WriteLine("Load trainer tests ...");
			var start = DateTime.Now;
			var testsSet = LoadTestsSet(TestsDir);
			var tests = testsSet.Item1;
			var featuresValues = testsSet.Item2;
			
			var nCachedFeatures = featuresValues.Where((feature) => feature.Values != null)
			                                   .Count();
			
			Console.WriteLine("{0} tests loaded in {1} secs ({2}% cached)", tests.Length,
			                  (DateTime.Now - start).TotalSeconds,
			                  nCachedFeatures * 100 / featuresValues.Length);
			
			Console.WriteLine("Ok. Let's start the neurons lazer powered firing machine ...");
			
			WeakClassifier[] classifiers = new WeakClassifier[Config.LearnPass];
			var iPass = 1;
			while (iPass <= Config.LearnPass) {
				start = DateTime.Now;
				Console.WriteLine("{0}/{1} trainer pass ...", iPass, Config.LearnPass);
				
				// Normalize the weights of the images to get
				// a rational distribution
				var weightsSum = tests.Aggregate(0.0, (acc, test) => test.Weight + acc);
				var validWeight = 0.0;
				for (var iTest = 0; iTest < tests.Length; iTest++) {
					tests[iTest].Weight = tests[iTest].Weight / weightsSum;
					
					if (tests[iTest].Valid)
						validWeight += tests[iTest].Weight;
				}
				
				// Select the feature with the lowest error level for the new
				// set of tests's weights
				TestWeakClassifier best = new TestWeakClassifier(featuresValues[0], 0, 1, double.MaxValue);
				//foreach (var feature in featuresValues) {
				Parallel.ForEach(featuresValues, (feature) => {
					var newClassifier = TestWeakClassifier.Train(tests, validWeight, feature);
					
					if (best.Errors > newClassifier.Errors)
					    best = newClassifier;
				});
				//}
				
				Console.WriteLine("New weak classifier selected in {0} secs (error score: {1})",
				                  (DateTime.Now - start).TotalSeconds, best.Errors);
				Console.WriteLine("X: {0} Y: {1} - Width : {2} Height : {3}",
				                  best.Feature.Feature.Frame.TopLeft.X,
				                  best.Feature.Feature.Frame.TopLeft.Y,
				                  best.Feature.Feature.Frame.Width,
				                  best.Feature.Feature.Frame.Height);
					
				// Update the weights
				var beta = best.Errors / (1.0 - best.Errors);
				if (beta < 1e-8) // Unallow alpha to be > 18
					beta = 1e-8;
				
				// Reduce the weights of valid checks				
				foreach (var featureValue in best.Feature.Values) {
					if (best.Check(featureValue.Value) == tests[featureValue.TestIndex].Valid)
						tests[featureValue.TestIndex].Weight *= beta;
				}
				
				// Add the new weak classifier to the strong classifier
				var alpha = Math.Log(1.0 / beta); // Weak classifier weight inside the strong classifier
				classifiers[iPass - 1] = best.GetClassifier(alpha);
				
				iPass += 1;
			}
			
			Console.WriteLine("Detector's brain ready to go");
			
			return new StrongClassifier(classifiers);
		}
		#endregion
		
		#region Window's scaled images loader
		/// <summary>
		/// Charge un ensemble d'images de test d'apprentissage, initialise
		/// les poids des tests et calcule les valeurs de chaque caractéristique.
		/// </summary>
		private static Tuple<TestImage[], FeatureValues[]> LoadTestsSet(string TestsDir)
		{
			var goodDir = Path.Combine(TestsDir, "good");
			var badDir = Path.Combine(TestsDir, "bad");

			var good = LoadImages(goodDir);
			var bad = LoadImages(badDir);
			
			// Init default weights values
			var goodWeight = 1.0 / (2 * good.Length);
			var badWeight = 1.0 / (2 * bad.Length);
			
			var tests = new TestImage[good.Length + bad.Length];
			
			// Save default weights and status (valid/invalid) and
			// fusion good & bad sets
			for (var i = 0; i < good.Length; i++) {
				tests[i] = new TestImage(good[i], goodWeight, true);
			}
			for (var i = good.Length; i < good.Length + bad.Length; i++) {
				tests[i] = new TestImage(bad[i - good.Length], badWeight, false);
			}
			
			// Compute features's values
			var featuresValues = ComputeFeaturesValues(tests);
			
			return Tuple.Create(tests, featuresValues);
		}
		
		/// <summary>
		/// Retourne un tableau a deux dimensions contenenant les valeurs
		/// calculées de toutes les caractéristiques pour tous les tests.
		/// int[feature,test].
		/// </summary>
		private static FeatureValues[] ComputeFeaturesValues(TestImage[] Tests)
		{
			// TODO: Windows compatibility
			if (Environment.OSVersion.Platform != PlatformID.Unix)
					throw new NotImplementedException();
			
			Func<int> AvailableMemory = () =>
			{
				return File.ReadAllLines("/proc/meminfo")
					.Where(elem =>
						elem.StartsWith("MemFree")
						|| elem.StartsWith("Buffers")
						|| elem.StartsWith("Cached"))
					.Select(elem => elem.Split(' '))
					.Aggregate(0, (acc, values) => acc + int.Parse(values[values.Length - 2]));
			};
			
			// List all features of a standard 24x24 window
			var features = Window.ListFeatures().ToArray();
			
			var featuresValues = new FeatureValues[features.Length];
			
			Parallel.For(0, features.Length, (iFeature) => {
			//for (var iFeature = 0; iFeature < FeaturesList.Length; iFeature++) {				
				if (AvailableMemory() > Config.MinFreeMemory) {
					var values = FeatureValue.ComputeAllValuesSorted(Tests, features[iFeature]);
					
					featuresValues[iFeature] = new FeatureValues(features[iFeature], values);
				} else
					featuresValues[iFeature] = new FeatureValues(features[iFeature], null);				
			});
			//}
			
			return featuresValues;
		}
	
		/// <summary>
		/// Charge une image de test.
		/// </summary>
		private static GreyPixbuf LoadImage(string ImagePath)
		{
			var pixbuf = new Gdk.Pixbuf(ImagePath);
			
			var scaledPixbuf = pixbuf.ScaleSimple(Config.WindowWidth, Config.WindowHeight,
			                                      Gdk.InterpType.Hyper);
			pixbuf.Dispose();
			
			return new GreyPixbuf(scaledPixbuf);
		}
		
		/// <summary>
		/// Charge l'ensemble des images d'un dossier de test.
		/// </summary>
		private static GreyPixbuf[] LoadImages(string Dir)
		{
			string[] filenames = Directory.GetFiles(Dir);
			
			var images = new GreyPixbuf[filenames.Length];
			
			Parallel.For(0, filenames.Length, (i) => {
				images[i] = LoadImage(filenames[i]);
			});
			return images;
		}
		#endregion
	}
}