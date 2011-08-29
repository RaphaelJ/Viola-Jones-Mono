using System;
using System.Collections.Generic;

namespace ViolaJones
{
	class MainClass
	{
		public static void Main (string[] args)
		{			
			Gtk.Application.Init();
			
			string method;
			if (args.Length < 1)
				method = "train";
			else
				method = args[0];
			
			StrongClassifier? classifier;
			if (method == "load")
				classifier = StrongClassifier.LoadFromFile(args[1]);
			else if (method == "train") {
				string trainingDir = args[1];
				
				classifier = StrongClassifier.Train(trainingDir);
				
				if (args.Length > 2) 
					classifier.Value.Save(args[2]);
			}
			
			var win = new Gtk.Window("Détecteur de visages");
			win.DefaultHeight = 350;
			win.DefaultWidth = 400;
			
			var toolbar = new Gtk.Toolbar();
			var openButton = new Gtk.ToolButton(Gtk.Stock.Open);
			toolbar.Add(openButton);
			
			var image = new Gtk.Image();
			
			openButton.Clicked += (sender, eventArgs) => {
				var fileDialog = new Gtk.FileChooserDialog("Choisissez une photo", null, Gtk.FileChooserAction.Open,
				                                           "Open", Gtk.ResponseType.Accept);
				fileDialog.Run();
				
				var photo = new Gdk.Pixbuf(fileDialog.Filename);
				fileDialog.Destroy();
				
//				var photo = new Gdk.Pixbuf("/home/rapha/Bureau/300px-Thief_-_Radiohead.jpg");
//				var photo = new IntegralImage("/home/rapha/Bureau/The_Beatles_Abbey_Road.jpg").Pixbuf;
				
				var detector = new Detector(photo, classifier.Value);
				
				foreach (var rect in detector.Detect()) {
					rect.Draw(photo);
					Console.WriteLine(rect);
				}
				
				image.Pixbuf = photo;
			};
			
			var vbox = new Gtk.VBox();
			vbox.PackStart(toolbar, false, false, 0);
			vbox.PackStart(image, true, true, 0);
			
			win.Add(vbox);
			win.ShowAll();
			
			Gtk.Application.Run();			
		}
	}
}