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
	using ToggleButton = System.Windows.Controls.Primitives.ToggleButton;
	using System.Windows.Input;
	using System.Collections.Generic;
	using BitArray = System.Collections.BitArray;
	using System.Text.RegularExpressions;
	using MidiOutApi.Api;

	public partial class MainWindow : Window {

		public MainWindow() {
			InitializeComponent();
			Icon = Main.TheApplication.Current.ApplicationIcon;
			SetupHelp(KeyboardLayout.Create(this));
			radioIsWickiHayden.Checked += (sender, eventArgs) => { KeyboardLayout.Instance.PopulateTones(true); };
			radioIsWickiHayden.Unchecked += (sender, eventArgs) => { KeyboardLayout.Instance.PopulateTones(false); };
			radioIsWickiHayden.IsChecked = true;
			PopulateInstruments();
			SetupSliders();
			var presetsResetAction = SetupChordPresets(SetupChords());
			buttonResetAll.Click += (sender, evenArgs) => { ResetAll(presetsResetAction); };
			SetupVisualization();
			int maxKey = (int)(int)KeyboardLayout.Instance.MaxKey + 1;
			keyStates = new BitArray(maxKey, false);
			this.KeyDown += (sender, evenArgs) => {
				KeyboardElement ke = KeyboardLayout.Instance.FindKeyboardElementByKey(evenArgs.Key);
				if (ke != null) {
					if ((int)evenArgs.Key > maxKey) return;
					if (keyStates[(int)evenArgs.Key]) {
						evenArgs.Handled = true;
						return;
					} //if
					keyStates[(int)evenArgs.Key] = true;
					byte velocity;
					if (Keyboard.IsKeyDown(Key.LeftShift))
						velocity = (byte)this.sliderOnPedalVelocity.Value;
					else
						velocity = (byte)this.sliderOnVelocity.Value;
					var chordToUse = KeyboardLayout.Instance.CurrentChord;
					if (!Keyboard.IsKeyDown(Key.LeftCtrl))
						chordToUse = null;
					byte volume = (byte)this.sliderOnVelocity.Value;
					if (Keyboard.IsKeyDown(Key.LeftShift))
						volume = (byte)this.sliderOnPedalVelocity.Value;
					ke.Activate(true, chordToUse, volume);
					evenArgs.Handled = true;
				} //if
			}; //this.KeyDown
			this.KeyUp += (sender, evenArgs) => {
				if ((int)evenArgs.Key > maxKey) return;
				keyStates[(int)evenArgs.Key] = false;
				KeyboardElement ke = KeyboardLayout.Instance.FindKeyboardElementByKey(evenArgs.Key);
				if (ke != null) {
					ke.Activate(false, null, api.maxVelocity);
					evenArgs.Handled = true;
				} //if
			}; //this.KeyDown
			this.PreviewKeyDown += (sender, evenArgs) => {
				Key key = evenArgs.SystemKey;
				if (key == Key.None) return;
				string keyString = key.ToString();
				ToggleButton option;
				if (!chordAlternateKey.TryGetValue(keyString, out option)) return;
				var wasChecked = option.IsChecked;
				CheckBox cb = option as CheckBox;
				if (cb != null)
					option.IsChecked = !wasChecked;
				else
					option.IsChecked = true;
			}; //this.PreviewKeyDown
			Persistence.RestoreState(this);
		} //MainWindow

		TextBlock[] indicatorSet;
		protected override void OnContentRendered(EventArgs e) {
			foreach (var indicator in indicatorSet)
				indicator.Width = indicator.ActualWidth;
			this.gridKeyboard.Width = this.gridInstruments.ActualWidth;
			this.gridKeyboard.Height = this.gridInstruments.ActualHeight;
		} //OnContentRendered

		void SetupHelp(Action sampleRenderingActivation) {
			Action showHelp = new Action(() => {
				if ((!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
					|| (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.LeftShift))) {
					about.Owner = this;
					about.ShowDialog();
				} else
					sampleRenderingActivation();
			});
			buttonHelp.Click += (sender, eventArgs) => { showHelp(); };
			this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Help,
				new ExecutedRoutedEventHandler((sender, eventArgs) => { showHelp(); })));
		} //SetupHelp

		Slider[] GetSliders() {
			return new Slider[] { sliderTransposition, sliderOnVelocity, sliderOnPedalVelocity, };
		} //GetSliders

		void SetupSliders() {
			sliderTransposition.ValueChanged += (sender, eventArgs) => {
				KeyboardLayout.Instance.Transpose((int)eventArgs.NewValue);
			}; //sliderTone.ValueChanged
			this.sliderTransposition.Minimum = -DefinitionSet.midiMiddleC;
			this.sliderTransposition.Maximum = +DefinitionSet.midiMiddleC;
			this.sliderTransposition.Value = 0;
			sliderOnVelocity.Minimum = 0;
			sliderOnVelocity.Maximum = api.maxVelocity;
			sliderOnVelocity.Value = sliderOnVelocity.Maximum;
			sliderOnPedalVelocity.Maximum = api.maxVelocity;
			sliderOnPedalVelocity.Value = Math.Round(sliderOnPedalVelocity.Maximum / 3 * 2);
			var sliders = GetSliders();
			var resets = new Button[] { buttonToneReset, buttonOnVelocityReset, buttonOnPedalVelocityReset, };
			var indicators = new TextBlock[] { sliderToneValue, sliderOnVelocityValue, sliderOnPedalVelocityValue };
			this.indicatorSet = indicators;
			for (int index = 0; index < sliders.Length; ++index) {
				var slider = sliders[index];
				slider.IsMoveToPointEnabled = true;
				slider.SmallChange = 1;
				slider.IsSnapToTickEnabled = true;
				slider.Tag = indicators[index];
				slider.DataContext = slider.Value;
				resets[index].Tag = slider;
				indicators[index].Tag = slider;
				indicators[index].Text = slider.Value.ToString();
				slider.ValueChanged += (sender, eventArgs) => {
					Slider sliderSender = (Slider)sender;
					TextBlock thisIndicator = (TextBlock)sliderSender.Tag;
					thisIndicator.Text = sliderSender.Value.ToString();
				}; //slider.ValueChanged
				resets[index].Click += (sender, eventArgs) => {
					Button buttonSender = (Button)sender;
					ResetSlider((Slider)buttonSender.Tag);
				}; //resets[index].Click
			} //loop
		} //SetupSliders

		void PopulateInstruments() {
			const int verticalMargin = 8;
			const int horizontalMargin = 12;
			var groupName = this.gridInstruments.GetType().Name;
			var splitPanel = new StackPanel();
			splitPanel.Orientation = Orientation.Vertical;
			var count = InstrumentGroupSet.Groups.Length;
			var first = count / 2;
			var rest = count - first;
			int[] start = new int[] { 0, first };
			int[] length = new int[] { first, rest };
			for (int splitIndex = 0; splitIndex < start.Length; ++splitIndex) {
				var instrumentPanel = new StackPanel();
				instrumentPanel.Orientation = Orientation.Horizontal;
				instrumentPanel.Margin = new Thickness(0, verticalMargin, 0, verticalMargin * splitIndex);
				for (int groupIndex = start[splitIndex]; groupIndex < start[splitIndex] + length[splitIndex]; ++groupIndex) {
					InstrumentGroup group = InstrumentGroupSet.Groups[groupIndex];
					var panel = new StackPanel();
					panel.Margin = new Thickness(horizontalMargin, 0, horizontalMargin, 0);
					panel.Orientation = Orientation.Vertical;
					TextBlock tb = new TextBlock();
					tb.Text = group.Name;
					panel.Children.Add(tb);
					for (int index = (int)group.First; index <= (int)group.Last; ++index) {
						RadioButton rb = new RadioButton();
						if (defaultInstrument == null)
							defaultInstrument = rb;
						rb.GroupName = groupName;
						rb.Tag = (Instrument)index;
						rb.Margin = new Thickness(0, 4, 0, 0);
						string id =
							Regex.Replace(
								((Instrument)index).ToString(),
								"([A-Z][a-z])", " $1",
								System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
						id = Regex.Replace(id, "([0-9])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
						rb.Content = id;
						panel.Children.Add(rb);
						rb.Checked += (sender, eventArgs) => {
							RadioButton instance = (RadioButton)rb;
							Instrument instrument = (Instrument)instance.Tag;
							api.SetInstrument(instrument, channel);
						};
						rb.IsChecked = index == 0;
					} //loop instruments
					instrumentPanel.Children.Add(panel);
				} //loop
				splitPanel.Children.Add(instrumentPanel);
			} //loop
			this.gridInstruments.Children.Add(splitPanel);
		} //PopulateInstruments

		void ResetSlider(Slider slider) {
			double defaultValue = (double)slider.DataContext;
			slider.Value = defaultValue;
		} //ResetSlider
		void ResetChords() {
			foreach (var child in gridChords.Children) {
				ToggleButton tb = child as ToggleButton;
				if (tb != null)
					tb.IsChecked = false;
			} //loop
			foreach (var btn in chordSettingsCheckedByDefault)
				btn.IsChecked = true;
		} //ResetChords
		RadioButton defaultInstrument;
		void ResetAll(Action presetsResetAction) {
			var sliders = GetSliders();
			foreach (var slider in sliders)
				ResetSlider(slider);
			ResetChords();
			radioIsWickiHayden.IsChecked = true;
			defaultInstrument.IsChecked = true;
			radioShowNotes.IsChecked = true;
			checkBoxShowCharacters.IsChecked = false;
			checkBoxHighlightChords.IsChecked = true;
			checkBoxHighlightChordsWithLabels.IsChecked = true;
			presetsResetAction();
		} //ResetAll

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			Persistence.SaveState(this);
			base.OnClosing(e);
			api.Dispose();
		} //OnClosing

		Dictionary<string, ToggleButton> chordAlternateKey = new Dictionary<string, ToggleButton>();
		BitArray keyStates;
		ToggleButton[] chordSettingsCheckedByDefault;
		WindowAbout about = new WindowAbout();
		SimpleApi api = new SimpleApi();
		byte channel = 0;

	} //class MainWindow

} //namespace WPF.Ui
