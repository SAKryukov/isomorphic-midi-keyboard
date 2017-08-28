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
using System;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class AssemblyPersonalUriAttribute : Attribute {
	public AssemblyPersonalUriAttribute(string value) { this.Value = value; }
	public string Value { get; private set; }
} //AssemblyPersonalUriAttribute

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class AssemblyPublicationAttribute : Attribute {
	public AssemblyPublicationAttribute(string title, string Uri) { this.Title = title; this.Uri = Uri; }
	public string Title { get; private set; }
	public string Uri { get; private set; }
} //AssemblyPublicationAttribute

