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
	using System.Collections.Generic;
	using StringBuilder = System.Text.StringBuilder;
	using ToggleButton = System.Windows.Controls.Primitives.ToggleButton;
	using ChordPresetCollection =  System.Collections.ObjectModel.ObservableCollection<MainWindow.ChordPresetItem>;

	public partial class MainWindow {

		internal class ChordPresetItem {
			internal ChordPresetItem(string name, ChordNoteState[] state) { this.Name = name; this.State = state; }
			internal string Name { get; private set; }
			internal ChordNoteState[] State { get; private set; }
			public override string ToString() { return Name; }
		} //class ChordPresetItem

		Controls.ChordNoteRadioButton[] SetupChords() {
			var checkedByDefaultList = new List<ToggleButton>();
			this.buttonResetChords.Click += (sender, eventArgs) => { ResetChords(); };
			var chordButtonList = new List<Controls.ChordNoteRadioButton>();
			Controls.ChordNoteRadioButton[] chordButtons = null;
			Action makeChord = new Action(() => {
				var byteList = new List<KeyboardLayout.ChordNote>();
				foreach (var button in chordButtons) {
					sbyte shift = sbyte.Parse(button.Shift);
					if (shift == 0) continue;
					if (button.IsChecked != true) continue;
					bool bigShiftDone = false;
					if (button.BigChanger != null && button.BigChanger.IsChecked == true) {
						shift -= 24;
						bigShiftDone = true;
					} //if
					if ((!bigShiftDone) && button.Changer != null && button.Changer.IsChecked == true)
						shift -= 12;
					byteList.Add(new KeyboardLayout.ChordNote(shift, (string)button.Content));
				} //loop
				KeyboardLayout.Instance.CurrentChord = byteList.ToArray();
			}); // makeChord
			cb13div4.Checked += (sender, eventArgs) => { cb13div2.IsEnabled = cb13div4.IsChecked != true; };
			cb13div4.Unchecked += (sender, eventArgs) => { cb13div2.IsEnabled = cb13div4.IsChecked != true; };
			cb11div4.Checked += (sender, eventArgs) => { cb11div2.IsEnabled = cb11div4.IsChecked != true; };
			cb11div4.Unchecked += (sender, eventArgs) => { cb11div2.IsEnabled = cb11div4.IsChecked != true; };
			cb9div4.Checked += (sender, eventArgs) => { cb9div2.IsEnabled = cb9div4.IsChecked != true; };
			cb9div4.Unchecked += (sender, eventArgs) => { cb9div2.IsEnabled = cb9div4.IsChecked != true; };
			foreach (var child in gridChords.Children) {
				((FrameworkElement)child).Margin = new Thickness(0, 4, 20, 0);
				ToggleButton tb = child as ToggleButton;
				if (tb != null) {
					tb.Checked += (sender, eventArgs) => { makeChord(); };
					tb.Unchecked += (sender, eventArgs) => { makeChord(); };
					if (tb.Tag != null) {
						string key = (string)tb.Tag;
						chordAlternateKey.Add(key, tb);
					} //if tagged for keyboard key
					if (tb.IsChecked == true)
						checkedByDefaultList.Add(tb);
				} //if
				Controls.ChordNoteRadioButton rb = child as Controls.ChordNoteRadioButton;
				if (rb != null)
					chordButtonList.Add(rb);
			} //loop
			chordSettingsCheckedByDefault = checkedByDefaultList.ToArray();
			chordButtons = chordButtonList.ToArray();
			makeChord();
			return chordButtons;
		} //SetupChords

		internal enum ChordNoteState : byte { Off = 0, On = 1, Double = 2, DoubleDouble = 4 }

		Action SetupChordPresets(Controls.ChordNoteRadioButton[] chordButtons) { // return is reset
			listBoxChordPresets.Items.Clear();
			var presetCollection = new ChordPresetCollection();
			listBoxChordPresets.ItemsSource = presetCollection;
			// saveChord:
			Func<ChordNoteState[]> saveChord = new Func<ChordNoteState[]>(() => {
				ChordNoteState[] result = new ChordNoteState[chordButtons.Length];
				for (int index = 0; index < chordButtons.Length; ++index) {
					var button = chordButtons[index];
					result[index] = button.IsChecked == true ?
						ChordNoteState.On : ChordNoteState.Off;
					if (button.BigChanger != null && button.BigChanger.IsChecked == true)
						result[index] |= ChordNoteState.DoubleDouble;
					if (button.Changer != null && button.Changer.IsChecked == true)
						result[index] |= ChordNoteState.Double;
				} //loop
				return result;
			}); //makeChordName
			// restoreChord:
			Action<ChordNoteState[]> restoreChord = new Action<ChordNoteState[]>((savedState) => {
				for (int index = 0; index < chordButtons.Length; ++index) {
					var button = chordButtons[index];
					if (button.BigChanger != null)
						button.BigChanger.IsChecked =
							(savedState[index] & ChordNoteState.DoubleDouble) == ChordNoteState.DoubleDouble;
					if (button.Changer != null)
						button.Changer.IsChecked =
							(savedState[index] & ChordNoteState.Double) == ChordNoteState.Double;
					button.IsChecked = (savedState[index] & ChordNoteState.On) == ChordNoteState.On;
				} //loop
			}); //restoreChord
			// makeChordName:
			Func<string> makeChordName = new Func<string>(() => {
				var missingName = (string)chordButtons[0].Content;
				var result = new StringBuilder();
				for (int index = chordButtons.Length - 1; index >=0; --index) {
					var button = chordButtons[index];
					if (button.IsChecked != true) continue;
					string noteName = (string)button.Content;
					if (noteName == "5" || noteName == "3") continue;
					if (noteName == missingName) continue;
					if (result.Length > 0 && noteName.Length > 0
						&& !char.IsLetter(noteName[0])
						&& !char.IsLetter(result[result.Length - 1]))
						result.Append("/");
					result.Append(noteName);
				} //loop
				string proposed = result.ToString();
				if (proposed.Length < 1) return "major";
				if (proposed == "m") return "minor";
				if (proposed.Length == 4 && proposed.Substring(0, 2) == "m5" && proposed[3] == '6') return "dim";
				if (proposed.StartsWith("2") || proposed.StartsWith("4")) return "sus" + proposed;
				return result.ToString();
			}); //makeChordName
			// setup handlers:
			var adjustEnable = new Action<bool>((resetChords) => {
				buttonRemoveChordPreset.IsEnabled = listBoxChordPresets.SelectedIndex >= 0;
				if (resetChords && !buttonRemoveChordPreset.IsEnabled)
					ResetChords();
			}); //adjustEnable
			adjustEnable(false);
			//
			presetCollection.CollectionChanged += (sender, eventArgs) => { adjustEnable(false); };
			buttonRemoveChordPreset.Click += (sender, eventArgs) => {
				var selected = listBoxChordPresets.SelectedIndex;
				if (selected < 0) return;
				presetCollection.RemoveAt(selected);
				if (selected < presetCollection.Count)
					listBoxChordPresets.SelectedIndex = selected;
				else if (presetCollection.Count > 0)
					listBoxChordPresets.SelectedIndex = presetCollection.Count - 1;
			}; //buttonRemoveChordPreset.Click
			buttonAddChordPreset.Click += (sender, eventArgs) => {
				var item = new ChordPresetItem(makeChordName(), saveChord());
				presetCollection.Add(item);
				listBoxChordPresets.SelectedIndex = presetCollection.Count - 1;
			}; //buttonAddChordPreset.Click
			buttonInsertChordPreset.Click += (sender, eventArgs) => {
				var item = new ChordPresetItem(makeChordName(), saveChord());
				int index = this.listBoxChordPresets.SelectedIndex;
				if (index < 0) index = 0;
				presetCollection.Insert(index, item);
				listBoxChordPresets.SelectedIndex = index;
			}; //buttonInsertChordPreset.Click
			listBoxChordPresets.SelectionChanged += (sender, eventArgs) => {
				bool needsReset = eventArgs.RemovedItems.Count > 0 && eventArgs.AddedItems.Count < 1;
				adjustEnable(needsReset);
				ListBox listBoxSender = (ListBox)sender;
				int index = this.listBoxChordPresets.SelectedIndex;
				if (index < 0) return;
				restoreChord(presetCollection[index].State);
			}; //listBoxChordPresets.SelectionChanged
			//
			Action<string, ChordNoteState[]> resetOne = new Action<string, ChordNoteState[]>((name, chord) => {
				presetCollection.Add(new ChordPresetItem(name, chord));
			}); //resetOne
			Func<int, int, int, ChordNoteState[]> makeTriad = new Func<int, int, int, ChordNoteState[]>( (mediant, dominant, leading) => {
				ChordNoteState[] chord = new ChordNoteState[chordButtons.Length];
				foreach (int index in new int[] { 0, 3, 6, 10 })
					chord[index] = ChordNoteState.On; // 'x'
				chord[mediant] = ChordNoteState.On;
				chord[dominant] = ChordNoteState.On;
				if (leading > 0) chord[leading] = ChordNoteState.On;
				return chord;
			}); //makeTriad
			//makeTriad
			Action reset = new Action(() => {
				const int fifth = 15; const int none = 0;
				presetCollection.Clear();
				resetOne("7", makeTriad(19, fifth, 12));
				resetOne("minor", makeTriad(18, fifth, none));
				resetOne("major", makeTriad(19, fifth, none));
				resetOne("7+", makeTriad(19, fifth, 13));
				resetOne("dim", makeTriad(18, fifth - 1, 11));
				resetOne("6", makeTriad(19, fifth, 11));
				resetOne("sus2", makeTriad(17, fifth, none));
				resetOne("sus4", makeTriad(20, fifth, none));
				resetOne("m5\u2212/7", makeTriad(18, fifth - 1, 12));
			}); //reset
			//
			this.buttonResetChordPreset.Click += (sender, eventArgs) => { reset(); };
			reset();
			return reset;
		} //SetupChordPresets

	} //class MainWindow

} //namespace WPF.Ui 
