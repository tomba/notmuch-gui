using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using NotMuchGUI;
using NM = NotMuch;
using WebKit;

public partial class MainWindow: Gtk.Window
{
	NM.Database m_db;
	WebKit.WebView m_webView;
	CancellationTokenSource m_cts;
	Task m_queryTask;

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
		m_db = NM.Database.Open(path, NM.DatabaseMode.READ_ONLY);

		// select first items
		treeviewSearch.SetCursor(TreePath.NewFirst(), null, false);
	}

	protected override void OnDestroyed()
	{
		base.OnDestroyed();

		m_db.Dispose();
		m_db = null;
	}

	void SetupQueryList()
	{
		var queryColumn = new Gtk.TreeViewColumn();
		queryColumn.Title = "Query";

		var queryCell = new Gtk.CellRendererText();

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
		treeviewList.FixedHeightMode = true;

		TreeViewColumn c;

		c = new TreeViewColumn("From", new Gtk.CellRendererText(), "text", 0);
		//c.Expand = true;
		c.Sizing = TreeViewColumnSizing.Fixed;
		c.FixedWidth = 150;
		treeviewList.AppendColumn(c);

		c = new TreeViewColumn("Subject", new Gtk.CellRendererText(), "text", 1);
		//c.Expand = true;
		c.Sizing = TreeViewColumnSizing.Fixed;
		c.FixedWidth = 150;
		treeviewList.AppendColumn(c);

		c = new TreeViewColumn("Date", new Gtk.CellRendererText(), "text", 2);
		//c.Expand = false;
		c.Sizing = TreeViewColumnSizing.Fixed;
		c.FixedWidth = 150;
		treeviewList.AppendColumn(c);
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}

	protected async void OnTreeviewSearchCursorChanged(object sender, EventArgs e)
	{
		TreeSelection selection = (sender as TreeView).Selection;
		TreeModel model;
		TreeIter iter;

		if (m_cts != null && m_cts.IsCancellationRequested)
			return;

		if (m_queryTask != null)
		{
			Console.WriteLine("cancelling");

			m_cts.Cancel();
			await m_queryTask;

			m_cts = null;
			m_queryTask = null;

			Console.WriteLine("cancelling done");
		}

		treeviewList.Model = null;

		if (!selection.GetSelected(out model, out iter))
			return;

		var queryString = (string)model.GetValue(iter, 0);

		m_cts = new CancellationTokenSource();
		m_queryTask = ProcessSearch(queryString, m_cts.Token);
	}

	async Task ProcessSearch(string queryString, CancellationToken ct)
	{
		Console.WriteLine("Process in thread {0}", System.Threading.Thread.CurrentThread.ManagedThreadId);

		long t1, t2, t3;

		var sw = Stopwatch.StartNew();

		var q = NM.Query.Create(m_db, queryString);
		int totalCount = q.Count;

		var msgs = q.SearchMessages();

		t1 = sw.ElapsedMilliseconds;
		sw.Restart();

		const int max = 1000000;
		int count = 0;

		var model = new MyTreeModel(totalCount, q);

		treeviewList.Model = new TreeModelAdapter(model);

		while (msgs.Valid)
		{
			if (ct.IsCancellationRequested)
			{
				Console.WriteLine("CANCEL");
				break;
			}

			var msg = msgs.Current;

			model.Append(msg);

			if (count == 0)
				treeviewList.SetCursor(TreePath.NewFirst(), null, false);

			count++;

			if (count % 100 == 0)
			{
				label3.Text = String.Format("{0}/{1} msgs", count.ToString(), totalCount);

				//Console.WriteLine("yielding");
				//await Task.Delay(100);
				await Task.Yield();
			}

			if (count >= max)
			{
				Console.WriteLine("aborting search, max count reached");
				break;
			}

			msgs.Next();
		}

		label3.Text = String.Format("{0}/{1} msgs", count.ToString(), totalCount);

		t2 = sw.ElapsedMilliseconds;
		sw.Restart();

		t3 = sw.ElapsedMilliseconds;
		sw.Stop();

		Console.WriteLine("Added {0} messages in {1},{2},{3} ms", count, t1, t2, t3);
	}

	protected void OnTreeviewListCursorChanged(object sender, EventArgs e)
	{
		TreeSelection selection = (sender as TreeView).Selection;
		TreeModel model;
		TreeIter iter;

		if (!selection.GetSelected(out model, out iter))
			return;

		var adap = (TreeModelAdapter)model;
		var myModel = (MyTreeModel)adap.Implementor;

		var msgN = myModel.GetMessage(iter);

		if (msgN == null)
			throw new Exception();

		var msg = msgN.Value;

		var filename = msg.FileName;

		ShowEmail(filename);
	}

	void ShowEmail(string filename)
	{
		int fd = Mono.Unix.Native.Syscall.open(filename, Mono.Unix.Native.OpenFlags.O_RDONLY);

		var readStream = new GMime.StreamFs(fd);

		var p = new GMime.Parser(readStream);
		var msg = p.ConstructMessage();

		var sw = new StringWriter();
		GMimeHelpers.DumpStructure(msg, sw, 0);
		var dump = sw.ToString();
		textviewDump.Buffer.Text = dump;

		GMime.Part textpart = null;

		if (textpart == null)
			textpart = GMimeHelpers.FindFirstContent(msg, new GMime.ContentType("text", "html"));

		if (textpart == null)
			textpart = GMimeHelpers.FindFirstContent(msg, new GMime.ContentType("text", "plain"));

		if (textpart == null)
			textpart = GMimeHelpers.FindFirstContent(msg, new GMime.ContentType("text", "*"));
			
		if (textpart == null)
			throw new Exception();

		AddText(textpart);

		label1.Text = textpart.ContentType.ToString();
		label2.Text = textpart.ContentType.GetParameter("charset");

		Mono.Unix.Native.Syscall.close(fd);
	}

	void AddText(GMime.Part part)
	{
		var html = PartToHtml(part);

		textviewSrc.Buffer.Text = html;
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
