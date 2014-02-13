using System;
using System.IO;
using System.Linq;
using NM = NotMuch;
using System.Collections.Generic;

namespace NotMuchGUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MessageWidget : Gtk.Bin
	{
		WebKit.WebView m_webView;
		Gtk.ListStore m_attachmentStore;
		string m_msgFile;

		public MessageWidget()
		{
			this.Build();

			m_webView = new WebKit.WebView();
			m_webView.Editable = false;

			scrolledwindowWeb.Add(m_webView);

			labelFrom.Ellipsize = Pango.EllipsizeMode.End;
			labelTo.Ellipsize = Pango.EllipsizeMode.End;
			labelCc.Ellipsize = Pango.EllipsizeMode.End;
			labelSubject.Ellipsize = Pango.EllipsizeMode.End;

			attachmentNodeview.AppendColumn("Attachment", new Gtk.CellRendererText(), "text", 0);

			// filename, index
			m_attachmentStore = new Gtk.ListStore(typeof(string), typeof(int));
			attachmentNodeview.Model = m_attachmentStore;
		}

		public void Clear()
		{
			labelFrom.Text = "";
			labelTo.Text = "";
			labelCc.Text = "";
			labelSubject.Text = "";
			labelDate.Text = "";
			labelContentType.Text = "";
			labelCharset.Text = "";

			m_webView.LoadString("", null, null, null);
			this.HtmlContent = null;

			m_attachmentStore.Clear();
		}

		public void ShowEmail(NM.Message msg, GMime.Message gmsg)
		{
			m_msgFile = msg.FileName;
			ShowBody(msg, gmsg);
			ShowAttachments(msg, gmsg);
		}

		void ShowAttachments(NM.Message msg, GMime.Message gmsg)
		{
			m_attachmentStore.Clear();

			int idx = 0;

			foreach (var part in GMimeHelpers.GetAttachments(gmsg))
			{
				string filename = part.Filename;
				if (filename == null)
					filename = "attachment-" + System.IO.Path.GetFileName(System.IO.Path.GetTempFileName());

				m_attachmentStore.AppendValues(filename, idx);

				idx++;
			}

			// show/hide the parent, i.e. the ScolledWindow
			if (idx == 0)
				attachmentNodeview.Parent.HideAll();
			else
				attachmentNodeview.Parent.ShowAll();
		}

		void ShowBody(NM.Message msg, GMime.Message gmsg)
		{
			labelFrom.Text = msg.From;
			labelTo.Text = msg.To;
			labelCc.Text = msg.Cc;
			labelSubject.Text = msg.Subject;
			labelDate.Text = msg.Date.ToLocalTime().ToString("g");

			GMime.Part textpart = null;

			if (textpart == null)
				textpart = GMimeHelpers.FindFirstContent(gmsg, new GMime.ContentType("text", "html"));

			if (textpart == null)
				textpart = GMimeHelpers.FindFirstContent(gmsg, new GMime.ContentType("text", "plain"));

			if (textpart == null)
				textpart = GMimeHelpers.FindFirstContent(gmsg, new GMime.ContentType("text", "*"));

			if (textpart == null)
				throw new Exception();

			var html = PartToHtml(textpart);

			labelContentType.Text = textpart.ContentType.ToString();
			labelCharset.Text = textpart.ContentType.GetParameter("charset");

			this.HtmlContent = html;

			m_webView.LoadHtmlString(html, null);
		}

		public string HtmlContent { get; private set; }

		string PartToHtml(GMime.Part part)
		{
			byte[] buf;

			using (var memstream = new GMime.StreamMem())
			using (var filterstream = new GMime.StreamFilter(memstream))
			{
				filterstream.Add(new GMime.FilterCRLF(false, false));

				var charset = part.ContentType.GetParameter("charset");
				if (charset != null)
					filterstream.Add(new GMime.FilterCharset(charset, "utf-8"));

				if (!part.ContentType.IsType("text", "html"))
				{
					var flags = 0
				            //| GMimeHtmlFilterFlags.PRE
					            | GMimeHtmlFilterFlags.CONVERT_NL
					            | GMimeHtmlFilterFlags.MARK_CITATION;
					uint quoteColor = 0x888888;
					filterstream.Add(new GMime.FilterHTML((uint)flags, quoteColor));
				}

				part.ContentObject.WriteToStream(filterstream);

				filterstream.Flush();

				memstream.Seek(0);

				buf = new byte[memstream.Length];

				int l = memstream.Read(buf, (uint)buf.Length);
				if (l != buf.Length)
					throw new Exception();

				filterstream.Close();
				memstream.Close();
			}

			using (var reader = new StreamReader(new MemoryStream(buf), System.Text.UTF8Encoding.UTF8))
			{
				var str = reader.ReadToEnd();
				return str;
			}
		}

		public bool ShowHtmlSource
		{
			get { return m_webView.ViewSourceMode; }
			set
			{
				m_webView.ViewSourceMode = value;
				if (this.HtmlContent != null)
					m_webView.LoadHtmlString(this.HtmlContent, null);
			}
		}

		protected void OnAttachmentNodeviewRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			Gtk.TreeIter iter;
			m_attachmentStore.GetIter(out iter, args.Path);
			int idx = (int)m_attachmentStore.GetValue(iter, 1);
			SaveAttachment(idx);
		}

		void SaveAttachment(int targetIdx)
		{
			int fd = Mono.Unix.Native.Syscall.open(m_msgFile, Mono.Unix.Native.OpenFlags.O_RDONLY);

			using (var readStream = new GMime.StreamFs(fd))
			{
				readStream.Owner = true;

				var p = new GMime.Parser(readStream);
				var gmsg = p.ConstructMessage();

				int idx = 0;
				foreach (var part in GMimeHelpers.GetAttachments(gmsg))
				{
					if (targetIdx != -1 && targetIdx != idx)
					{
						idx++;
						continue;
					}

					byte[] buf;

					using (var memstream = new GMime.StreamMem())
					{
						int l = part.ContentObject.WriteToStream(memstream);

						if (l < 0)
							throw new Exception();

						memstream.Seek(0);

						buf = new byte[memstream.Length];

						l = memstream.Read(buf, (uint)buf.Length);
						if (l != buf.Length)
							throw new Exception();

						memstream.Close();
					}

					string filename = part.Filename;
					if (filename == null)
						filename = "attachment-" + System.IO.Path.GetFileName(System.IO.Path.GetTempFileName());

					var fullPath = "/tmp/" + filename;
					File.WriteAllBytes(fullPath, buf);

					CmdHelpers.LaunchDefaultApp(fullPath);

					idx++;

					if (targetIdx != -1)
						break;
				}

				// GMime.StreamFs is buggy. Dispose doesn't close the fd.
				readStream.Close();
			}
		}
	}
}

