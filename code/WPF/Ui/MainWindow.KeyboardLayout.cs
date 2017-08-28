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
	using Key = System.Windows.Input.Key;
	using KeyDictionary = System.Collections.Generic.Dictionary<System.Windows.Input.Key, MainWindow.KeyboardElement>;
	using KeyboardElementList = System.Collections.Generic.List<MainWindow.KeyboardElement>;

	public partial class MainWindow {

		internal class KeyboardLayout {

			internal class KeyboardRowDescriptor {
				internal KeyboardRowDescriptor(KeyboardElement[] elements, bool isShort) {
					this.Elements = elements;
					this.IsShort = isShort;
				} //KeyboardRowDescriptor
				internal KeyboardElement[] Elements { get; private set; }
				internal bool IsShort { get; private set; }
			} //class KeyboardRowDescriptor

			internal KeyboardLayout(MainWindow parent) {
				this.parent = parent;
				this.sampleRenderer = new SampleRenderer(parent);
				KeyboardElementList keyList = new KeyboardElementList(0);
				Rows = new KeyboardRowDescriptor[DefinitionSet.keyboardHeight];
				for (int yIndex = 0; yIndex < DefinitionSet.keyboardHeight; ++yIndex) {
					bool shortRow = yIndex % 2 == 0;
					int xLength = DefinitionSet.maximimKeyboardWidth;
					if (!shortRow) ++xLength;
					KeyboardElement[] rowElements = new KeyboardElement[xLength];
					for (int xIndex = 0; xIndex < xLength; ++xIndex) {
						var ke = new KeyboardElement(parent, (byte)yIndex, (byte)xIndex, sampleRenderer.Recording);
						keyList.Add(ke);
						rowElements[xIndex] = ke;
					} //loop x
					Rows[yIndex] = new KeyboardRowDescriptor(rowElements, shortRow);
				} //loop y
				int physicalRightmost = int.MinValue;
				for (int yIndex = 0; yIndex < Main.PhysicalKeyboardLayout.physicalRows.Length; ++yIndex) {
					int rightMost = Main.PhysicalKeyboardLayout.physicalRows[yIndex].Length * 2 - yIndex;
					if (physicalRightmost < rightMost)
						physicalRightmost = rightMost;
				} //loop
				physicalRightmost += 1;
				int rowShift = 1 + (DefinitionSet.keyboardHeight - Main.PhysicalKeyboardLayout.physicalRows.Length) / 2;
				int shiftInRow = 1 + (DefinitionSet.maximimKeyboardWidth * 2 - physicalRightmost) / 4;
				this.Keys = keyList.ToArray();
				int maxKey = -1;
				for (int yIndex = 0; yIndex < Main.PhysicalKeyboardLayout.physicalRows.Length; ++yIndex) {
					for (int xIndex = 0; xIndex < Main.PhysicalKeyboardLayout.physicalRows[yIndex].Length; ++xIndex) {
						int actualIndexX = shiftInRow;
						KeyboardElement element = Rows[yIndex + rowShift].Elements[actualIndexX - yIndex + xIndex];
						Key key = Main.PhysicalKeyboardLayout.physicalRows[yIndex][xIndex];
						string sKey = key.ToString();
						if (sKey.Length > 1) {
							if (key < Key.D0 || key > Key.D9) {
								char replacement;
								if (DefinitionSet.keyReplacements.TryGetValue(key, out replacement))
									sKey = new string(new char[] { replacement });
							} else
								sKey = sKey.Substring(1, 1);
						} //if
						element.Character = sKey;
						if ((int)key > maxKey)
							maxKey = (int)key;
						notes.Add(key, element);
					} // loop x
					if (Rows[yIndex].IsShort)
						++shiftInRow;
				} //loop y
				MaxKey = (Key)maxKey;
			} //KeyboardLayout

			internal void PopulateTones(bool isWickiHayden) {
				byte rowTone = 0;
				if (!isWickiHayden)
					rowTone = DefinitionSet.shortRowStartJanko;
				for (int yIndex = 0; yIndex < Rows.Length; ++yIndex) {
					for (int xIndex = 0; xIndex < Rows[yIndex].Elements.Length; ++xIndex) {
						sbyte baseTone = (sbyte)(rowTone + xIndex * 2);
						Rows[yIndex].Elements[xIndex].BaseTone = baseTone;
					} //loop x
					if (isWickiHayden) {
						if (Rows[yIndex].IsShort)
							rowTone += DefinitionSet.topLeftFourthDegreeWickiHayden;
						else
							rowTone += DefinitionSet.topLeftFifthDegreeWickiHayden;
					} else {
						if (Rows[yIndex].IsShort)
							rowTone -= DefinitionSet.incrementJanko;
						else
							rowTone += DefinitionSet.incrementJanko;
					} //if
				} //loop y
			} //PopulateTones
			
			internal void Transpose(int value) {
				foreach (var key in Keys)
					key.Transpose(value);
			} //Transpose

			internal NotationMode NotationMode {
				get { return notationMode; }
				set {
					if (notationMode == value) return;
					notationMode = value;
					foreach (var key in Keys)
						key.NotationMode = value;
				} //set NotationMode
			} //NotationMode

			internal Key MaxKey { get; private set; }
			internal KeyboardElement[] Keys { get; private set; }
			internal KeyboardRowDescriptor[] Rows { get; private set; }

			internal class ChordNote {
				internal ChordNote(sbyte pitchShift, string noteDescriptor) { this.PitchShift = pitchShift; this.NoteDescriptor = noteDescriptor; }
				internal sbyte PitchShift { get; private set; }
				internal string NoteDescriptor { get; private set; }
			} //class ChordNote

			internal ChordNote[] CurrentChord { get; set; }

			internal KeyboardElement FindKeyboardElementByKey(Key key) {
				KeyboardElement result;
				if (!notes.TryGetValue(key, out result)) return null;
				return result;
			} //FindKeyboardElementByKey

			internal void ForEachPhysicalKeyboardElement(System.Action<KeyboardElement> action) {
				foreach (var note in notes.Values)
					action(note);
			} //ForEachPhysicalKeyboardElement

			KeyboardElement FindKeyboardElementInRow(sbyte note, int row) {
				if (row + 1 > Rows.Length) return null;
				if (row < 0) return null;
				int elementCount = Rows[row].Elements.Length;
				sbyte rowStart = Rows[row].Elements[0].BaseTone;
				bool noteIsEven = note % 2 == 0;
				bool rowIsEven = rowStart % 2 == 0;
				if (rowIsEven != noteIsEven) return null;
				int shift = note - rowStart;
				if (shift < 0) return null;
				if (shift / 2 >= elementCount) return null;
				return Rows[row].Elements[shift / 2];
			} //FindKeyboardElementInRow

			internal KeyboardElement[] FindChordLayout(KeyboardElement root, ChordNote[] chord) {
				KeyboardElementList list = new KeyboardElementList();
				int rootRow = root.Location.Row;
				int rootShift = root.Location.ShiftInRow;
				foreach (var chordNote in chord) {
					double distance = double.PositiveInfinity;
					KeyboardElement tryToFind = null;
					for (int rowIndex = 0; rowIndex < Rows.Length; ++rowIndex) {
						var alternative = FindKeyboardElementInRow((sbyte)(chordNote.PitchShift + root.BaseTone), rowIndex);
						if (alternative != null) {
							double newDistance = root.Distance(alternative);
							if (newDistance < distance) {
								tryToFind = alternative;
								distance = newDistance;
							} //if
						} //if
					} //loop
					if (tryToFind != null) {
						if (parent.checkBoxHighlightChordsWithLabels.IsChecked == true)
							tryToFind.Highlight(chordNote.NoteDescriptor);
						list.Add(tryToFind);
					} //if
				} //loop
				if (list.Count < 1)
					return null;
				else
					return list.ToArray();
			} //FindChordLayout

			NotationMode notationMode;
			KeyDictionary notes = new KeyDictionary();
			MainWindow parent;
			SampleRenderer sampleRenderer;

			internal static Action Create(MainWindow parent) {
				Instance = new KeyboardLayout(parent);
				return Instance.sampleRenderer.Activation;
			} //Create

			internal static KeyboardLayout Instance { get; private set; }

		} //KeyboardLayout

	} //class MainWindow

} //namespace WPF.Ui
