using System;
using Gtk;
using NotMuch;
using System.IO;
using WebKit;

public partial class MainWindow: Gtk.Window
{
	Database m_db;
	WebKit.WebView m_view;

	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();

		SetupQueryList();
		SetupMailList();

		var path = "/home/tomba/Maildir";
		m_db = Database.Open(path, DatabaseMode.READ_ONLY);

		/*
		var webView = new WebKit.WebView();
		webView.Open("http://mono-project.com");
		//webView.LoadString();
		webView.Editable = false;
		vpaned1.Add(webView);
*/
		ScrolledWindow scrollWindow = new ScrolledWindow();
		scrollWindow.HeightRequest = 200;
		var webView = new WebKit.WebView();
		//webView.Open("http://mono-project.com");
		webView.Editable = false;
		scrollWindow.Add(webView);
		vpaned1.Add(scrollWindow);
		scrollWindow.ShowAll();

		m_view = webView;
	}

	protected override void OnDestroyed()
	{
		base.OnDestroyed();

		m_db.Dispose();
		m_db = null;
	}

	void SetupQueryList()
	{
		// Create a column for the artist name
		var queryColumn = new Gtk.TreeViewColumn();
		queryColumn.Title = "Query";

		// Create the text cell that will display the artist name
		var queryCell = new Gtk.CellRendererText();

		// Add the cell to the column
		queryColumn.PackStart(queryCell, true);

		treeviewSearch.AppendColumn(queryColumn);

		queryColumn.AddAttribute(queryCell, "text", 0);

		var queryStore = new Gtk.ListStore(typeof(string));

		treeviewSearch.Model = queryStore;

		queryStore.AppendValues("Tomi");
		queryStore.AppendValues("from:ti.com");
		queryStore.AppendValues("Test3");
	}

	void SetupMailList()
	{
		treeviewList.AppendColumn("From", new Gtk.CellRendererText(), "text", 0);
		treeviewList.AppendColumn("Subject", new Gtk.CellRendererText(), "text", 1);

		m_mailStore = new Gtk.ListStore(typeof(string), typeof(string), typeof(string));
		treeviewList.Model = m_mailStore;
	}

	Gtk.ListStore m_mailStore;

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}

	protected void OnTreeviewSearchRowActivated(object o, RowActivatedArgs args)
	{
	}

	protected void OnTreeviewSearchCursorChanged(object sender, EventArgs e)
	{
		m_mailStore.Clear();

		TreeSelection selection = (sender as TreeView).Selection;
		TreeModel model;
		TreeIter iter;

		// THE ITER WILL POINT TO THE SELECTED ROW
		if (selection.GetSelected(out model, out iter))
		{
			//Console.WriteLine("Selected Value:" + model.GetValue(iter, 0).ToString() + model.GetValue(iter, 1).ToString());

			var queryString = model.GetValue(iter, 0).ToString();

			var q = Query.Create(m_db, queryString);

			var msgs = q.Search();

			foreach (var msg in msgs)
			{
				var filename = msg.FileName;
				var from = msg.GetHeader("From");
				var subject = msg.GetHeader("Subject");

				m_mailStore.AppendValues(from, subject, filename);
			}

			q.Dispose();
		}
	}

	protected void OnTreeviewListCursorChanged(object sender, EventArgs e)
	{
		TreeSelection selection = (sender as TreeView).Selection;
		TreeModel model;
		TreeIter iter;

		// THE ITER WILL POINT TO THE SELECTED ROW
		if (selection.GetSelected(out model, out iter))
		{
			//Console.WriteLine("Selected Value:" + model.GetValue(iter, 0).ToString() + model.GetValue(iter, 1).ToString());

			var filename = model.GetValue(iter, 2).ToString();

			ShowEmail(filename);
		}
		else
		{
			//textview1.Buffer.Clear();
		}
	}

	void DumpStructure(GMime.Entity ent)
	{
		if (ent is GMime.Message)
		{
			var msg = (GMime.Message)ent;

			Console.WriteLine("{0}", ent.GetType());

			DumpStructure(msg.MimePart);
		}
		else if (ent is GMime.Multipart)
		{
			var mp = (GMime.Multipart)ent;

			Console.WriteLine("{0}", ent.GetType());

			foreach (GMime.Entity part in mp)
				DumpStructure(part);
		}
		else if (ent is GMime.Part)
		{
			var part = (GMime.Part)ent;
			Console.WriteLine("{0}: {1}, {2}, {3}",
				part.GetType(), part.ContentType.ToString(), part.ContentType.GetParameter("charset"),
				part.ContentEncoding.ToString());
		}
		else
		{
			throw new Exception();
		}
	}

	GMime.Part FindFirstContent(GMime.Entity ent, GMime.ContentType ct)
	{
		if (ent is GMime.Message)
		{
			var msg = (GMime.Message)ent;

			return FindFirstContent(msg.MimePart, ct);
		}
		else if (ent is GMime.Multipart)
		{
			var mp = (GMime.Multipart)ent;

			foreach (GMime.Entity part in mp)
			{
				var p = FindFirstContent(part, ct);
				if (p != null)
					return p;
			}

			return null;
		}
		else if (ent is GMime.Part)
		{
			var part = (GMime.Part)ent;

			if (part.ContentType.IsType(ct.MediaType, ct.MediaSubtype))
				return part;
			else
				return null;
		}
		else
		{
			throw new Exception();
		}
	}

	void ShowEmail(string filename)
	{
		var text = File.ReadAllText(filename);

		var stream = new GMime.StreamMem(text);

		var p = new GMime.Parser(stream);
		var msg = p.ConstructMessage();

		DumpStructure(msg);

		var textpart = FindFirstContent(msg, new GMime.ContentType("text", "plain"));

		if (textpart == null)
		{
			textpart = FindFirstContent(msg, new GMime.ContentType("text", "html"));
			if (textpart == null)
				throw new Exception();
		}

		//var textpart = msg.Body;

		AddText(textpart);

		//var wqeqw = stream2.();

		//m_view.LoadString(wqeqw, "text/plain", null, null);
		//m_view.LoadHtmlString(text, null);
		//textview1.Buffer.Text = text;
	}

	enum HtmlFilterFlags
	{
		PRE = 1 << 0,
		CONVERT_NL = 1 << 1,
		CONVERT_SPACES = 1 << 2,
		CONVERT_URLS = 1 << 3,
		MARK_CITATION = 1 << 4,
		CONVERT_ADDRESSES = 1 << 5,
		ESCAPE_8BIT = 1 << 6,
		CITE = 1 << 7,
	}

	void AddText(GMime.Part part)
	{
		var memstream = new GMime.StreamMem();

		var filterstream = new GMime.StreamFilter(memstream);

		bool filt = false;

		var charset = part.ContentType.GetParameter("charset");

		if (filt)
		{
			var flags = 0
		            //| HtmlFilterFlags.PRE
			            | HtmlFilterFlags.CONVERT_NL
			            | HtmlFilterFlags.MARK_CITATION;
			filterstream.Add(new GMime.FilterHTML((uint)flags, 0xff0000));
		}

		//filterstream.Add(new GMime.FilterBasic(part.ContentEncoding, true));
		//filterstream.Add(new GMime.FilterCharset(charset, "utf-8"));
		part.ContentObject.WriteToStream(filterstream);

		//msg.ContentType.IsType("text", "*") && !msg.ContentType.IsType("text", "html")
		/*
		var sw = new GMime.StreamWrapper(stream2);
		sw.Position = 0;
		var texti = new StreamReader(sw).ReadToEnd();
*/

		byte[] arr = new byte[(int)memstream.Length];
		memstream.Seek(0);
		memstream.Read(arr, (uint)memstream.Length);


		var ct = part.ContentType.ToString();

		label1.Text = ct;
		label2.Text = charset;

		for (int i = 0; i < arr.Length && i < 100; ++i)
			Console.Write("{0}/{1:x} ", (char)arr[i], arr[i]);

		var encoding = System.Text.Encoding.GetEncoding(charset);
		encoding = System.Text.ASCIIEncoding.UTF8;
		var texti = encoding.GetString(arr);

		//var texti = System.Text.ASCIIEncoding.ASCII.GetString(arr);

		//var ce = part.ContentEncoding.ToString();

		textview1.Buffer.Text = texti;

		m_view.LoadString(texti, ct, null, null);
	}
}
