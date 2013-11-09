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
		queryStore.AppendValues("to:linux-kernel@vger.kernel.org");
		queryStore.AppendValues("*");
		queryStore.AppendValues("");
	}

	void MyCellDataFunc(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		bool unread = (bool)model.GetValue(iter, MyTreeModel.COL_UNREAD);

		var c = (Gtk.CellRendererText)cell;
		
		if (unread)
			c.Weight = (int)Pango.Weight.Bold;
		else
			c.Weight = (int)Pango.Weight.Normal;

		//(cell as Gtk.CellRendererText).Text = song;
	}

	void SetupMailList()
	{
		treeviewList.FixedHeightMode = true;

		// colorize every other row
		//treeviewList.RulesHint = true;

		TreeViewColumn c;
		Gtk.CellRendererText re;

		re = new Gtk.CellRendererText();
		c = new TreeViewColumn("From", re, "text", MyTreeModel.COL_FROM);
		c.SetCellDataFunc(re, MyCellDataFunc);
		c.Expand = false;
		c.Sizing = TreeViewColumnSizing.Fixed;
		c.FixedWidth = 350;
		c.Resizable = true;
		c.Reorderable = true;
		treeviewList.AppendColumn(c);

		re = new Gtk.CellRendererText();
		c = new TreeViewColumn("Subject", re, "text", MyTreeModel.COL_SUBJECT);
		c.SetCellDataFunc(re, MyCellDataFunc);
		c.Expand = true;
		c.Sizing = TreeViewColumnSizing.Fixed;
		c.FixedWidth = 150;
		c.Resizable = true;
		c.Reorderable = true;
		treeviewList.AppendColumn(c);

		re = new Gtk.CellRendererText();
		c = new TreeViewColumn("Date", re, "text", MyTreeModel.COL_DATE);
		c.SetCellDataFunc(re, MyCellDataFunc);
		c.Expand = false;
		c.Sizing = TreeViewColumnSizing.Fixed;
		c.FixedWidth = 150;
		c.Resizable = true;
		c.Reorderable = true;
		treeviewList.AppendColumn(c);

		re = new Gtk.CellRendererText();
		c = new TreeViewColumn("Tags", re, "text", MyTreeModel.COL_TAGS);
		c.SetCellDataFunc(re, MyCellDataFunc);
		c.Expand = false;
		c.Sizing = TreeViewColumnSizing.Fixed;
		c.FixedWidth = 150;
		c.Resizable = true;
		c.Reorderable = true;
		treeviewList.AppendColumn(c);	
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}

	protected void OnTreeviewSearchCursorChanged(object sender, EventArgs e)
	{
		TreeSelection selection = (sender as TreeView).Selection;
		TreeModel model;
		TreeIter iter;

		string queryString = "";

		if (selection.GetSelected(out model, out iter))
			queryString = (string)model.GetValue(iter, 0);

		queryEntry.Text = queryString;
		queryEntry.Activate();
	}

	void ExecuteQuery(string queryString)
	{
		if (m_cts != null && m_cts.IsCancellationRequested)
			throw new Exception();

		if (m_queryTask != null)
		{
			if (m_queryTask.IsCompleted == false)
			{
				Console.WriteLine("cancelling");

				m_cts.Cancel();

				Console.WriteLine("cancelling done");
			}

			m_cts = null;
			m_queryTask = null;
		}

		m_cts = new CancellationTokenSource();
		m_queryTask = ProcessSearch(queryString, m_cts.Token);
	}

	async Task ProcessSearch(string queryString, CancellationToken ct)
	{
		if (string.IsNullOrWhiteSpace(queryString))
		{
			treeviewList.Model = new TreeModelAdapter(new MyTreeModel());
			return;
		}

		var sw = Stopwatch.StartNew();

		var query = NM.Query.Create(m_db, queryString);

		long t1 = sw.ElapsedMilliseconds;

		var msgs = query.SearchMessages();

		long t2 = sw.ElapsedMilliseconds;

		int count = 0;

		var model = new MyTreeModel(query);

		treeviewList.Model = new TreeModelAdapter(model);

		while (msgs.Valid)
		{
			if (ct.IsCancellationRequested)
			{
				Console.WriteLine("CANCEL");
				sw.Stop();
				return;
			}

			var msg = msgs.Current;

			model.Append(msg);

			if (count == 0)
				treeviewList.SetCursor(TreePath.NewFirst(), null, false);

			count++;

			if (count % 100 == 0)
			{
				label3.Text = String.Format("{0}/{1} msgs", count.ToString(), model.Count);

				//Console.WriteLine("yielding");
				//await Task.Delay(100);
				await Task.Yield();
			}

			msgs.Next();
		}

		label3.Text = String.Format("{0}/{1} msgs", count.ToString(), model.Count);

		long t3 = sw.ElapsedMilliseconds;

		sw.Stop();

		Console.WriteLine("Added {0} messages in {1}, {2}, {3} = {4} ms", count, t1, t2 - t1, t3 - t2, t3);
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

	NM.Message? GetCurrentMessage()
	{
		TreeSelection selection = treeviewList.Selection;
		TreeModel model;
		TreeIter iter;

		if (!selection.GetSelected(out model, out iter))
			return null;

		var adap = (TreeModelAdapter)model;
		var myModel = (MyTreeModel)adap.Implementor;

		var msgN = myModel.GetMessage(iter);

		if (msgN == null)
			throw new Exception();

		var msg = msgN.Value;

		return msg;
	}

	protected void OnGcActionActivated(object sender, EventArgs e)
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	protected void OnReplyAllActionActivated(object sender, EventArgs e)
	{
		Reply(true);
	}

	protected void OnReplyActionActivated(object sender, EventArgs e)
	{
		Reply(false);
	}

	static Task RunProcessAsync(Process process)
	{
		var tcs = new TaskCompletionSource<bool>();

		process.EnableRaisingEvents = true;

		process.Exited += (sender, args) =>
		{
			tcs.SetResult(true);
			process.Dispose();
		};

		process.Start();

		return tcs.Task;
	}

	async void Reply(bool replyAll)
	{
		var nmMsg = GetCurrentMessage();

		if (nmMsg.HasValue == false)
			return;

		var msgId = nmMsg.Value.Id;

		string replyText;

		using (var process = new Process())
		{
			var si = process.StartInfo;
			si.FileName = "notmuch";
			si.Arguments = String.Format("reply --reply-to={0} id:{1}", replyAll ? "all" : "sender", msgId);
			si.UseShellExecute = false;
			si.CreateNoWindow = true;
			si.RedirectStandardOutput = true;

			process.Start();

			var reader = process.StandardOutput;

			replyText = reader.ReadToEnd();

			process.WaitForExit();
		}

		var tmpFile = System.IO.Path.GetTempFileName();

		File.WriteAllText(tmpFile, replyText);

		const string editorCmd = "gvim";
		const string editorArgs = "-f \"+set filetype=mail\" +6 {0}";

		using (var process = new Process())
		{
			var si = process.StartInfo;
			si.FileName = editorCmd;
			si.Arguments = String.Format(editorArgs, tmpFile);
			si.UseShellExecute = false;
			si.CreateNoWindow = true;

			var task = RunProcessAsync(process);

			var dlg = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Cancel, "Waiting for editor");

			dlg.Response += (o, args) =>
			{
				Console.WriteLine("Killing editor process");
				process.Kill();
			};

			dlg.ShowNow();

			await task;

			dlg.Destroy();
		}

		File.Delete(tmpFile);
	}

	protected void OnQueryEntryChanged(object sender, EventArgs e)
	{
		Console.WriteLine("changes");

		var queryStr = queryEntry.Text;

		//ExecuteQuery(queryStr);
	}

	protected void OnQueryEntryActivated(object sender, EventArgs e)
	{
		Console.WriteLine("act");

		var queryStr = queryEntry.Text;

		ExecuteQuery(queryStr);
	}
}
