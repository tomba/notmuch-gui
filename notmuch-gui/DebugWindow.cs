using System;

namespace NotMuchGUI
{
	public partial class DebugWindow : Gtk.Window
	{
		public DebugWindow() : 
			base(Gtk.WindowType.Toplevel)
		{
			this.Build();
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

