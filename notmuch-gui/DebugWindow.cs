using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace NotMuchGUI
{
	public class DebugWindow : Window
	{
		public static DebugWindow Create()
		{
			var builder = new Builder(null, "NotMuchGUI.UI.DebugWindow.ui", null);
			var dlg = new DebugWindow(builder, builder.GetObject("DebugWindow").Handle);
			return dlg;
		}

		[UI] readonly TextView textviewDump;
		[UI] readonly TextView textviewSrc;

		public DebugWindow(Builder builder, IntPtr handle) : base(handle)
		{
			builder.Autoconnect(this);
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

