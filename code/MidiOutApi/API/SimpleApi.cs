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
namespace MidiOutApi.Api {
	using System.Runtime.InteropServices;
	using InstrumentGroupList = System.Collections.Generic.List<InstrumentGroup>;
	using System;
	using System.Text;
	using System.Threading;

	// See:
	// http://www.codeguru.com/columns/dotnet/making-music-with-midi-and-c.html
	// https://www.midi.org/specifications/item/table-1-summary-of-midi-message
	// https://en.wikipedia.org/wiki/General_MIDI
	// https://en.wikipedia.org/wiki/Rollover_%28key%29

	[StructLayout(LayoutKind.Sequential)]
	public struct MidiOutCaps {
		public UInt16 wMid;
		public UInt16 wPid;
		public UInt32 vDriverVersion;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public String szPname;
		public UInt16 wTechnology;
		public UInt16 wVoices;
		public UInt16 wNotes;
		public UInt16 wChannelMask;
		public UInt32 dwSupport;
	} //struct MidiOutCaps

	public enum Instrument {
		AcousticGrandPiano,
		BrightAcousticPiano,
		ElectricGrandPiano,
		HonkyTonkPiano,
		ElectricPiano1,
		ElectricPiano2,
		Harpsichord,
		Clavinet,
		Celesta,
		Glockenspiel,
		MusicBox,
		Vibraphone,
		Marimba,
		Xylophone,
		TubularBells,
		Dulcimer,
		DrawbarOrgan,
		PercussiveOrgan,
		RockOrgan,
		ChurchOrgan,
		ReedOrgan,
		Accordion,
		Harmonica,
		TangoAccordion,
		AcousticGuitarNylon,
		AcousticGuitarSteel,
		ElectricGuitarJazz,
		ElectricGuitarClean,
		ElectricGuitarMuted,
		OverdrivenGuitar,
		DistortionGuitar,
		GuitarHarmonics,
		AcousticBass,
		ElectricBassFinger,
		ElectricBassPick,
		FretlessBass,
		SlapBass1,
		SlapBass2,
		SynthBass1,
		SynthBass2,
		Violin,
		Viola,
		Cello,
		Contrabass,
		TremoloStrings,
		PizzicatoStrings,
		OrchestralHarp,
		Timpani,
		StringEnsemble1,
		StringEnsemble2,
		SynthStrings1,
		SynthStrings2,
		ChoirAahs,
		VoiceOohs,
		SynthVoice,
		OrchestraHit,
		Trumpet,
		Trombone,
		Tuba,
		MutedTrumpet,
		FrenchHorn,
		BrassSection,
		SynthBrass1,
		SynthBrass2,
		SopranoSax,
		AltoSax,
		TenorSax,
		BaritoneSax,
		Oboe,
		EnglishHorn,
		Bassoon,
		Clarinet,
		Piccolo,
		Flute,
		Recorder,
		PanFlute,
		BlownBottle,
		Shakuhachi,
		Whistle,
		Ocarina,
		Lead1Square,
		Lead2Sawtooth,
		Lead3Calliope,
		Lead4Chiff,
		Lead5Charang,
		Lead6Voice,
		Lead7Fifths,
		Lead8BassAndLead,
		Pad1NewAge,
		Pad2Warm,
		Pad3Polysynth,
		Pad4Choir,
		Pad5Bowed,
		Pad6Metallic,
		Pad7Halo,
		Pad8Sweep,
		FX1Rain,
		FX2Soundtrack,
		FX3Crystal,
		FX4Atmosphere,
		FX5Brightness,
		FX6Goblins,
		FX7Echoes,
		FX8SciFi,
		Sitar,
		Banjo,
		Shamisen,
		Koto,
		Kalimba,
		BagPipe,
		Fiddle,
		Shanai,
		TinkleBell,
		Agogo,
		SteelDrums,
		Woodblock,
		TaikoDrum,
		MelodicTom,
		SynthDrum,
		ReverseCymbal,
		GuitarFretNoise,
		BreathNoise,
		Seashore,
		BirdTweet,
		TelephoneRing,
		Helicopter,
		Applause,
		Gunshot
	} //Instrument

	public class InstrumentGroup {
		internal InstrumentGroup(string name, Instrument first, Instrument last) { this.Name = name; this.First = first; this.Last = last; }
		public string Name { get; private set; }
		public Instrument First { get; private set; }
		public Instrument Last { get; private set; }
	} //class InstrumentGroup

	public static class InstrumentGroupSet {
		static InstrumentGroupSet() {
			var list = new InstrumentGroupList();
			list.Add(new InstrumentGroup("Piano", Instrument.AcousticGrandPiano, Instrument.Clavinet));
			list.Add(new InstrumentGroup("Chromatic Percussion", Instrument.Celesta, Instrument.Dulcimer));
			list.Add(new InstrumentGroup("Organ", Instrument.DrawbarOrgan, Instrument.TangoAccordion));
			list.Add(new InstrumentGroup("Guitar", Instrument.AcousticGuitarNylon, Instrument.GuitarHarmonics));
			list.Add(new InstrumentGroup("Bass", Instrument.AcousticBass, Instrument.SynthBass2));
			list.Add(new InstrumentGroup("String", Instrument.Violin, Instrument.Timpani));
			list.Add(new InstrumentGroup("Ensemble", Instrument.StringEnsemble1, Instrument.OrchestraHit));
			list.Add(new InstrumentGroup("Brass", Instrument.Trumpet, Instrument.SynthBrass2));
			list.Add(new InstrumentGroup("Reed", Instrument.SopranoSax, Instrument.Clarinet));
			list.Add(new InstrumentGroup("Pipe", Instrument.Piccolo, Instrument.Ocarina));
			list.Add(new InstrumentGroup("Synth Lead", Instrument.Lead1Square, Instrument.Lead8BassAndLead));
			list.Add(new InstrumentGroup("Synth Pad", Instrument.Pad1NewAge, Instrument.Pad8Sweep));
			list.Add(new InstrumentGroup("Synth Effects", Instrument.FX1Rain, Instrument.FX8SciFi));
			list.Add(new InstrumentGroup("Ethnic", Instrument.Sitar, Instrument.Shanai));
			list.Add(new InstrumentGroup("Percussive", Instrument.TinkleBell, Instrument.ReverseCymbal));
			list.Add(new InstrumentGroup("Sound Effects", Instrument.GuitarFretNoise, Instrument.Gunshot));
			Groups = list.ToArray();
		} //InstrumentGroupSet
		public static InstrumentGroup[] Groups { get; private set; }
	} //class InstrumentGroupSet

	public enum GeneralMidiPercussiobInstrument {
		BassDrum2 = 35,
		BassDrum1 = 36,
		SideStickRimshot = 37,
		SnareDrum1 = 38,
		HandClap = 39,
		SnareDrum2 = 40,
		LowTom2 = 41,
		ClosedHiHat = 42,
		LowTom1 = 43,
		PedalHiHat = 44,
		MidTom2 = 45,
		OpenHiHat = 46,
		MidTom1 = 47,
		HighTom2 = 48,
		CrashCymbal1 = 49,
		HighTom1 = 50,
		RideCymbal1 = 51,
		ChineseCymbal = 52,
		RideBell = 53,
		Tambourine = 54,
		SplashCymbal = 55,
		Cowbell = 56,
		CrashCymbal2 = 57,
		VibraSlap = 58,
		RideCymbal2 = 59,
		HighBongo = 60,
		LowBongo = 61,
		MuteHighConga = 62,
		OpenHighConga = 63,
		LowConga = 64,
		HighTimbale = 65,
		LowTimbale = 66,
		HighAgogô = 67,
		LowAgogô = 68,
		Cabasa = 69,
		Maracas = 70,
		ShortWhistle = 71,
		LongWhistle = 72,
		ShortGüiro = 73,
		LongGüiro = 74,
		Claves = 75,
		HighWoodBlock = 76,
		LowWoodBlock = 77,
		MuteCuíca = 78,
		OpenCuíca = 79,
		MuteTriangle = 80,
		OpenTriangle = 81
	} //GeneralMidiPercussiobInstrument

	public enum ChannelCommand {
		//note-off
		NoteOff = 0x80, // 1000 + low half-byte: channel, next byte: note, next byte: velocity
		//note-on
		NoteOn = 0x90, // 1001 + low half-byte: channel, next byte: note, next byte: velocity
		//poly pressure (aftertouch)
		PolyPressure = 0xA0, // 1010 + low half-byte: channel, next byte: note, next byte: pressure
		//controller (controler is pedal, lever, etc.
		ControlChange = 0xB0, // 1011 + low half-byte: channel, next byte: controller (0-119), next byte: value (0-127)
		//program change (instrument)
		ProgramChange = 0xC0, // 1100 + low half-byte: channel, next byte: new value
		// the channel pressure (aftertouch)
		ChannelPressure = 0xD0, // 1101 + low half-byte: channel, next byte: pressure (all notes)
		PitchWheel = 0xE0 // 1110 + low half-byte: channel, then two-byte unsigned level, center (no change) is 0x2000
				  //                                7 bit of each counts
	} //ChannelCommand

	public class SimpleApi : IDisposable {

		const string dllName = "winmm.dll";

		[DllImport(dllName)]
		private static extern long mciSendString(string command,
		   StringBuilder returnValue, int returnLength,
		   IntPtr winHandle);

		[DllImport(dllName)]
		private static extern int midiOutGetNumDevs();

		[DllImport(dllName)]
		private static extern int midiOutGetDevCaps(Int32 uDeviceID,
		   ref MidiOutCaps lpMidiOutCaps, UInt32 cbMidiOutCaps);

		[DllImport(dllName)]
		private static extern int midiOutOpen(out int handle,
		   int deviceID, MidiCallBack proc, int instance, int flags);

		[DllImport(dllName)]
		private static extern int midiOutShortMsg(int handle,
		   int message);

		[DllImport(dllName)]
		private static extern int midiOutClose(int handle);

		private delegate void MidiCallBack(int handle, int msg,
		   int instance, int param1, int param2);

		static string Mci(string command) {
			StringBuilder reply = new StringBuilder(256);
			mciSendString(command, reply, 256, IntPtr.Zero);
			return reply.ToString();
		} //Mci

		int handle;

		public SimpleApi() {
			var numDevs = midiOutGetNumDevs();
			MidiOutCaps myCaps = new MidiOutCaps();
			var res = midiOutGetDevCaps(0, ref myCaps,
			   (UInt32)Marshal.SizeOf(myCaps));
			res = midiOutOpen(out handle, 0, null, 0, 0);
		} // SimpleApi

		public const byte VelocityMax = 0x7F;
		public const byte PercussionChannel = 10;
		public readonly byte maxVelocity = VelocityMax;

		public void /*IDisposable.*/Dispose() {
			midiOutClose(handle);
		} //IDisposable.Dispose

		public void SetInstrument(Instrument instrument, byte channel) {
			int program = (int)ChannelCommand.ProgramChange | (int)instrument << 8 | channel;
			midiOutShortMsg(handle, program);
		} //SetInstrument
		public void SetInstrument(Instrument instrument) {
			SetInstrument(instrument, 0);
		} //SetInstrument

		public void ApplyWheel(byte channel, short value) {
			// normal number to pair of 7-bit unsigned numbers
			int relative = 0x2000 + value;
			int maskLo = sbyte.MaxValue;
			int maskHi = maskLo << 7; // sic!
			int loByte = relative & maskLo;
			int hiByte = (relative & maskHi) << 1; // sic! creator of MIDI standard is moron!
			// effective is 14-bit value:
			int wheel = (int)ChannelCommand.PitchWheel | channel | loByte | hiByte;
			midiOutShortMsg(handle, wheel);
		} //ApplyWheel

		public void SetPercussionInstrument(GeneralMidiPercussiobInstrument instrument) {
			int program = (int)ChannelCommand.ProgramChange | (int)instrument << 8 | PercussionChannel;
			midiOutShortMsg(handle, program);
		} //SetPercussionInstrument

		public void PercussionOn(byte note, byte velocity) {
			NoteOn(note, velocity, PercussionChannel);
		} //PercussionOn

		public void PercussionOff(byte note, byte velocity) {
			NoteOff(note, velocity, PercussionChannel);
		} //PercussionOff

		public void NoteOn(byte note, byte velocity, byte channel) {
			int command = note << 8 | velocity << 2 * 8 | (int)ChannelCommand.NoteOn | channel;
			midiOutShortMsg(handle, command);
		} //NoteOn

		public void NoteOff(byte note, byte velocity, byte channel) {
			int command = (note << 8) | (velocity << 2 * 8) | (int)ChannelCommand.NoteOff | channel;
			midiOutShortMsg(handle, command);
		} //NoteOff

		public void NoteOn(byte note, byte velocity) {
			NoteOn(note, velocity, 0);
		} //NoteOn
		public void NoteOff(byte note, byte velocity) {
			NoteOff(note, velocity, 0);
		} //NoteOff

		public void NoteOn(byte note) {
			NoteOn(note, VelocityMax, 0);
		} //NoteOn
		public void NoteOff(byte note) {
			NoteOff(note, VelocityMax, 0);
		} //NoteOff

	} //class SimpleApi

} //namespace MidiOutApi.Api
