using System;
using System.IO;
using System.Diagnostics;
using Gtk;
using MK = MimeKit;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace NotMuchGUI
{
	public partial class ComposeWindow : Gtk.Window
	{
		MK.MimeMessage m_message;

		public ComposeWindow() : 
			base(Gtk.WindowType.Toplevel)
		{
			this.Build();
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

			var reply = new MK.MimeMessage();

			reply.InReplyTo = msg.MessageId;

			foreach (var r in msg.References)
				reply.References.Add(r);
			reply.References.Add(msg.MessageId);

			reply.Subject = "Re: " + msg.Subject;

			reply.From.Add(Globals.MyAddresses.First());	// XXX
			reply.To.AddRange(msg.From);

			if (replyAll)
			{
				var comparer = new AddressComparer();

				reply.To.AddRange(msg.To.Except(Globals.MyAddresses, comparer));

				var cc = msg.Cc
					.Except(Globals.MyAddresses, comparer)
					.Except(reply.To, comparer).ToArray();

				if (cc.Length > 0)
					reply.Cc.AddRange(cc);
			}

			var textpart = msg.BodyParts.OfType<MK.TextPart>()
				.FirstOrDefault(p => p.ContentType.Matches("text", "plain"));

			var sb = new StringBuilder();

			sb.AppendFormat("On {1}, {0} wrote:\n\n",
				msg.From.First().Name,
				msg.Date.ToString("u"));

			using (StringReader reader = new StringReader(textpart.Text))
			{
				string prefix = "> ";
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					sb.Append(prefix);
					sb.AppendLine(line);
				}
			}

			reply.Body = new MK.TextPart("plain")
			{
				Text = sb.ToString(),
			};

			messagewidget1.ShowEmail(reply, "", "");

			m_message = reply;
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

		protected void OnEditButtonClicked(object sender, EventArgs args)
		{
			var tmpFile = System.IO.Path.GetTempFileName();

			var formatOptions = new MK.FormatOptions();
			formatOptions.HiddenHeaders.Add(MK.HeaderId.MimeVersion);
			formatOptions.HiddenHeaders.Add(MK.HeaderId.MessageId);
			formatOptions.HiddenHeaders.Add(MK.HeaderId.ContentType);

			var savedMsg = new MK.MimeMessage();
			savedMsg.Subject = m_message.Subject;
			savedMsg.Body = m_message.Body;
			savedMsg.From.AddRange(m_message.From);
			savedMsg.To.AddRange(m_message.To);
			if (m_message.Cc.Count > 0)
				savedMsg.Cc.AddRange(m_message.Cc);

			using (var stream = File.OpenWrite(tmpFile))
				savedMsg.WriteTo(formatOptions, stream);

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

				process.Start();

				dlg.Run();

				process.WaitForExit();

				if (process.ExitCode != 0)
					Console.WriteLine("Failed to edit reply");
			}

			var loadedMsg = MK.MimeMessage.Load(tmpFile);

			((MK.TextPart)m_message.Body).Text = ((MK.TextPart)loadedMsg.Body).Text;
			m_message.Subject = loadedMsg.Subject;
			m_message.To.Clear();
			m_message.To.AddRange(loadedMsg.To);
			m_message.From.Clear();
			m_message.From.AddRange(loadedMsg.From);
			m_message.Cc.Clear();
			m_message.Cc.AddRange(loadedMsg.Cc);

			messagewidget1.ShowEmail(m_message, "", "");

			File.Delete(tmpFile);
		}
	}
}

