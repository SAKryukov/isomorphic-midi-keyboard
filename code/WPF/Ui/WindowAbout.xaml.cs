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
	using System.Windows.Documents;
	using Process = System.Diagnostics.Process;

	public partial class WindowAbout : Window {

		public WindowAbout() {			
			InitializeComponent();
			var app = Main.TheApplication.Current;
			var version = app.AssemblyVersion;
			Title = app.ProductName;
			Icon = app.ApplicationIcon;
			buttonOk.Click += (sender, eventArgs) => { Hide(); };
			textBlockProduct.Text = string.Format("{0} v.{1}.{2}", app.ProductName, version.Major, version.Minor);
			textBlockCopyright.Text = app.Copyright;
			string[] personalUris = app.PersonalUri;
			if (personalUris != null) {
				if (personalUris.Length > 0)
					SetHyperlink(linkSa, new System.Uri(personalUris[0]), personalUris[0]);
				if (personalUris.Length > 1)
					SetHyperlink(linkSaCodeProject, new System.Uri(personalUris[1]), personalUris[1]);
			} //if
			AssemblyPublicationAttribute[] publications = app.Publications;
			if (publications != null && publications.Length > 0) {
				SetHyperlink(linkArticle, new System.Uri(publications[0].Uri), publications[0].Title);
			} //if
		} //WindowAbout

		void SetHyperlink(Hyperlink target, Uri uri, string text) {
			if (text == null || text.Trim().Length < 0)
				text = uri.ToString();
			target.Inlines.Clear();
			target.Inlines.Add(text);
			target.NavigateUri = uri;
			target.Click += (s, e) => { Process.Start(uri.ToString()); };
		} //SetHyperlink

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
			e.Cancel = true;
			Hide();
		} //OnClosing

	} //class WindowAbout

} //namespace WPF.Ui 
