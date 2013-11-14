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
	ListStore m_queryStore;
	DebugWindow m_dbgWnd;
	List<string> m_allTags = new List<string>();
	bool m_threadedView = false;
	bool m_cancelProcessing;
	bool m_processing;

	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();

		threadedAction.Active = m_threadedView;

		SetupQueryList();
		SetupMailList();

		using (var db = MainClass.OpenDB())
		{
			var tags = db.AllTags;

			while (tags.Valid)
			{
				m_allTags.Add(tags.Current);
				m_queryStore.AppendValues(String.Format("tag:{0}", tags.Current));
				tags.Next();
			}
		}

		// select first items
		treeviewSearch.SetCursor(TreePath.NewFirst(), null, false);
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
		if (m_processing)
		{
			Console.WriteLine("cancelling");

			m_cancelProcessing = true;

			Console.WriteLine("cancelling done");
		}

		var queryString = queryEntry.Text;

		if (string.IsNullOrWhiteSpace(dateSearchEntry.Text) == false)
			queryString = queryString + String.Format(" date:{0}", dateSearchEntry.Text);

		if (m_processing)
		{
			Console.WriteLine("ProcessSearch already running");
			return;
		}

		if (string.IsNullOrWhiteSpace(queryString))
		{
			treeviewList.Model = new TreeModelAdapter(new MyTreeModel());
			return;
		}

		m_processing = true;

		Console.WriteLine("Query({0})", queryString);

		using (var db = MainClass.OpenDB())
		{
			RunQuery(db, queryString);
		}
	}

	void RunQuery(NM.Database db, string queryString)
	{
		var sw = Stopwatch.StartNew();

		var query = db.CreateQuery(queryString);

		query.Sort = NM.SortOrder.OLDEST_FIRST;

		long t1 = sw.ElapsedMilliseconds;

		int count = 0;

		var model = new MyTreeModel(query.Count);

		treeviewList.Model = new TreeModelAdapter(model);

		long t2 = sw.ElapsedMilliseconds;

		if (!m_threadedView)
		{
			var msgs = query.SearchMessages();

			while (msgs.Valid)
			{
				if (m_cancelProcessing)
				{
					Console.WriteLine("CANCEL");
					sw.Stop();
					m_processing = false;
					m_cancelProcessing = false;
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
					Application.RunIteration();
					//await Task.Yield();
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
				if (m_cancelProcessing)
				{
					Console.WriteLine("CANCEL");
					sw.Stop();
					m_processing = false;
					m_cancelProcessing = false;
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
					//await Task.Yield();
					Application.RunIteration();

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
	
		m_processing = false;
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

		var id = myModel.GetMessageID(iter);

		if (id == null)
			return;

		using (var db = MainClass.OpenDB())
		{
			var msg = db.FindMessage(id);

			tagsWidget.UpdateTagsView(msg, m_allTags);

			var filename = msg.FileName;

			int fd = Mono.Unix.Native.Syscall.open(filename, Mono.Unix.Native.OpenFlags.O_RDONLY);

			using (var readStream = new GMime.StreamFs(fd))
			{
				readStream.Owner = true;

				var p = new GMime.Parser(readStream);
				var gmsg = p.ConstructMessage();

				#if MBOX_PARSE_HACK
			// HACK: try skipping >From: line
			if (gmsg == null)
			{
				p.Dispose();

				gmsg = TryParseMboxMessage(readStream);

				if (gmsg != null)
					DialogHelpers.ShowDialog(this, MessageType.Warning, "Parsed old style mbox message", "Parsed old style mbox message '{0}'", filename);
			}
				#endif

				if (gmsg == null)
				{
					DialogHelpers.ShowDialog(this, MessageType.Error, "Failed to parse message", "Failed to parse message from '{0}'", filename);
					readStream.Close();
					return;
				}

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
	}
	#if MBOX_PARSE_HACK
	GMime.Message TryParseMboxMessage(GMime.StreamFs readStream)
	{
		readStream.Seek(0);

		var buf = new byte[1];

		if (readStream.Read(buf, 1) != 1)
			return null;

		if (buf[0] != (byte)'>')
			return null;

		while (buf[0] != (byte)'\n')
		{
			if (readStream.Read(buf, 1) != 1)
				return null;
		}

		var start = readStream.Tell();
		var end = readStream.Length;

		readStream.SetBounds(start, end);

		var p = new GMime.Parser(readStream);
		var gmsg = p.ConstructMessage();

		return gmsg;
	}
	#endif

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

	string GetCurrentMessageID()
	{
		TreeSelection selection = treeviewList.Selection;
		TreeModel model;
		TreeIter iter;

		if (!selection.GetSelected(out model, out iter))
			return null;

		var adap = (TreeModelAdapter)model;
		var myModel = (MyTreeModel)adap.Implementor;

		return myModel.GetMessageID(iter);
	}

	void Reply(bool replyAll)
	{
		var msgId = GetCurrentMessageID();

		if (msgId == null)
			return;

		string replyText;

		if (CmdHelpers.RunNotmuch(String.Format("reply --reply-to={0} id:{1}", replyAll ? "all" : "sender", msgId), out replyText) == false)
		{
			Console.WriteLine("Failed to construct reply with notmuch: {0}", replyText);
			return;
		}

		var tmpFile = System.IO.Path.GetTempFileName();

		File.WriteAllText(tmpFile, replyText);

		const string editorCmd = "gvim";
		const string editorArgs = "-f \"+set columns=100\" \"+set lines=50\" \"+set filetype=mail\" +6 {0}";

		using (var process = new Process())
		{
			var si = process.StartInfo;
			si.FileName = editorCmd;
			si.Arguments = String.Format(editorArgs, tmpFile);
			si.UseShellExecute = false;
			si.CreateNoWindow = true;

			var dlg = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Cancel,
				          "Editing.\n\nEditor command {0}\n\nPress cancel to kill the editor.", si.Arguments);

			process.EnableRaisingEvents = true;

			process.Exited += (sender, args) =>
			{
				Application.Invoke((s, e) =>
				{
					dlg.Destroy();
				});
			};

			dlg.Response += (o, args) =>
			{
				process.Kill();
			};

			process.Start();

			dlg.Run();

			process.WaitForExit();

			if (process.ExitCode != 0)
				Console.WriteLine("Failed to edit reply");
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
