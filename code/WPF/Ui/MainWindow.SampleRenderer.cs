//	Isomorophic MIDI Keyboard
//
//	Copyright © Sergey A Kryukov, 2017
//	http://www.SAKryukov.org
//	https://www.codeproject.com/Members/SAKryukov
//
//	Original publication:
//	"Musical Study with Isomorphic Computer Keyboard"
//	https://www.codeproject.com/Articles/1201737/Musical-Study-with-Isomorphic-Computer-Keyboard
//
namespace WPF.Ui {
	using System;
	using System.Windows;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.IO;

	public partial class MainWindow {

		class SampleRenderer {

			internal SampleRenderer(MainWindow parent) {
				this.parent = parent;
				recordStep = new Action<Rect>((rectangle) => { if (elementSet != null) elementSet.Add(rectangle); });
			} //SampleRenderer

			void Activate() {
				if (!parent.canvasKeyboard.IsVisible) return;
				if (elementSet == null) {
					elementSet = new System.Collections.Generic.List<Rect>();
					parent.Title = Main.TheApplication.Current.ProductName + " ...Recording... Ctrl+Shift+help to end";
				} else {
					parent.Title = Main.TheApplication.Current.ProductName;
					Render();
				} //if
			} //Activate

			string GetRenderingModeFileName() {
				for (uint count = 0; count < uint.MaxValue; ++count) {
					string fileName = string.Format("{0:x8}.png", count);
					if (!File.Exists(fileName)) return fileName;
				} //loop
				return null;
			} //RenderingModeFileName

			void Render() {
				DrawingVisual dv = new DrawingVisual();
				using (DrawingContext drawingContext = dv.RenderOpen()) {
					Pen pen = new Pen(Brushes.Black, 1);
					double minX = double.PositiveInfinity;
					double minY = double.PositiveInfinity;
					Size size = new Size(0, 0);
					foreach (var rect in elementSet) {
						if (minX > rect.Left) minX = rect.Left;
						if (minY > rect.Top) minY = rect.Top;
					} //loop
					uint maxX = uint.MinValue;
					uint maxY = uint.MinValue;
					double halfStep = DefinitionSet.sampleRenderingHalfStepKeyWidth;
					double renderSize = halfStep * 2;
					foreach (var rect in elementSet) {
						uint x = (uint)Math.Round((rect.Left - minX) * 2 / rect.Width);
						uint y = (uint)Math.Round((rect.Top - minY) / rect.Height);
						if (maxX < x) maxX = x;
						if (maxY < y) maxY = y;
						drawingContext.DrawRectangle(Brushes.White, pen, new Rect(1 + x * halfStep, 1 + y * renderSize, renderSize, renderSize));
					} //loop
					drawingContext.Close();
					RenderTargetBitmap rb = new RenderTargetBitmap((int)((maxX + 2)* halfStep + 2), (int)((maxY + 1) * renderSize + 2), 96, 96, PixelFormats.Default);
					rb.Render(dv);
					PngBitmapEncoder encoder = new PngBitmapEncoder();
					encoder.Frames.Add(BitmapFrame.Create(rb));
					using (Stream stream = File.Create(GetRenderingModeFileName())) {
						encoder.Save(stream);
					} //using
				} //using
				elementSet = null;
			} //Render

			System.Collections.Generic.List<Rect> elementSet;
			Action<Rect> recordStep;
			MainWindow parent;
			internal Action<Rect> Recording { get { return recordStep; } }
			internal Action Activation { get { return new Action(() => { Activate(); }); } }

		} //SampleRenderer

	} //class MainWindow

} //namespace WPF.Ui	
