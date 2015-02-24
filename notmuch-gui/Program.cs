using System;
using System.Linq;
using Gtk;
using NM = NotMuch;
using System.IO;

namespace NotMuchGUI
{
	static class MainClass
	{
		public static string DatabasePath { get; private set; }

		public static string NotmuchExe { get; private set; }

		static System.Threading.Thread s_mainThread;

		public static MyKeyFile AppKeyFile { get; private set; }

		public static MainWindow MainWindow { get; private set; }

		public static void Main(string[] args)
		{
			Application.Init();

			LoadCustomTheme();

			s_mainThread = System.Threading.Thread.CurrentThread;

			try
			{
				LoadConfig();
			}
			catch (Exception e)
			{
				DialogHelpers.ShowDialog(null, MessageType.Error, "Failed to parse config", "Failed to parse config file:\n\n{0}", e.Message);
				return;
			}

			string output;

			if (CmdHelpers.RunNotmuch("config get database.path", out output) == false)
			{
				output = output.Trim();
				DialogHelpers.ShowDialog(null, MessageType.Error, "Failed to get database path", "Failed to get database path:\n\n{0}", output);
				return;
			}

			MainClass.DatabasePath = output.Trim();

			var builder = new Builder(null, "NotMuchGUI.UI.MainWindow.ui", null);
			MainClass.MainWindow = new MainWindow(builder, builder.GetObject("MainWindow").Handle);
			MainClass.MainWindow.ShowAll();
			
			Application.Run();
		}

		static void LoadCustomTheme()
		{
			var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "notmuch-gui.css");

			if (System.IO.File.Exists(path) == false)
				return;

			var prov = new CssProvider();
			prov.LoadFromPath(path);

			StyleContext.AddProviderForScreen(Gdk.Screen.Default, prov, 600);
		}

		public static void VerifyThread()
		{
			if (System.Threading.Thread.CurrentThread == s_mainThread)
				return;

			Console.WriteLine("BAD THREAD");
			throw new Exception();
		}

		static void LoadConfig()
		{
			MainClass.NotmuchExe = "notmuch";

			var home = Environment.GetEnvironmentVariable("HOME");

			var filename = Path.Combine(home, ".notmuch-gui-config");

			if (File.Exists(filename) == false)
			{
				using (File.Create(filename))
					;
			}

			MainClass.AppKeyFile = new MyKeyFile(filename);

			var exe = MainClass.AppKeyFile.GetStringOrNull("notmuch", "executable");
			if (exe != null)
				MainClass.NotmuchExe = exe;

			/* collect user's email addresses */
			var emails =
				from section in MainClass.AppKeyFile.Sections
				where section.Name == "identity"
				let name = section.FindFirstValue("name")
				from kvp in section.KeyValues
				where kvp.Key == "email"
				select new MimeKit.MailboxAddress(name, kvp.Value);

			Globals.MyAddresses = emails.ToArray();
		}
	}
}
