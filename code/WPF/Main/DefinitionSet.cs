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
namespace WPF {
	using System.Windows.Media;
	using System.Windows.Input;
	using KeyReplacementDictionary = System.Collections.Generic.Dictionary<System.Windows.Input.Key, char>;

	static class DefinitionSet {
		static DefinitionSet() {
			keyReplacements = new KeyReplacementDictionary();
			keyReplacements.Add(Key.RightShift, '\u2191');
			keyReplacements.Add(Key.Oem6, ']');
			keyReplacements.Add(Key.Oem4, '[');
			keyReplacements.Add(Key.Oem5, '\\');
			keyReplacements.Add(Key.Return, '\u21b2');
			keyReplacements.Add(Key.OemSemicolon, ';');
			keyReplacements.Add(Key.OemComma, '<');
			keyReplacements.Add(Key.OemPeriod, '>');
			keyReplacements.Add(Key.Oem7, ',');
			keyReplacements.Add(Key.Oem2, '?');
			keyReplacements.Add(Key.OemMinus, '\u2014');
			keyReplacements.Add(Key.OemPlus, '+');
			keyReplacements.Add(Key.Back, '\u2190');
		} //DefinitionSet
		internal const int maximimKeyboardWidth = 34;
		internal const int keyboardHeight = 11;
		internal const sbyte midiMiddleC = 60;
		internal const byte topLeftFourthDegreeWickiHayden = 5;
		internal const byte topLeftFifthDegreeWickiHayden = 7;
		internal const byte incrementJanko = 1;
		internal const byte shortRowStartJanko = 24; // calculated to keep middle C at the same position for Wicki-Hayden<->Janko
		internal static readonly string[] noteSet = new string[] { "C", "C\u266F", "D", "D\u266F", "E", "F", "F\u266F", "G", "G\u266F", "A", "A\u266F", "B" };
		internal const char keyTypeDigitIndicator = 'D';
		internal static readonly KeyReplacementDictionary keyReplacements;
		internal static readonly Brush chordHighlightBrush = Brushes.Yellow;
		internal static readonly Color pressedButtonStartColor = Colors.White;
		internal static readonly Color pressedButtonEndColor = Colors.Red;
		internal const string formatPersistentStateFileName = "{0}-{1}-{2}"; // file name, GUID, URI
		internal const byte sampleRenderingHalfStepKeyWidth = 6;
	} //DefinitionSet

} //namespace WPF