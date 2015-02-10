using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gtk;
using MimeKit;
using MimeKit.Cryptography;
using MK = MimeKit;
using NM = NotMuch;
using UI = Gtk.Builder.ObjectAttribute;

namespace NotMuchGUI
{
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

			m_webView = new WebKit.WebView()
			{
				Editable = false,
				Expand = true,
			};

			m_webView.PrintRequested += (o, args) => args.RetVal = true;
			m_webView.NewWindowPolicyDecisionRequested += HandleNewWindowPolicyDecisionRequested;
			m_webView.NavigationPolicyDecisionRequested += HandleNavigationPolicyDecisionRequested;

			var settings = new WebKit.WebSettings()
			{
				AutoLoadImages = true,
				EnableDeveloperExtras = true,
				EnableScripts = false,
			};
			m_webView.Settings = settings;

			scrolledwindowWeb.Add(m_webView);

			// filename, index
			m_attachmentStore = new ListStore(typeof(string), typeof(int), typeof(Gdk.Pixbuf));

			attachmentIconview.Model = m_attachmentStore;
			attachmentIconview.PixbufColumn = 2;
			attachmentIconview.TextColumn = 0;
			attachmentIconview.ItemActivated += OnAttachmentItemActivated;

			var inspector = m_webView.Inspector;
			inspector.InspectWebView += (o, args) =>
			{
				var wnd = new Window("Inspector");
				var sv = new ScrolledWindow();
				var wv = new WebKit.WebView();

				wnd.SetDefaultSize(800, 600);

				sv.Add(wv);
				wnd.Add(sv);

				args.RetVal = wv;

				wnd.ShowAll();
				wnd.Maximize();
			};

			m_webView.LoadFinished += (o, args) =>
			{
				if (m_webView.ViewSourceMode)
					return;

				var doc = m_webView.DomDocument;
				if (doc == null || doc.Head == null)
					return;

				var link = (WebKit.DOMHTMLLinkElement)doc.CreateElement("link");
				link.SetAttribute("rel", "stylesheet");
				link.Type = "text/css";
				link.Href = "message.css";
				doc.Head.AppendChild(link);
			};
		}

		void HandleNavigationPolicyDecisionRequested(object o, WebKit.NavigationPolicyDecisionRequestedArgs args)
		{
			var reason = args.NavigationAction.Reason;

			if (reason == WebKit.WebNavigationReason.Other)
				return;

			args.PolicyDecision.Ignore();
			args.RetVal = true;
		}

		void HandleNewWindowPolicyDecisionRequested(object o, WebKit.NewWindowPolicyDecisionRequestedArgs args)
		{
			var reason = args.NavigationAction.Reason;

			if (reason == WebKit.WebNavigationReason.Other)
				return;

			args.PolicyDecision.Ignore();
			args.RetVal = true;
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

			SetMessageLabels(msg, threadID);

			var ctx = MessageParser.ParseMessage(msg);

			ShowBody(ctx);
			ShowAttachments(ctx);
		}

		void SetMessageLabels(MK.MimeMessage msg, string threadID)
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
		}

		void ShowBody(MessageParser.ParseContext ctx)
		{
			var content = ctx.Builder.ToString();

			this.HtmlContent = content;

			var baseUri = "file://" + AppDomain.CurrentDomain.BaseDirectory;

			m_webView.LoadString(content, "text/html", "UTF-8", baseUri);
		}

		void ShowAttachments(MessageParser.ParseContext ctx)
		{
			m_attachmentStore.Clear();

			int idx = 0;

			foreach (var attachment in ctx.Attachments)
			{
				string filename = attachment.FileName;
				if (filename == null)
					filename = "attachment-" + System.IO.Path.GetFileName(System.IO.Path.GetTempFileName());

				const int iconSize = 24;

				Gdk.Pixbuf pix = null;

				var icon = GLib.ContentType.GetIcon(attachment.ContentType.MimeType.ToLower());
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

