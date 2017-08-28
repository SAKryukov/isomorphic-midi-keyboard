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
	using System.Reflection;
	using System.IO;
	using System.Linq;
	using System.Windows;
	using System.Windows.Controls;
	using System.Collections.Generic;
	using StringBuilder = System.Text.StringBuilder;
	using ToggleButton = System.Windows.Controls.Primitives.ToggleButton;
	using ChordPresetCollection = System.Collections.ObjectModel.ObservableCollection<MainWindow.ChordPresetItem>;
	using GuidAttribute = System.Runtime.InteropServices.GuidAttribute;

	public partial class MainWindow {

		class Persistence {

			internal static void SaveState(Window parent) {
				Persistence instance = new Persistence();
				instance.TraverseChildren(parent, null, null);
				string fileName = instance.GetFileName();
				using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create))) {
					writer.Write(instance.buttonCount);
					writer.Write(instance.sliderCount);
					writer.Write(instance.listCount);
					instance.TraverseChildren(parent, writer, null);
				} //using
			} //SaveState

			internal static void RestoreState(Window parent) {
				Persistence instance = new Persistence();
				instance.TraverseChildren(parent, null, null);
				string fileName = instance.GetFileName();
				if (!File.Exists(fileName)) return;
				try {
					using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open))) {
						var buttonCount = reader.ReadInt32();
						var sliderCount = reader.ReadInt32();
						var listCount = reader.ReadInt32();
						if (instance.IsMatch(buttonCount, sliderCount, listCount))
							instance.TraverseChildren(parent, null, reader);
					} //using
				} catch { }
			} //RestoreState

			int buttonCount, sliderCount, listCount;

			bool IsMatch(int buttonCount, int sliderCount, int listCount) {
				return this.buttonCount == buttonCount && this.sliderCount == sliderCount && this.listCount == listCount;
			} //IsMatch

			string GetFileName() {
				var attributeString = string.Empty;
				Assembly assembly = Assembly.GetEntryAssembly();
				var attributes =
					assembly.GetCustomAttributes(false).OfType<System.Runtime.InteropServices.GuidAttribute>();
				if (attributes.Any())
					attributeString = attributes.First().Value;
				string result = Path.GetFileNameWithoutExtension(assembly.Location);
				string special = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				// for debugging:
				//string special = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
				string uriString = string.Empty;
				string[] personalUris = Main.TheApplication.Current.PersonalUri;
				if (personalUris.Any()) {
					uriString = personalUris[0];
					Uri uri = new Uri(uriString);
					uriString = uri.Host;
				} //if
				result = string.Format(DefinitionSet.formatPersistentStateFileName,
					result,
					uriString,
					attributeString);
				return Path.Combine(special, result);
			} //GetFileName

			void TraverseChildren(FrameworkElement parent, BinaryWriter writer, BinaryReader reader) {
				var children = LogicalTreeHelper.GetChildren(parent);
				foreach (var child in children) {
					FrameworkElement element = child as FrameworkElement;
					if (element == null) continue;
					ToggleButton button = element as ToggleButton;
					if (button != null)
						HandleToggleButton(button, writer, reader);
					else {
						Slider slider = element as Slider;
						if (slider != null)
							HandleSlider(slider, writer, reader);
						else {
							ListBox listBox = element as ListBox;
							if (listBox != null)
								HandleListBox(listBox, writer, reader);
							else
								TraverseChildren(element, writer, reader);
						} //if
					} //if
				} //loop
			} //SaveState

			void HandleToggleButton(ToggleButton button, BinaryWriter writer, BinaryReader reader) {
				buttonCount++;
				if (reader == null && writer == null) return;
				if (writer != null)
					writer.Write(button.IsChecked == true);
				if (reader != null)
					button.IsChecked = reader.ReadBoolean();
			} //HandleToggleButton

			void HandleSlider(Slider slider, BinaryWriter writer, BinaryReader reader) {
				sliderCount++;
				if (reader == null && writer == null) return;
				if (writer != null)
					writer.Write(slider.Value);
				if (reader != null)
					slider.Value = reader.ReadDouble();
			} //HandleSlider

			void HandleListBox(ListBox listBox, BinaryWriter writer, BinaryReader reader) {
				listCount++;
				if (reader == null && writer == null) return;
				if (writer != null)
					StoreChordPresets(listBox, writer);
				if (reader != null)
					RestoreChordPresets(listBox, reader);
			} //HandleListBox

			void StoreChordPresets(ListBox listBox, BinaryWriter writer) {
				ChordPresetCollection collection = listBox.ItemsSource as ChordPresetCollection;
				writer.Write(listBox.SelectedIndex);
				writer.Write(collection.Count);
				for (int index = 0; index < collection.Count; ++index) {
					writer.Write(collection[index].Name);
					var states = collection[index].State;
					writer.Write(states.Length);
					for (int stateIndex = 0; stateIndex < states.Length; ++stateIndex)
						writer.Write((byte)states[stateIndex]);
				} //loop
			} //StoreChordPresets
			void RestoreChordPresets(ListBox listBox, BinaryReader reader) {
				ChordPresetCollection collection = listBox.ItemsSource as ChordPresetCollection;
				collection.Clear();
				var selectedIndex = reader.ReadInt32();
				var collectionCount = reader.ReadInt32();
				for (int index = 0; index < collectionCount; ++index) {
					var name = reader.ReadString();
					var stateLength = reader.ReadInt32();
					var states = new ChordNoteState[stateLength];
					for (int stateIndex = 0; stateIndex < stateLength; ++stateIndex)
						states[stateIndex] = (ChordNoteState)reader.ReadByte();
					collection.Add(new ChordPresetItem(name, states));
				} //loop
				if (selectedIndex >= 0 && selectedIndex < collectionCount - 1)
					listBox.SelectedIndex = selectedIndex;
			} //RestoreChordPresets

		} //class Persistence

	} //class MainWindow

} //namespace WPF.Ui 
