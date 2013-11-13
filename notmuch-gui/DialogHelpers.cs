using System;
using Gtk;

namespace NotMuchGUI
{
	public static class DialogHelpers
	{
		public static void ShowDialog(Window parent, MessageType msgType, string title, string text, params object[] args)
		{
			var dlg = new MessageDialog(parent, DialogFlags.Modal, msgType, ButtonsType.Ok,
				          text, args);
			dlg.Title = title;
			dlg.Run();
			dlg.Destroy();
		}
	}
}
