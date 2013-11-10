using System;
using System.IO;
using NM = NotMuch;

namespace NotMuchGUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MessageWidget : Gtk.Bin
	{
		WebKit.WebView m_webView;

		public MessageWidget()
		{
			this.Build();

			m_webView = new WebKit.WebView();
			m_webView.Editable = false;

			scrolledwindowWeb.Add(m_webView);
			scrolledwindowWeb.ShowAll();
		}

		public void ShowEmail(NM.Message msg, GMime.Message gmsg)
		{
			labelFrom.Text = msg.GetHeader("From");
			labelTo.Text = msg.GetHeader("To");
			labelCc.Text = msg.GetHeader("Cc");
			labelSubject.Text = msg.GetHeader("Subject");
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
			var memstream = new GMime.StreamMem();

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
			}

			memstream.Seek(0);

			// XXX StreamWrapper's Dispose is broken
			var sw = new GMime.StreamWrapper(memstream);

			using (var reader = new StreamReader(sw, System.Text.UTF8Encoding.UTF8, false, 128, true))
			{
				var str = reader.ReadToEnd();
				return str;
			}
		}

		protected void OnTogglebutton1Toggled(object sender, EventArgs e)
		{
			m_webView.ViewSourceMode = togglebutton1.Active;
			m_webView.LoadHtmlString(this.HtmlContent, null);
		}
	}
}

