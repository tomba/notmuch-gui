using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;
using System.IO;

namespace NotMuchGUI
{
	public class MessageSourceWindow : Window
	{
		public static MessageSourceWindow Create()
		{
			var builder = new Builder(null, "NotMuchGUI.UI.MessageSourceWindow.ui", null);
			var dlg = new MessageSourceWindow(builder, builder.GetObject("MessageSourceWindow").Handle);
			return dlg;
		}

		[UI] readonly TextView msgSourceTextView;
		[UI] readonly TextView msgStructureTextView;

		public MessageSourceWindow(Builder builder, IntPtr handle) : base(handle)
		{
			builder.Autoconnect(this);
			this.SetDefaultSize(800, 600);
		}

		public void ShowMessage(string messageID)
		{
			string filename;

			using (var cdb = new CachedDB())
			{
				var msg = cdb.Database.FindMessage(messageID);
				filename = msg.FileName;
			}

			msgSourceTextView.Buffer.Text = File.ReadAllText(filename);

			MimeKit.MimeMessage mkmsg;

			try
			{
				mkmsg = MimeKit.MimeMessage.Load(filename);

				var sb = new System.Text.StringBuilder();
				MimeKitHelpers.DumpMessage(mkmsg, sb);
				msgStructureTextView.Buffer.Text = sb.ToString();
			}
			catch (Exception exc)
			{
				msgStructureTextView.Buffer.Text  = string.Format("Failed to parse message from '{0}':\n{1}", filename, exc.Message);
			}
		}
	}
}

