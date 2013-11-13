using System;
using Gtk;
using NM = NotMuch;
using System.IO;

namespace NotMuchGUI
{
	static class MainClass
	{
		public static NM.Database Database { get; private set; }
		public static string NotmuchExe { get; private set; }

		public static void Main(string[] args)
		{
			ParseConfig();

			string path;
			string output;

			if (CmdHelpers.RunNotmuch("config get database.path", out output) == false)
			{
				output = output.Trim();
				Console.WriteLine("Failed to get database path: {0}", output);
				return;
			}

			output = output.Trim();

			path = output;

			NM.Status status;

			MainClass.Database = NM.Database.Open(path, NM.DatabaseMode.READ_ONLY, out status);

			if (MainClass.Database == null)
			{
				Console.WriteLine("Failed to open database '{0}': {1}", path, status);
				return;
			}

			Console.WriteLine("Opened database '{0}'", path);

			Application.Init();

			System.Threading.SynchronizationContext.SetSynchronizationContext(new GLib.GLibSynchronizationContext());

			MainWindow win = new MainWindow();
			win.Show();

			Application.Run();
		}

		static void ParseConfig()
		{
			MainClass.NotmuchExe = "notmuch";

			var home = Environment.GetEnvironmentVariable("HOME");

			var filename = Path.Combine(home, ".notmuch-gui-config");

			if (File.Exists(filename) == false)
				return;

			var keyfile = new KeyFile.GKeyFile(filename);

			MainClass.NotmuchExe = keyfile.GetString("notmuch", "executable");
		}
	}
}
