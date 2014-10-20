using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NM = NotMuch;
using MK = MimeKit;
using UI = Gtk.Builder.ObjectAttribute;
using Gtk;

namespace NotMuchGUI
{
	public class MessageWidget : Gtk.Bin
	{
		WebKit.WebView m_webView;
		Gtk.ListStore m_attachmentStore;
		string m_msgFile;

		public string HtmlContent { get; private set; }

		[UI] Gtk.Label labelFrom;
		[UI] Gtk.Label labelTo;
		[UI] Gtk.Label labelCc;
		[UI] Gtk.Label labelSubject;
		[UI] Gtk.Label labelDate;
		[UI] Gtk.Label labelMsgID;
		[UI] Gtk.Label labelContent;
		[UI] Gtk.Label labelThreadID;

		[UI] Gtk.ScrolledWindow scrolledwindowWeb;

		Gtk.NodeView attachmentNodeview;

		[UI] Box hbox1;

		public MessageWidget()
		{
			Builder builder = new Gtk.Builder (null, "NotMuchGUI.UI.MessageWidget.ui", null);
			builder.Autoconnect (this);
			Add ((Box) builder.GetObject ("MessageWidget"));

			m_webView = new WebKit.WebView();
			m_webView.Editable = false;

			//scrolledwindowWeb.Add(m_webView);
			hbox1.Add(m_webView); // XXX

			labelFrom.Ellipsize = Pango.EllipsizeMode.End;
			labelTo.Ellipsize = Pango.EllipsizeMode.End;
			labelCc.Ellipsize = Pango.EllipsizeMode.End;
			labelSubject.Ellipsize = Pango.EllipsizeMode.End;


			m_webView.LoadHtmlString("ASDASDASDA\n", null);

			//attachmentNodeview.AppendColumn("Attachment", new Gtk.CellRendererText(), "text", 0);

			// filename, index
			//m_attachmentStore = new Gtk.ListStore(typeof(string), typeof(int));
			//attachmentNodeview.Model = m_attachmentStore;
		}

		public void Clear()
		{
			labelFrom.Text = "";
			labelTo.Text = "";
			labelCc.Text = "";
			labelSubject.Text = "";
			labelDate.Text = "";
			labelMsgID.Text = "";
			labelContent.Text = "";
			labelThreadID.Text = "";

			m_webView.LoadString("", null, null, null);
			this.HtmlContent = null;

			m_attachmentStore.Clear();
		}

		public void ShowEmail(MK.MimeMessage msg, string filename, string threadID)
		{
			m_msgFile = filename;

			ShowBody(msg, threadID);
			ShowAttachments(msg);
		}

		void ShowAttachments(MK.MimeMessage msg)
		{
			m_attachmentStore.Clear();

			int idx = 0;

			foreach (var part in msg.Attachments)
			{
				string filename = part.FileName;
				if (filename == null)
					filename = "attachment-" + System.IO.Path.GetFileName(System.IO.Path.GetTempFileName());

				m_attachmentStore.AppendValues(filename, idx);

				idx++;
			}

			// show/hide the parent, i.e. the ScolledWindow
			if (idx == 0)
				attachmentNodeview.Parent.Hide();
			else
				attachmentNodeview.Parent.ShowAll();
		}

		void ShowBody(MK.MimeMessage msg, string threadID)
		{
			labelFrom.Text = msg.From.ToString();
			labelTo.Text = msg.To.ToString();
			labelCc.Text = msg.Cc.ToString();
			labelSubject.Text = msg.Subject;
			labelDate.Text = msg.Date.ToLocalTime().ToString("g");
			labelMsgID.Text = "id:" + msg.MessageId;

			if (string.IsNullOrEmpty(threadID))
			{
				labelThreadID.Visible = false;
			}
			else
			{
				labelThreadID.Text = "thread:" + threadID;
				labelThreadID.Visible = true;
			}

			var textParts = msg.BodyParts.OfType<MK.TextPart>();

			MK.TextPart textpart = null;

			if (textpart == null)
				textpart = textParts.FirstOrDefault(p => p.ContentType.Matches("text", "html"));

			if (textpart == null)
				textpart = textParts.FirstOrDefault(p => p.ContentType.Matches("text", "plain"));

			if (textpart == null)
				textpart = textParts.FirstOrDefault(p => p.ContentType.Matches("text", "*"));

			if (textpart == null)
				throw new Exception();

			labelContent.Text = String.Format("{0} ({1})",
				textpart.ContentType,
				textpart.ContentType.Charset);

			string html;

			if (textpart.ContentType.Matches("text", "html"))
				html = textpart.Text;
			else
				html = TextToHtmlHelper.TextToHtml(textpart.Text);

			this.HtmlContent = html;

			m_webView.LoadHtmlString(html, null);
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
			var msg = MK.MimeMessage.Load(m_msgFile);

			if (targetIdx == -1)
				throw new Exception();

			var part = msg.Attachments.Skip(targetIdx).First();

			string filename = part.FileName;
			if (filename == null)
				filename = "attachment-" + System.IO.Path.GetFileName(System.IO.Path.GetTempFileName());

			var fullPath = "/tmp/" + filename;

			using (var stream = File.OpenWrite(fullPath))
				part.ContentObject.DecodeTo(stream);

			CmdHelpers.LaunchDefaultApp(fullPath);
		}
	}
}

