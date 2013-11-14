using System;
using Gtk;
using NM = NotMuch;
using System.IO;
using System.Diagnostics;

namespace NotMuchGUI
{
	static class MainClass
	{
		public static string DatabasePath { get; private set; }
		public static NM.Database Database { get; private set; }

		public static string NotmuchExe { get; private set; }

		public static void Main(string[] args)
		{
			Application.Init();

			try
			{
				ParseConfig();
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

			NM.Status status;

			MainClass.Database = NM.Database.Open(MainClass.DatabasePath, NM.DatabaseMode.READ_ONLY, out status);

			if (MainClass.Database == null)
			{
				DialogHelpers.ShowDialog(null, MessageType.Error, "Failed to open database", "Failed to open database\n'{0}':\n\n{1}",
					MainClass.DatabasePath, status);
				return;
			}

			Debug.WriteLine("Opened database '{0}'", MainClass.DatabasePath);

			System.Threading.SynchronizationContext.SetSynchronizationContext(new GLib.GLibSynchronizationContext());

			MainWindow win = new MainWindow();
			win.Show();

			Application.Run();

			MainClass.Database.Dispose();
			MainClass.Database = null;
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
