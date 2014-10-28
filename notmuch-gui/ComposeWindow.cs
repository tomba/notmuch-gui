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

		[UI] readonly Box vbox2;
		[UI] readonly Button editButton;
		[UI] readonly Button sendButton;

		MK.MimeMessage m_message;

		MessageWidget messagewidget1;

		ComposeWindow(Builder builder, IntPtr handle) : base(handle)
		{
			builder.Autoconnect(this);

			messagewidget1 = new MessageWidget();
			vbox2.Add(messagewidget1);

			editButton.Clicked += OnEditButtonClicked;
			sendButton.Clicked += OnSendButtonClicked;
		}

		public MK.MimeMessage Message
		{
			get { return m_message; }
			set
			{
				m_message = value;
				messagewidget1.ShowEmail(m_message, null, null);
			}
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

			this.Message = reply;
		}

		public void New()
		{
			var msg = new MK.MimeMessage();
			msg.From.Add(Globals.MyAddresses.First());	// XXX
			msg.Body = new MK.TextPart() { Text = "" };

			this.Message = msg;
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

		void OnEditButtonClicked(object sender, EventArgs args)
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

			int lineNum = FindFirstBlankLine(tmpFile) + 1;

			const string editorCmd = "gvim";
			const string editorArgs = "-f \"+set columns=100\" \"+set lines=50\" \"+set filetype=mail\" +{1} {0}";

			using (var process = new Process())
			{
				var si = process.StartInfo;
				si.FileName = editorCmd;
				si.Arguments = String.Format(editorArgs, tmpFile, lineNum);
				si.UseShellExecute = false;
				si.CreateNoWindow = true;

				var dlg = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Cancel,
					          "Editing.\n\nEditor command {0}\n\nPress cancel to kill the editor.", si.Arguments);

				process.EnableRaisingEvents = true;

				process.Exited += (s, a) =>
				{
					Application.Invoke((_s, _e) =>
					{
						dlg.Destroy();
					});
				};

				dlg.Response += (o, e) =>
				{
					process.Kill();
				};

				bool b = process.Start();
				if (!b)
					Console.WriteLine("Failed to edit start edit process");

				dlg.Run();

				process.WaitForExit();

				if (process.ExitCode != 0)
					Console.WriteLine("Failed to edit reply");
			}

			using (var reader = File.OpenText(tmpFile))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					if (line.Length == 0)
						break;

					var parts = line.Split(new[] {": "}, 2, StringSplitOptions.RemoveEmptyEntries);

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

				this.Message = m_message;
			}

			File.Delete(tmpFile);
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
		}
	}
}

