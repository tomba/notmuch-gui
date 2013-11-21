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

		public static string NotmuchExe { get; private set; }

		static System.Threading.Thread s_mainThread;

		public static void Main(string[] args)
		{
			Application.Init();

			s_mainThread = System.Threading.Thread.CurrentThread;

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

			Builder builder = new Builder(null, "NotMuchGUI.UI.MainWindow.ui", null);
			MainWindow win = new MainWindow(builder, builder.GetObject("MainWindow").Handle);
			win.Show();

			Application.Run();
		}

		public static void VerifyThread()
		{
			if (System.Threading.Thread.CurrentThread == s_mainThread)
				return;

			Console.WriteLine("BAD THREAD");
			throw new Exception();
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

		public static NM.Database OpenDB()
		{
			NM.Status status;

			var db = NM.Database.Open(MainClass.DatabasePath, NM.DatabaseMode.READ_ONLY, out status);

			if (db == null)
			{
				// XXX this doesn't work if it happens on another thread...

				DialogHelpers.ShowDialog(null, MessageType.Error, "Failed to open database", "Failed to open database\n'{0}':\n\n{1}",
					MainClass.DatabasePath, status);
				Application.Quit();
			}

			//Debug.WriteLine("Opened database '{0}'", db);

			return db;
		}
	}

	class CachedDB : IDisposable
	{
		static NM.Database s_db;
		static int s_dbRefs;

		public NM.Database Database { get; private set; }

		public CachedDB()
		{
			MainClass.VerifyThread();
			this.Database = GetCachedDB();
		}

		public void Dispose()
		{
			MainClass.VerifyThread();
			PutCachedDB();
			GC.SuppressFinalize(this);
		}

		~CachedDB()
		{
			PutCachedDB();
		}

		static NM.Database GetCachedDB()
		{
			MainClass.VerifyThread();

			if (s_db == null)
			{
				//Console.WriteLine("open DB");
				s_db = MainClass.OpenDB();

				GLib.Timeout.Add(1000, () =>
					{
						if (s_dbRefs > 0)
							return true;

						//Console.WriteLine("close DB");

						s_db.Dispose();
						s_db = null;
						return false;
					});
			}

			s_dbRefs++;
			return s_db;
		}

		static void PutCachedDB()
		{
			s_dbRefs--;
			if (s_dbRefs < 0)
				throw new Exception();
		}
	}
}
