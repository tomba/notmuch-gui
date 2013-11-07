using System;
using Gtk;
using NotMuch;
using System.IO;
using WebKit;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

public partial class MainWindow: Gtk.Window
{
	Database m_db;
	WebKit.WebView m_webView;

	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();

		SetupQueryList();
		SetupMailList();

		m_webView = new WebKit.WebView();
		m_webView.Editable = false;

		scrolledwindowWeb.Add(m_webView);
		scrolledwindowWeb.ShowAll();

		var path = "/home/tomba/Maildir";
		m_db = Database.Open(path, DatabaseMode.READ_ONLY);

		// select first items
		treeviewSearch.SetCursor(TreePath.NewFirst(), null, false);
		treeviewList.SetCursor(TreePath.NewFirst(), null, false);
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

		queryStore.AppendValues("ttlampopumppuhuolto");
		queryStore.AppendValues("Tomi");
		queryStore.AppendValues("from:ti.com");
		queryStore.AppendValues("from:linkedin");
		queryStore.AppendValues("");
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

	void ProcessSearch(object data)
	{
		Console.WriteLine("Process in thread {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);

		var queryString = (string)data;

		long t1, t2, t3, t4;

		var sw = Stopwatch.StartNew();

		var q = Query.Create(m_db, queryString);

		var msgs = q.SearchMessages();

		t1 = sw.ElapsedMilliseconds;
		sw.Restart();

		const int max = 5000;
		int count = 0;

		var mailStore = new Gtk.ListStore(typeof(string), typeof(string), typeof(string));

		while (msgs.Valid)
		{
			if (m_queryTaskCTS.IsCancellationRequested)
			{
				Console.WriteLine("CANCEL");
				break;
			}

			var msg = msgs.Current;

			var id = msg.Id;
			var from = msg.GetHeader("From");
			var subject = msg.GetHeader("Subject");

			mailStore.AppendValues(from, subject, id);

			count++;

			if (count >= max)
			{
				Console.WriteLine("aborting search, max count reached");
				break;
			}

			msgs.Next();
		}

		t2 = sw.ElapsedMilliseconds;
		sw.Restart();

		label3.Text = String.Format("{0}/{1} msgs", count.ToString(), q.Count);

		t3 = sw.ElapsedMilliseconds;
		sw.Restart();

		t4 = sw.ElapsedMilliseconds;
		sw.Stop();

		Console.WriteLine("Added {0} messages in {1},{2},{3},{4} ms", count, t1, t2, t3, t4);

		Gtk.Application.Invoke(delegate
		{
			m_mailStore = mailStore;
			treeviewList.Model = m_mailStore;
		});
	}

	Task m_queryTask;
	CancellationTokenSource m_queryTaskCTS;

	protected void OnTreeviewSearchCursorChanged(object sender, EventArgs e)
	{
		if (m_queryTask != null)
		{
			Console.WriteLine("cancelling");
			m_queryTaskCTS.Cancel();
			m_queryTask.Wait();
			Console.WriteLine("cancel done");
		
			m_queryTask = null;
			m_queryTaskCTS = null;
		}

		TreeSelection selection = (sender as TreeView).Selection;
		TreeModel model;
		TreeIter iter;

		if (selection.GetSelected(out model, out iter))
		{
			var queryString = (string)model.GetValue(iter, 0);

			var sc = System.Threading.SynchronizationContext.Current;

			Console.WriteLine("Main in thread {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);

			m_queryTaskCTS = new CancellationTokenSource();

			m_queryTask = Task.Factory.StartNew(ProcessSearch, queryString, m_queryTaskCTS.Token);
		}
		else
		{
			m_mailStore.Clear();
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
			var id = (string)model.GetValue(iter, 2);

			var msgN = m_db.FindMessage(id);

			if (msgN == null)
				throw new Exception();

			var msg = msgN.Value;

			var filename = msg.FileName;

			ShowEmail(filename);

			msg.DestroyHandle();
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
		int fd = Mono.Unix.Native.Syscall.open(filename, Mono.Unix.Native.OpenFlags.O_RDONLY);

		var readStream = new GMime.StreamFs(fd);

		var p = new GMime.Parser(readStream);
		var msg = p.ConstructMessage();

		DumpStructure(msg);

		GMime.Part textpart = null;

		if (textpart == null)
			textpart = FindFirstContent(msg, new GMime.ContentType("text", "html"));

		if (textpart == null)
			FindFirstContent(msg, new GMime.ContentType("text", "plain"));

		if (textpart == null)
			textpart = FindFirstContent(msg, new GMime.ContentType("text", "*"));
			
		if (textpart == null)
			throw new Exception();

		AddText(textpart);

		label1.Text = textpart.ContentType.ToString();
		label2.Text = textpart.ContentType.GetParameter("charset");

		Mono.Unix.Native.Syscall.close(fd);
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

		filterstream.Add(new GMime.FilterCRLF(false, false));

		var charset = part.ContentType.GetParameter("charset");
		if (charset != null)
			filterstream.Add(new GMime.FilterCharset(charset, "utf-8"));

		if (!part.ContentType.IsType("text", "html"))
		{
			var flags = 0
			            //| HtmlFilterFlags.PRE
			            | HtmlFilterFlags.CONVERT_NL
			            | HtmlFilterFlags.MARK_CITATION;
			uint quoteColor = 0x888888;
			filterstream.Add(new GMime.FilterHTML((uint)flags, quoteColor));
		}

		part.ContentObject.WriteToStream(filterstream);

		filterstream.Flush();

		var encoding = System.Text.UTF8Encoding.UTF8;

		var sw = new GMime.StreamWrapper(memstream);
		sw.Position = 0;
		var texti = new StreamReader(sw, encoding).ReadToEnd();

		textview1.Buffer.Text = texti;
		m_webView.LoadHtmlString(texti, null);
	}
}
