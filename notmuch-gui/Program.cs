using System;
using Gtk;
using NM = NotMuch;

namespace NotMuchGUI
{
	static class MainClass
	{
		public static NM.Database Database { get; private set; }

		public static void Main(string[] args)
		{
			string path;

			if (args.Length > 1)
				path = args[0];
			else
				path = "/home/tomba/Maildir";

			NM.Status status;

			MainClass.Database = NM.Database.Open(path, NM.DatabaseMode.READ_ONLY, out status);

			if (MainClass.Database == null)
			{
				Console.WriteLine("Failed to open database '{0}': {1}", path, status);
				return;
			}

			Application.Init();

			System.Threading.SynchronizationContext.SetSynchronizationContext(new GLib.GLibSynchronizationContext());

			MainWindow win = new MainWindow();
			win.Show();

			Application.Run();
		}
	}
}
