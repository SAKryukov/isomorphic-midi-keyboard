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
namespace WPF.Main {
	using Key = System.Windows.Input.Key;

	static class PhysicalKeyboardLayout {

		static PhysicalKeyboardLayout() {
			physicalRows = new Key[][] { lowRow, aRow, tabRow, numRow };
		} //PhysicalKeyboardLayout

		internal static readonly Key[][] physicalRows;

		static Key[] lowRow = new Key[] { Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M, Key.OemComma, Key.OemPeriod, Key.Oem2, Key.RightShift };
		static Key[] aRow = new Key[] { Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L, Key.OemSemicolon, Key.Oem7, Key.Enter };
		static Key[] tabRow = new Key[] { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P, Key.Oem4, Key.Oem6, Key.Oem5 }; // {}| }; 
		static Key[] numRow = new Key[] { Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.D0, Key.OemMinus, Key.OemPlus, Key.Back };

	} //class PhysicalKeyboardLayout

} //namespace WPF.Main
