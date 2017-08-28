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
namespace WPF.Controls {
	using System.Windows;
	using RadioButton = System.Windows.Controls.RadioButton;
	using ToggleButton = System.Windows.Controls.Primitives.ToggleButton;

	internal partial class ChordNoteRadioButton : RadioButton {

		static FrameworkPropertyMetadata ShiftPropertyMetadata =
			new FrameworkPropertyMetadata("0");
		public static DependencyProperty ShiftProperty =
			DependencyProperty.Register("Shift", typeof(string), typeof(ChordNoteRadioButton), ShiftPropertyMetadata);
		public string Shift {
			get { return (string)GetValue(ShiftProperty); }
			set { SetValue(ShiftProperty, value); }
		} //Shift

		static FrameworkPropertyMetadata ChangerPropertyMetadata =
			new FrameworkPropertyMetadata(null);
		public static DependencyProperty ChangerProperty =
			DependencyProperty.Register("Changer", typeof(ToggleButton), typeof(ChordNoteRadioButton), ChangerPropertyMetadata);
		public ToggleButton Changer {
			get { return (ToggleButton)GetValue(ChangerProperty); }
			set { SetValue(ChangerProperty, value); }
		} //Changer

		static FrameworkPropertyMetadata BigChangerPropertyMetadata =
			new FrameworkPropertyMetadata(null);
		public static DependencyProperty BigChangerProperty =
			DependencyProperty.Register("BigChanger", typeof(ToggleButton), typeof(ChordNoteRadioButton), BigChangerPropertyMetadata);
		public ToggleButton BigChanger {
			get { return (ToggleButton)GetValue(BigChangerProperty); }
			set { SetValue(BigChangerProperty, value); }
		} //BigChanger

	} //class ChordNoteRadioButton

} //namespace WPF.Controls
