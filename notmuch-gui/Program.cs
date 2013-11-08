using System;
using Gtk;

namespace NotMuchGUI
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Application.Init();

			System.Threading.SynchronizationContext.SetSynchronizationContext(new GLib.GLibSynchronizationContext());

			MainWindow win = new MainWindow();
			win.Show();

			Application.Run();
		}
	}
}