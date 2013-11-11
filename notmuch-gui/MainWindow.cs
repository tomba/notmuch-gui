using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Gtk;
using NotMuchGUI;
using NM = NotMuch;
using WebKit;
using System.Collections.Generic;
using System.Linq;

public partial class MainWindow: Gtk.Window
{
	NM.Database m_db;
	CancellationTokenSource m_cts;
	Task m_queryTask;
	ListStore m_queryStore;
	DebugWindow m_dbgWnd;
	List<string> m_allTags = new List<string>();
	bool m_threadedView = false;

	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();

		threadedAction.Active = m_threadedView;

		SetupQueryList();
		SetupMailList();

		var path = "/home/tomba/Maildir";
		m_db = NM.Database.Open(path, NM.DatabaseMode.READ_ONLY);

		var tags = m_db.AllTags;

		while (tags.Valid)
		{
			m_allTags.Add(tags.Current);
			m_queryStore.AppendValues(String.Format("tag:{0}", tags.Current));
			tags.Next();
		}

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

		m_queryStore = queryStore;
	}

	void MyCellDataFunc(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		bool unread = (bool)model.GetValue(iter, MyTreeModel.COL_UNREAD);

		var c = (Gtk.CellRendererText)cell;

		if (unread)
			c.Weight = (int)Pango.Weight.Bold;
		else
			c.Weight = (int)Pango.Weight.Normal;
	}

	void SetupMailList()
	{
		treeviewList.FixedHeightMode = true;

		// colorize every other row
		//treeviewList.RulesHint = true;

		TreeViewColumn c;

		c = CreateTreeViewColumn("From", MyTreeModel.COL_FROM);
		c.FixedWidth = 350;
		treeviewList.AppendColumn(c);

		c = CreateTreeViewColumn("Subject", MyTreeModel.COL_SUBJECT);
		c.Expand = true;
		c.FixedWidth = 150;
		treeviewList.AppendColumn(c);

		c = CreateTreeViewColumn("Date", MyTreeModel.COL_DATE);
		c.FixedWidth = 150;
		treeviewList.AppendColumn(c);

		c = CreateTreeViewColumn("Tags", MyTreeModel.COL_TAGS);
		c.FixedWidth = 150;
		treeviewList.AppendColumn(c);

		c = CreateTreeViewColumn("D", MyTreeModel.COL_DEPTH);
		c.FixedWidth = 50;
		treeviewList.AppendColumn(c);

		c = CreateTreeViewColumn("N", MyTreeModel.COL_MSG_NUM);
		c.FixedWidth = 50;
		treeviewList.AppendColumn(c);
	}

	TreeViewColumn CreateTreeViewColumn(string name, int col)
	{
		var re = new Gtk.CellRendererText();
		var c = new TreeViewColumn(name, re, "text", col);
		c.SetCellDataFunc(re, MyCellDataFunc);
		c.Expand = false;
		c.Sizing = TreeViewColumnSizing.Fixed;
		c.FixedWidth = 50;
		c.Resizable = true;
		c.Reorderable = true;

		return c;
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

	void ExecuteQuery()
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

		var queryString = queryEntry.Text;

		if (string.IsNullOrWhiteSpace(dateSearchEntry.Text) == false)
			queryString = queryString + String.Format(" date:{0}", dateSearchEntry.Text);

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

		Console.WriteLine("Query({0})", queryString);

		var query = NM.Query.Create(m_db, queryString);

		long t1 = sw.ElapsedMilliseconds;

		int count = 0;

		var model = new MyTreeModel(query);

		treeviewList.Model = new TreeModelAdapter(model);

		long t2 = sw.ElapsedMilliseconds;

		if (!m_threadedView)
		{
			var msgs = query.SearchMessages();

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

				model.Append(msg, 0);

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
		}
		else
		{
			var threads = query.SearchThreads();
			int lastYield = 0;

			while (threads.Valid)
			{
				if (ct.IsCancellationRequested)
				{
					Console.WriteLine("CANCEL");
					sw.Stop();
					return;
				}

				var thread = threads.Current;

				//Console.WriteLine("thread {0}: {1}", thread.Id, thread.TotalMessages);

				var msgs = thread.GetToplevelMessages();

				bool firstLoop = count == 0;

				while (msgs.Valid)
				{
					var msg = msgs.Current;

					AddMsgsRecursive(model, msg, 0, ref count);

					msgs.Next();
				}

				if (firstLoop)
					treeviewList.SetCursor(TreePath.NewFirst(), null, false);

				if (count - lastYield > 500)
				{
					label3.Text = String.Format("{0}/{1} msgs", count.ToString(), model.Count);

					//Console.WriteLine("yielding");
					//await Task.Delay(1000);
					await Task.Yield();

					lastYield = count;
				}

				threads.Next();
			}
		}

		long t3 = sw.ElapsedMilliseconds;

		model.FinishAdding();

		label3.Text = String.Format("{0}/{1} msgs", count, model.Count);

		long t4 = sw.ElapsedMilliseconds;

		sw.Stop();

		Console.WriteLine("Added {0} messages in {1}, {2}, {3}, {4} = {5} ms", count, t1, t2 - t1, t3 - t2, t4 - t3, t4);
	}

	void AddMsgsRecursive(MyTreeModel model, NM.Message msg, int depth, ref int count)
	{
		//Console.WriteLine("append {0}", msg.Id);

		model.Append(msg, depth);

		count++;

		var replies = msg.GetReplies();

		while (replies.Valid)
		{
			var reply = replies.Current;

			AddMsgsRecursive(model, reply, depth + 1, ref count);

			replies.Next();
		}
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

		var msg = myModel.GetMessage(iter);

		if (msg.IsNull)
			return;

		tagsWidget.UpdateTagsView(msg, m_allTags);


		var filename = msg.FileName;

		int fd = Mono.Unix.Native.Syscall.open(filename, Mono.Unix.Native.OpenFlags.O_RDONLY);

		using (var readStream = new GMime.StreamFs(fd))
		{
			readStream.Owner = true;

			var p = new GMime.Parser(readStream);
			var gmsg = p.ConstructMessage();

			if (m_dbgWnd != null)
			{
				var sw = new StringWriter();
				GMimeHelpers.DumpStructure(gmsg, sw, 0);
				var dump = sw.ToString();
				m_dbgWnd.SetDump(dump);
			}

			messagewidget1.ShowEmail(msg, gmsg);

			if (m_dbgWnd != null)
			{
				m_dbgWnd.SetSrc(messagewidget1.HtmlContent);
			}

			// GMime.StreamFs is buggy. Dispose doesn't close the fd.
			readStream.Close();
		}
	}

	NM.Message GetCurrentMessage()
	{
		TreeSelection selection = treeviewList.Selection;
		TreeModel model;
		TreeIter iter;

		if (!selection.GetSelected(out model, out iter))
			return NM.Message.NullMessage;

		var adap = (TreeModelAdapter)model;
		var myModel = (MyTreeModel)adap.Implementor;

		var msg = myModel.GetMessage(iter);

		if (msg.IsNull)
			throw new Exception();

		return msg;
	}

	protected void OnGcActionActivated(object sender, EventArgs e)
	{
		var sw = Stopwatch.StartNew();

		GC.Collect();
		GC.WaitForPendingFinalizers();

		sw.Stop();

		Console.WriteLine("GC in {0} ms", sw.ElapsedMilliseconds);
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

		if (nmMsg.IsNull)
			return;

		var msgId = nmMsg.Id;

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
		//var queryStr = queryEntry.Text;

		//ExecuteQuery(queryStr);
	}

	protected void OnQueryEntryActivated(object sender, EventArgs e)
	{
		ExecuteQuery();
	}

	protected void OnDateSearchEntryActivated(object sender, EventArgs e)
	{
		ExecuteQuery();
	}

	protected void OnDbgActionActivated(object sender, EventArgs e)
	{
		if (m_dbgWnd != null)
		{
			m_dbgWnd.Destroy();
			m_dbgWnd = null;
		}

		if (dbgAction.Active)
		{
			m_dbgWnd = new DebugWindow();
			m_dbgWnd.ShowAll();
		}
	}

	protected void OnThreadedActionActivated(object sender, EventArgs e)
	{
		var b = (ToggleAction)sender;

		m_threadedView = b.Active;

		ExecuteQuery();
	}
}
