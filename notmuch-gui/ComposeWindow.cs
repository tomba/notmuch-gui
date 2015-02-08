using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Gtk;
using MK = MimeKit;
using UI = Gtk.Builder.ObjectAttribute;

namespace NotMuchGUI
{
	public class ComposeWindow : Window
	{
		public static ComposeWindow Create()
		{
			var builder = new Builder(null, "NotMuchGUI.UI.ComposeWindow.ui", null);
			var wnd = new ComposeWindow(builder, builder.GetObject("ComposeWindow").Handle);
			return wnd;
		}

		[UI] readonly Box box1;
		[UI] readonly Button editButton;
		[UI] readonly Button sendButton;

		MK.MimeMessage m_message;

		Gtk.Socket m_gtkSocket;


		ComposeWindow(Builder builder, IntPtr handle)
			: base(handle)
		{
			builder.Autoconnect(this);

			this.WidthRequest = 800;
			this.HeightRequest = 600;

			editButton.Clicked += OnEditButtonClicked;
			sendButton.Clicked += OnSendButtonClicked;
		}

		public void New()
		{
			var msg = new MK.MimeMessage();
			msg.From.Add(Globals.MyAddresses.First());	// XXX
			msg.Body = new MK.TextPart() { Text = "" };

			m_message = msg;

			Edit();
		}

		public void Reply(string msgId, bool replyAll)
		{
			if (msgId == null)
				return;

			MK.MimeMessage msg;

			using (var cdb = new CachedDB())
			{
				var db = cdb.Database;

				var nmmsg = db.FindMessage(msgId);
				var filename = nmmsg.FileName;

				msg = MK.MimeMessage.Load(filename);
			}

			var reply = MimeKitHelpers.CreateReply(msg, replyAll);

			m_message = reply;

			Edit();
		}

		static int FindFirstBlankLine(string filename)
		{
			using (var reader = new StreamReader(filename))
			{
				string str;
				int n = 0;

				while ((str = reader.ReadLine()) != null)
				{
					n++;

					if (str.Length == 0)
						return n;
				}
			}

			return 0;
		}

		void View()
		{
			editButton.Sensitive = true;
			sendButton.Sensitive = true;

			foreach (var widget in box1.Children)
				box1.Remove(widget);

			var messagewidget = new MessageWidget();
			box1.Add(messagewidget);

			messagewidget.ShowEmail(m_message, null, null);

			messagewidget.ShowAll();
		}

		string WriteMessageToTmpFile()
		{
			var tmpFile = System.IO.Path.GetTempFileName();

			using (var writer = File.CreateText(tmpFile))
			{
				writer.WriteLine("From: {0}", m_message.From);
				writer.WriteLine("To: {0}", m_message.To);
				if (m_message.Cc.Count > 0)
					writer.WriteLine("Cc: {0}", m_message.Cc);
				writer.WriteLine("Subject: {0}", m_message.Subject);
				writer.WriteLine();
				writer.Write(((MK.TextPart)m_message.Body).Text);
			}

			return tmpFile;
		}

		void Edit()
		{
			editButton.Sensitive = false;
			sendButton.Sensitive = false;

			var tmpFile = WriteMessageToTmpFile();

			foreach (var widget in box1.Children)
				box1.Remove(widget);

			m_gtkSocket = new Gtk.Socket();
			m_gtkSocket.Expand = true;
			m_gtkSocket.CanFocus = true;
			m_gtkSocket.PlugAdded += (o, e) =>
			{
				// https://bugzilla.gnome.org/show_bug.cgi?id=729248

				m_gtkSocket.ChildFocus(DirectionType.TabForward);
			};
			m_gtkSocket.PlugRemoved += (o, e) =>
			{
				ReadEditedMail(tmpFile);
			};

			box1.Add(m_gtkSocket);
			m_gtkSocket.ShowAll();

			int lineNum = FindFirstBlankLine(tmpFile) + 1;

			const string editorCmd = "gvim";
			const string editorArgs = "-f '+set filetype=mail' '+set fileencoding=utf-8' '+set ff=unix' '+set enc=utf-8' +{1} {0} --socketid {2}";

			var process = new Process();

			var si = process.StartInfo;
			si.FileName = editorCmd;
			si.Arguments = String.Format(editorArgs, tmpFile, lineNum, m_gtkSocket.Id);
			si.UseShellExecute = false;
			si.CreateNoWindow = true;

			process.EnableRaisingEvents = true;

			bool b = process.Start();
			if (!b)
				throw new Exception("Failed to edit start edit process");
		}

		void ReadEditedMail(string tmpFile)
		{
			using (var reader = File.OpenText(tmpFile))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					if (line.Length == 0)
						break;

					var parts = line.Split(new[] { ": " }, 2, StringSplitOptions.RemoveEmptyEntries);

					switch (parts.Length)
					{
					case 1:
						// empty
						continue;

					case 2:
						// normal
						break;

					default:
						throw new Exception("bad header");
					}

					MK.HeaderId hid;

					switch (parts[0])
					{
					case "To":
						hid = MK.HeaderId.To;
						break;

					case "From":
						hid = MK.HeaderId.From;
						break;

					case "Cc":
						hid = MK.HeaderId.Cc;
						break;

					case "Subject":
						hid = MK.HeaderId.Subject;
						break;

					default:
						continue;
					}


					m_message.Headers[hid] = parts[1];
				}

				var bodyText = reader.ReadToEnd();
				var part = new MK.TextPart();
				part.SetText(Encoding.UTF8, bodyText);

				m_message.Body = part;
			}

			File.Delete(tmpFile);

			View();
		}

		void OnEditButtonClicked(object sender, EventArgs args)
		{
			Edit();
		}

		void OnSendButtonClicked(object sender, EventArgs e)
		{
			var exe = MainClass.AppKeyFile.GetStringOrNull("send", "cmd");
			if (exe == null)
				throw new Exception();

			var tmpFile = System.IO.Path.GetTempFileName();

			using (var stream = File.OpenWrite(tmpFile))
				m_message.WriteTo(stream);

			var dlg = TermDialog.Create();
			dlg.ParentWindow = this.RootWindow;
			dlg.Start(exe, tmpFile);
			var resp = (ResponseType)dlg.Run();

			Console.WriteLine("got resp {0}", resp);

			dlg.Destroy();

			File.Delete(tmpFile);
		}
	}
}
