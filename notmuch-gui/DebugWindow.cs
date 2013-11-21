using System;

namespace NotMuchGUI
{
	public partial class DebugWindow : Gtk.Window
	{
		Gtk.TextView textviewSrc;
		Gtk.TextView textviewDump;

		public DebugWindow() : 
			base(Gtk.WindowType.Toplevel)
		{
			var box = new Gtk.Box(Gtk.Orientation.Vertical, 2);
			Add(box);

			textviewSrc = new Gtk.TextView();
			box.Add(textviewSrc);

			textviewDump = new Gtk.TextView();
			box.Add(textviewDump);
		}

		public void SetDump(string txt)
		{
			textviewDump.Buffer.Text = txt;
		}

		public void SetSrc(string txt)
		{
			textviewSrc.Buffer.Text = txt;
		}
	}
}

