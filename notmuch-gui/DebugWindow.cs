using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace NotMuchGUI
{
	public class DebugWindow : Window
	{
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

