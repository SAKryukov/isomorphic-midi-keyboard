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
	using System;
	using System.Windows;
	using System.Reflection;
	using StringList = System.Collections.Generic.List<string>;
	using PublicationList = System.Collections.Generic.List<AssemblyPublicationAttribute>;
	using IStringList = System.Collections.Generic.IList<string>;
	using StringBuilder = System.Text.StringBuilder;
	using ImageSource = System.Windows.Media.ImageSource;

	class TheApplication : Application {

		internal TheApplication() {
			DispatcherUnhandledException += (sender, eventArgs) => {
				ShowException(eventArgs.Exception);
				eventArgs.Handled = true;
			}; //DispatcherUnhandledException
		} //TheApplication

		internal static new TheApplication Current;

		protected override void OnStartup(StartupEventArgs e) {
			this.ShutdownMode = ShutdownMode.OnMainWindowClose;
			MainWindow = new Ui.MainWindow();
			MainWindow.Title = ProductName;
			MainWindow.Show();
			startupComplete = true;
		} //OnStartup

		void ShowException(Exception e) {
			Func<Exception, string> exceptionTextFinder = (ex) => {
				Action<Exception, IStringList> exceptionTextCollector = null; // for recursiveness
				exceptionTextCollector = (exc, aList) => {
					aList.Add(string.Format(WPF.Resources.Exceptions.ExceptionFormat, exc.GetType().FullName, exc.Message));
					if (exc.InnerException != null)
						exceptionTextCollector(exc.InnerException, aList);
				}; //exceptionTextCollector
				IStringList list = new StringList();
				exceptionTextCollector(ex, list);
				StringBuilder sb = new StringBuilder();
				bool first = true;
				foreach (string item in list)
					if (first) {
						sb.Append(item);
						first = false;
					} else
						sb.Append(WPF.Resources.Exceptions.ExceptionStackItemDelimiter + item);
				return sb.ToString();
			}; //exceptionTextFinder
			MessageBox.Show(
				exceptionTextFinder(e),
				ProductName,
				MessageBoxButton.OK,
				MessageBoxImage.Error);
			if (!startupComplete)
				Shutdown();
		} //ShowException

		[STAThread]
		static void Main() {
			using (var iconStream = new System.IO.MemoryStream()) {
				TheApplication app = new TheApplication();
				Current = app;
				WPF.Resources.Main.IconMain.Save(iconStream);
				iconStream.Seek(0, System.IO.SeekOrigin.Begin);
				app.ApplicationIcon = System.Windows.Media.Imaging.BitmapFrame.Create(iconStream);
				app.Run();
			} //using
		} //Main

		internal string ProductName {
			get {
				if (productName == null) {
					object[] attributes = TheAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
					if (attributes == null) return null;
					if (attributes.Length < 1) return null;
					productName = ((AssemblyProductAttribute)attributes[0]).Product;
				} //if
				return productName;
			} //get ProductName
		} //ProductName
		internal string Copyright {
			get {
				if (copyright == null) {
					object[] attributes = TheAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
					if (attributes == null) return null;
					if (attributes.Length < 1) return null;
					copyright = ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
				} //if
				return copyright;
			} //get Copyright
		} //Copyright
		internal string Company {
			get {
				if (company == null) {
					object[] attributes = TheAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
					if (attributes == null) return null;
					if (attributes.Length < 1) return null;
					company = ((AssemblyCompanyAttribute)attributes[0]).Company;
				} //if
				return company;
			} //get Company
		} //Company
		internal string[] PersonalUri {
			get {
				if (personalUri == null) {
					object[] attributes = TheAssembly.GetCustomAttributes(typeof(AssemblyPersonalUriAttribute), false);
					if (attributes == null) return null;
					if (attributes.Length < 1) return null;
					StringList list = new StringList();
					foreach (var attribute in attributes)
						list.Add(((AssemblyPersonalUriAttribute)attribute).Value);
					personalUri = list.ToArray();
				} //if
				return personalUri;
			} //get PersonalUri
		} //PersonalUri
		internal AssemblyPublicationAttribute[] Publications {
			get {
				if (publications == null) {
					object[] attributes = TheAssembly.GetCustomAttributes(typeof(AssemblyPublicationAttribute), false);
					if (attributes == null) return null;
					if (attributes.Length < 1) return null;
					var list = new PublicationList();
					foreach (var attribute in attributes)
						list.Add(((AssemblyPublicationAttribute)attribute));
					return list.ToArray();
				} //if
				return publications;
			} //get Publications
		} //Publications
		internal Version AssemblyVersion {
			get {
				if (assemblyVersion == null) {
					object[] attributes = TheAssembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
					if (attributes == null) return null;
					if (attributes.Length < 1) return null;
					assemblyVersion = new Version(((AssemblyFileVersionAttribute)attributes[0]).Version);
				} //if
				return assemblyVersion;
			} //get AssemblyVersion
		} //AssemblyVersion
		internal Version AssemblyInformationalVersion {
			get {
				if (assemblyVersion == null) {
					object[] attributes = TheAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
					if (attributes == null) return null;
					if (attributes.Length < 1) return null;
					assemblyVersion = new Version(((AssemblyInformationalVersionAttribute)attributes[0]).InformationalVersion);
				} //if
				return assemblyVersion;
			} //get AssemblyVersion
		} //AssemblyVersion

		Assembly TheAssembly {
			get {
				if (assembly == null)
					assembly = Assembly.GetEntryAssembly();
				return assembly;
			} //get TheAssembly
		} //TheAssembly

		bool startupComplete;
		Assembly assembly;
		string productName, copyright, company;
		string[] personalUri;
		AssemblyPublicationAttribute[] publications;
		Version assemblyVersion;
		internal ImageSource ApplicationIcon { get; private set; } 

	} //class TheApplication

} //namespace WPF.Main
