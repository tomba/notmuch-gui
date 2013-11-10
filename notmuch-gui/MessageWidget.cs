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

		public void ShowEmail(NM.Message msg)
		{
			labelFrom.Text = msg.GetHeader("From");
			labelTo.Text = msg.GetHeader("To");
			labelCc.Text = msg.GetHeader("Cc");
			labelSubject.Text = msg.GetHeader("Subject");
			labelDate.Text = msg.Date.ToLocalTime().ToString("g");
			//labelTags.Text = msg.ta

			var filename = msg.FileName;

			int fd = Mono.Unix.Native.Syscall.open(filename, Mono.Unix.Native.OpenFlags.O_RDONLY);

			var readStream = new GMime.StreamFs(fd);

			var p = new GMime.Parser(readStream);
			var gmsg = p.ConstructMessage();
			/*
			if (m_dbgWnd != null)
			{
				var sw = new StringWriter();
				GMimeHelpers.DumpStructure(gmsg, sw, 0);
				var dump = sw.ToString();
				m_dbgWnd.SetDump(dump);
			}*/

			GMime.Part textpart = null;

			if (textpart == null)
				textpart = GMimeHelpers.FindFirstContent(gmsg, new GMime.ContentType("text", "html"));

			if (textpart == null)
				textpart = GMimeHelpers.FindFirstContent(gmsg, new GMime.ContentType("text", "plain"));

			if (textpart == null)
				textpart = GMimeHelpers.FindFirstContent(gmsg, new GMime.ContentType("text", "*"));

			if (textpart == null)
				throw new Exception();

			AddText(textpart);

			//label1.Text = textpart.ContentType.ToString();
			//label2.Text = textpart.ContentType.GetParameter("charset");

			Mono.Unix.Native.Syscall.close(fd);
		}

		void AddText(GMime.Part part)
		{
			var html = PartToHtml(part);
			/*
			if (m_dbgWnd != null)
				m_dbgWnd.SetSrc(html);
*/
			m_webView.LoadHtmlString(html, null);
		}

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
					        //| HtmlFilterFlags.PRE
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
	}
}

