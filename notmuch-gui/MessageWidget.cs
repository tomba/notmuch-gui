using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NM = NotMuch;
using MK = MimeKit;
using UI = Gtk.Builder.ObjectAttribute;
using Gtk;
using MimeKit.Cryptography;

namespace NotMuchGUI
{
	// SetProperty is protected, so we need a custom class
	class MyWebSettings : WebKit.WebSettings
	{
		public void Set(string name, bool val)
		{
			SetProperty(name, new GLib.Value(val));
		}
	}

	public class MessageWidget : Bin
	{
		WebKit.WebView m_webView;
		ListStore m_attachmentStore;
		string m_msgFile;

		public string HtmlContent { get; private set; }

		[UI] Label labelFrom;
		[UI] Label labelTo;
		[UI] Label labelCc;
		[UI] Label labelSubject;
		[UI] Label labelDate;
		[UI] Label labelMsgID;
		[UI] Label labelContent;
		[UI] Label labelThreadID;

		[UI] ScrolledWindow scrolledwindowWeb;

		[UI] Expander attachmentExpander;
		[UI] Label attachmentLabel;
		[UI] IconView attachmentIconview;

		public MessageWidget()
		{
			Builder builder = new Builder(null, "NotMuchGUI.UI.MessageWidget.ui", null);
			builder.Autoconnect(this);
			Add((Box)builder.GetObject("MessageWidget"));

			m_webView = new WebKit.WebView();
			m_webView.Editable = false;
			m_webView.Expand = true;

			var settings = new MyWebSettings();
			settings.Set("auto-load-images", true);
			m_webView.Settings = settings;

			scrolledwindowWeb.Add(m_webView);

			// filename, index
			m_attachmentStore = new ListStore(typeof(string), typeof(int), typeof(Gdk.Pixbuf));

			attachmentIconview.Model = m_attachmentStore;
			attachmentIconview.PixbufColumn = 2;
			attachmentIconview.TextColumn = 0;
			attachmentIconview.ItemActivated += OnAttachmentItemActivated;
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

		public void ShowError(string text)
		{
			Clear();
			m_webView.LoadString(text, null, null, null);
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

				const int iconSize = 24;

				Gdk.Pixbuf pix = null;

				var icon = GLib.ContentType.GetIcon(part.ContentType.MimeType.ToLower());
				if (icon != null)
				{
					var iconInfo = IconTheme.Default.LookupIcon(icon, iconSize, 0);
					if (iconInfo != null)
						pix = iconInfo.LoadIcon();
				}

				m_attachmentStore.AppendValues(filename, idx, pix);

				idx++;
			}

			if (idx == 0)
			{
				attachmentExpander.Hide();
			}
			else
			{
				attachmentExpander.Expanded = false;
				attachmentLabel.Text = String.Format("{0} attachments", idx);
				attachmentExpander.Show();
			}
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

			MK.TextPart textpart = null;

			if (msg.Body is MultipartEncrypted)
			{
				var encryptedPart = (MultipartEncrypted)msg.Body;

				var ctx = new MyGPGContext();

				try
				{
					var decrypted = encryptedPart.Decrypt(ctx);

					textpart = (MK.TextPart)decrypted;
				}
				catch (OperationCanceledException e)
				{
					m_webView.LoadString(string.Format("Unauthorized<p>{0}", e.Message), null, null, null);
					return;
				}
				catch (UnauthorizedAccessException e)
				{
					m_webView.LoadString(string.Format("Unauthorized<p>{0}", e.Message), null, null, null);
					return;
				}
				catch (Exception e)
				{
					m_webView.LoadString(string.Format("Error<p>{0}", e.Message), null, null, null);
					return;
				}

			}
			else
			{
				var textParts = msg.BodyParts.OfType<MK.TextPart>();

				if (textpart == null)
					textpart = textParts.FirstOrDefault(p => p.ContentType.Matches("text", "html"));

				if (textpart == null)
					textpart = textParts.FirstOrDefault(p => p.ContentType.Matches("text", "plain"));

				if (textpart == null)
					textpart = textParts.FirstOrDefault(p => p.ContentType.Matches("text", "*"));

				if (textpart == null)
					textpart = textParts.FirstOrDefault();
			}

			if (textpart == null)
			{
				m_webView.LoadString("Error: no text parts", null, null, null);
				return;
			}

			labelContent.Text = String.Format("{0} ({1})",
				textpart.ContentType,
				textpart.ContentType.Charset);

			string html;

			if (textpart.ContentType.Matches("text", "html"))
				html = textpart.Text;
			else
				html = TextToHtmlHelper.TextToHtml(textpart.Text);

			this.HtmlContent = html;

			m_webView.LoadString(html, null, null, null);
		}

		public bool ShowHtmlSource
		{
			get { return m_webView.ViewSourceMode; }
			set
			{
				m_webView.ViewSourceMode = value;
				if (this.HtmlContent != null)
					m_webView.LoadString(this.HtmlContent, null, null, null);
			}
		}

		void OnAttachmentItemActivated(object o, ItemActivatedArgs args)
		{
			TreeIter iter;
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

