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
	using System.Windows.Controls;
	using System.Windows.Shapes;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using System.Windows.Input;
	using System.IO;
	using BrushStack = System.Collections.Generic.Stack<System.Windows.Media.Brush>;
	using LabelStack = System.Collections.Generic.Stack<string>;
	using ToggleButton = System.Windows.Controls.Primitives.ToggleButton;
	using KeyDictionary = System.Collections.Generic.Dictionary<System.Windows.Input.Key, MainWindow.KeyboardElement>;

	public partial class MainWindow {

		Brush ButtonPressedBrush = Brushes.Red;
		internal enum NotationMode { None = 0, Notes = 1, MidiNotes = 2, Characters = 4, }

		internal class KeyboardElementLocation {
			internal KeyboardElementLocation(byte row, byte shiftInRow) { this.Row = row; this.ShiftInRow = shiftInRow; }
			internal byte Row { get; private set; }
			internal byte ShiftInRow { get; private set; }
		} //class KeyboardElementLocation

		internal class KeyboardElement {
			internal KeyboardElement(MainWindow parent, byte row, byte shiftInRow, Action<Rect> recording) {
				this.parent = parent;
				this.Location = new KeyboardElementLocation(row, shiftInRow);
				this.recording = recording;
			} //KeyboardElement
			internal void Visualize(Rectangle rectangle, TextBlock textBlock) {
				this.textBlock = textBlock;
				this.rectangle = rectangle;
				textBlock.IsHitTestVisible = false;
			} //Attach
			internal void ReColor(Brush color) {
				if (rectangle == null) return;
				rectangle.Fill = color;
			} //ReColor
			void Highlight(Brush color) {
				if (rectangle == null) return;
				brushStack.Push(rectangle.Fill);
				rectangle.Fill = color;
			} //Highlight
			void UnHighlight() {
				if (rectangle == null) return;
				if (brushStack.Count > 0)
					rectangle.Fill = brushStack.Pop();
				if (textBlock == null) return;
				if (labelStack.Count > 0)
					textBlock.Text = labelStack.Pop();
			} //UnHighlight
			internal void Highlight(string label) {
				if (label == null || textBlock == null) return;
				labelStack.Push(textBlock.Text);
				textBlock.Text = label;
			} //Highlight

			internal sbyte BaseTone {
				get { return baseTone; }
				set {
					if (baseTone == value) return;
					baseTone = value;
					showNote();
				} //set BaseTone
			} //BaseTone
			internal double Distance(KeyboardElement otherElement) {
				if (rectangle == null || otherElement.rectangle == null) return double.PositiveInfinity;
				double x = Canvas.GetLeft(this.rectangle);
				double xOther = Canvas.GetLeft(otherElement.rectangle);
				double y = Canvas.GetTop(this.rectangle);
				double yOther = Canvas.GetTop(otherElement.rectangle);
				return System.Math.Sqrt((x - xOther) * (x - xOther) + (y - yOther) * (y - yOther));
			} //Distance
			sbyte baseTone;
			BrushStack brushStack = new BrushStack();
			LabelStack labelStack = new LabelStack();
			internal KeyboardElementLocation Location { get; private set; }
			internal NotationMode NotationMode {
				get { return notationMode; }
				set {
					if (notationMode == value) return;
					notationMode = value;
					showNote();
				} //set NotationMode
			} //NotationMode
			void showNote() {
				int value = BaseTone + transposition;
				if (notationMode == NotationMode.None)
					text = string.Empty;
				if ((notationMode & NotationMode.Notes) > 0) {
					int size = DefinitionSet.noteSet.Length;
					int index = ((value % size) + size) % size;
					text = DefinitionSet.noteSet[index];
				} //if Notes
				if ((notationMode & NotationMode.MidiNotes) > 0) {
					text = goodMidiNote(value) ?
						value.ToString() : string.Empty;
				} // if MIDI
				if ((notationMode & NotationMode.Characters) > 0) {
					if (Character != null)
						text = Character;
				} //if Characters
			} //showNote
			NotationMode notationMode;
			internal string Character { get; set; }
			string text {
				get { if (textBlock != null) return textBlock.Text; else return null; }
				set { if (textBlock != null) textBlock.Text = value; }
			} //text
			internal void Transpose(int value) {
				transposition = value;
				showNote();
				if (rectangle != null) {
					int testVisibility = transposition + this.BaseTone;
					rectangle.Visibility = goodMidiNote(testVisibility) ?
						Visibility.Visible : Visibility.Hidden;
				} //if
			} //Transpose
			int transposition;
			internal void Activate(bool activate, KeyboardLayout.ChordNote[] chord, byte volume) {
				byte note = (byte)(transposition + BaseTone);
				if (activate) {
					if (this.recording != null && this.rectangle != null) {
						double x = Canvas.GetLeft(rectangle);
						double y = Canvas.GetTop(rectangle);
						Rect rect = new Rect(x, y, rectangle.Width, rectangle.Height);
						this.recording(rect);
					} //if
					parent.api.NoteOn(note, volume, parent.channel);
					if (chord != null) {
						lastChord = chord;
						foreach (var chordShift in chord) {
							int shiftedNote = note + chordShift.PitchShift;
							if (goodMidiNote(shiftedNote))
								parent.api.NoteOn((byte)shiftedNote, volume, parent.channel);
						} //loop chord
					} //if chord
					if (!parent.canvasKeyboard.IsVisible) return;
					if (rectangle == null) return;
					Highlight(parent.ButtonPressedBrush);
					if (parent.checkBoxHighlightChords.IsChecked != true) return;
					if (lastChord == null) return;
					lastChordLayout = KeyboardLayout.Instance.FindChordLayout(this, lastChord);
					if (lastChordLayout == null) return;
					foreach (var element in lastChordLayout)
						element.Highlight(DefinitionSet.chordHighlightBrush);
				} else {
					if (lastChord != null) {
						foreach (var chordNote in lastChord) {
							int shiftedNote = note + chordNote.PitchShift;
							if (goodMidiNote(shiftedNote))
								parent.api.NoteOff((byte)shiftedNote, parent.api.maxVelocity, parent.channel);
						} //loop chord
						lastChord = null;
					} //if chord
					parent.api.NoteOff(note, parent.api.maxVelocity, parent.channel);
					if (!parent.canvasKeyboard.IsVisible) return;
					if (rectangle == null) return;
					UnHighlight();
					if (lastChordLayout == null) return;
					foreach (var element in lastChordLayout)
						element.UnHighlight();
				} //if
			} //Activate
			bool goodMidiNote(int value) {
				return value >= 0 && value <= sbyte.MaxValue;
			} //goodMidiNote
			TextBlock textBlock;
			Rectangle rectangle;
			MainWindow parent;
			Action<Rect> recording;
			KeyboardLayout.ChordNote[] lastChord;
			KeyboardElement[] lastChordLayout;
		} //class KeyboardElement

		void SetupVisualization() {
			RadialGradientBrush buttonPressedBrush = new RadialGradientBrush(
				DefinitionSet.pressedButtonStartColor,
				DefinitionSet.pressedButtonEndColor);
			buttonPressedBrush.RadiusX = 1;
			buttonPressedBrush.RadiusY = 1;
			this.ButtonPressedBrush = buttonPressedBrush;
			canvasKeyboard.SnapsToDevicePixels = true;
			//canvasKeyboard.Background = Brushes.Yellow;
			canvasKeyboard.SizeChanged += (sizeSender, sizeEventArgs) => {
				Grid grid = (Grid)canvasKeyboard.Parent;
				canvasKeyboard.Width = grid.ActualWidth;
				canvasKeyboard.Height = grid.ActualHeight;
				double actualWidth = canvasKeyboard.Width;
				double actualHeight = canvasKeyboard.Height;
				canvasKeyboard.Clip = new RectangleGeometry(new Rect(0, 0, actualWidth, actualHeight));
				double sizeByWidth = actualWidth / DefinitionSet.maximimKeyboardWidth;
				double sizeByHeight = actualHeight / DefinitionSet.keyboardHeight;
				double size = Math.Min(sizeByWidth, sizeByHeight);
				double shiftLeft = (actualWidth - size * DefinitionSet.maximimKeyboardWidth) / 2;
				double shiftTop = DefinitionSet.keyboardHeight / 2 + (actualHeight - size * DefinitionSet.keyboardHeight) / 2;
				int keyboardElementCount = 0;
				for (int yIndex = 0; yIndex < KeyboardLayout.Instance.Rows.Length; ++yIndex) {
					bool shortRow = KeyboardLayout.Instance.Rows[yIndex].IsShort;
					double intervalShift = shortRow ? size / 2 : 0;
					for (int xIndex = 0; xIndex < KeyboardLayout.Instance.Rows[yIndex].Elements.Length; ++xIndex) {
						Rectangle rc = new Rectangle();
						TextBlock tb = new TextBlock();
						rc.Fill = Brushes.White; //SA???
						rc.Stroke = Brushes.Black;
						rc.StrokeThickness = 1;
						rc.Width = size;
						rc.Height = size;
						tb.FontSize = (size + 2) / 3; //SA???
						rc.MouseDown += (sender, eventArgs) => {
							ActivateVisualizedKey((Rectangle)sender, true);
						};
						rc.MouseUp += (sender, eventArgs) => {
							ActivateVisualizedKey((Rectangle)sender, false);
						};
						rc.MouseEnter += (sender, eventArgs) => {
							if (eventArgs.LeftButton == MouseButtonState.Pressed)
								ActivateVisualizedKey((Rectangle)sender, true);
						};
						rc.MouseLeave += (sender, eventArgs) => {
							ActivateVisualizedKey((Rectangle)sender, false);
						};
						double xTop = shiftLeft + intervalShift + xIndex * (size - 1);
						double yTop = shiftTop + (DefinitionSet.keyboardHeight - yIndex - 1) * (size - 1);
						Canvas.SetLeft(rc, xTop);
						Canvas.SetTop(rc, yTop);
						Canvas.SetLeft(tb, xTop + 3); //SA???
						Canvas.SetTop(tb, yTop + 2); //SA???
						canvasKeyboard.Children.Add(rc);
						canvasKeyboard.Children.Add(tb);
						KeyboardElement ke = KeyboardLayout.Instance.Keys[keyboardElementCount++];
						ke.Visualize(rc, tb);
						rc.Tag = ke;
					} //loop x
				} //loop y
				KeyboardLayout.Instance.ForEachPhysicalKeyboardElement((note) => { note.ReColor(Brushes.PaleGreen); });
				SetupVisualizationOptions();
				radioShowNotes.IsChecked = false; // for refresh in next line
				radioShowNotes.IsChecked = true;
			}; //canvasKeyboard.SizeChanged
		} //SetupVisualization

		void ActivateVisualizedKey(Rectangle rectangle, bool activate) {
			KeyboardElement el = (KeyboardElement)rectangle.Tag;
			var chord = KeyboardLayout.Instance.CurrentChord;
			if (!Keyboard.IsKeyDown(Key.LeftCtrl))
				chord = null;
			byte volume = (byte)this.sliderOnVelocity.Value;
			if (Keyboard.IsKeyDown(Key.LeftShift))
				volume = (byte)this.sliderOnPedalVelocity.Value;
			el.Activate(activate, chord, volume);
		} //ActivateVisualizedKey
		void SetupVisualizationOptions() {
			Action setCharacters = new Action(() => {
				NotationMode mode = NotationMode.None;
				if (radioShowMidiNotes.IsChecked == true)
					mode |= NotationMode.MidiNotes;
				if (radioShowNotes.IsChecked == true)
					mode |= NotationMode.Notes;
				if (checkBoxShowCharacters.IsChecked == true)
					mode |= NotationMode.Characters;
				KeyboardLayout.Instance.NotationMode = mode;
			});
			foreach (ToggleButton btn in new ToggleButton[] { radioShowMidiNotes, radioShowNotes, checkBoxShowCharacters}) {
				btn.Checked += (sender, eventArgs) => { setCharacters(); };
				if (btn is CheckBox)
					btn.Unchecked += (sender, eventArgs) => { setCharacters(); };
			} //loop
		} //SetupVisualizationOptions

	} //class MainWindow

} //namespace WPF.Ui