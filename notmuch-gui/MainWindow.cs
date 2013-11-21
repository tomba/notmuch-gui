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
using UI = Gtk.Builder.ObjectAttribute;

public partial class MainWindow: Gtk.Window
{
	ListStore m_queryStore;
	//DebugWindow m_dbgWnd;
	List<string> m_allTags = new List<string>();

	[UI] Gtk.TreeView queryTreeview;
	[UI] Gtk.Entry queryEntry;
	[UI] Gtk.Frame messageFrame;

	public MainWindow(Builder builder, IntPtr handle) : base(handle)
	{
		builder.Autoconnect(this);
		this.DeleteEvent += OnDeleteEvent;

		CreateSubWidgets();

		this.ShowAll();

		//threadedAction.Active = messageListWidget.ThreadedView;

		SetupQueryList();

		using (var cdb = new CachedDB())
		{
			var db = cdb.Database;

			foreach (var tag in db.GetAllTags())
			{
				m_allTags.Add(tag);
				m_queryStore.AppendValues(String.Format("tag:{0}", tag), 0, 0);
			}
		}

		// select first items
		queryTreeview.SetCursor(TreePath.NewFirst(), null, false);

		// XXX implement cancel
		var queries = m_queryStore.AsEnumerable().Select(arr => (string)arr[0]).ToArray();
		//Task.Factory.StartNew(() => UpdateQueryCounts(queries));
	}

	void CreateSubWidgets()
	{
		var builder = new Builder(null, "NotMuchGUI.UI.MainWindow.ui", null);
		var widget = new MessageWidget(builder, builder.GetObject("MessageWidget").Handle);
		messageFrame.Add(widget);
	}

	void UpdateQueryCounts(string[] queries)
	{
		var sw = Stopwatch.StartNew();

		var list = queries.SelectMany(q => new []
		{
			new { Key = q, Query = q, Column = 1 },
			new { Key = q, Query = q + " AND tag:unread", Column = 2 }
		});

		Parallel.ForEach(list,
			() =>
			{
				var db = MainClass.OpenDB();
				return db;
			},
			(val, state, db) =>
			{
				using (var query = db.CreateQuery(val.Query))
				{
					int count = query.CountMessages();

					GLib.Idle.Add(() =>
					{
						var iter = m_queryStore.Find(i => (string)m_queryStore.GetValue(i, 0) == val.Key);

						if (iter.UserData != IntPtr.Zero)
							m_queryStore.SetValue(iter, val.Column, count);

						return false;
					});
				}

				return db;
			},
			(db) =>
			{
				db.Dispose();
			}
		);

		sw.Stop();

		Console.WriteLine("Updated query counts in {0} ms", sw.ElapsedMilliseconds);
	}

	void SetupQueryList()
	{
		var c = queryTreeview.AppendColumn("Query", new CellRendererText(), "text", 0);
		c.Expand = true;
		c.Resizable = true;
		c.Reorderable = true;

		c = queryTreeview.AppendColumn("C", new CellRendererText(), "text", 1);
		c.Resizable = true;
		c.Reorderable = true;

		c = queryTreeview.AppendColumn("U", new CellRendererText(), "text", 2);
		c.Resizable = true;
		c.Reorderable = true;

		var queryStore = new Gtk.ListStore(typeof(string), typeof(int), typeof(int));

		queryTreeview.Model = queryStore;

		queryStore.AppendValues("ttlampopumppuhuolto", 0, 0);
		queryStore.AppendValues("Tomi", 0, 0);
		queryStore.AppendValues("from:ti.com", 0, 0);
		queryStore.AppendValues("from:linkedin", 0, 0);
		queryStore.AppendValues("to:linux-kernel@vger.kernel.org", 0, 0);
		queryStore.AppendValues("*", 0, 0);
		queryStore.AppendValues("", 0, 0);

		m_queryStore = queryStore;
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		Application.Quit();
		a.RetVal = true;
	}

	protected void OnQueryTreeviewCursorChanged(object sender, EventArgs e)
	{
		TreeSelection selection = (sender as TreeView).Selection;
		ITreeModel model;
		TreeIter iter;

		string queryString = "";

		if (selection.GetSelected(out model, out iter))
			queryString = (string)model.GetValue(iter, 0);

		queryEntry.Text = queryString;
		queryEntry.Activate();
	}
	#if asd
	protected void OnMessageListWidgetCountsChanged (object sender, EventArgs e)
	{
		label3.Text = String.Format("{0}/{1} msgs", messageListWidget.Count, messageListWidget.TotalCount);
	}

	protected void OnMessageListWidgetMessageSelected (object sender, EventArgs e)
	{
		var id = messageListWidget.GetCurrentMessageID();

		if (id == null)
			return;

		using (var cdb = new CachedDB())
		{
			var db = cdb.Database;
		
			var msg = db.FindMessage(id);

			if (msg.IsNull)
			{
				Console.WriteLine("can't find message");
				return;
			}

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

	void Reply(bool replyAll)
	{
		var msgId = messageListWidget.GetCurrentMessageID();

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

	void ExecuteQuery()
	{
		var queryString = queryEntry.Text;

		if (string.IsNullOrWhiteSpace(dateSearchEntry.Text) == false)
			queryString = queryString + String.Format(" date:{0}", dateSearchEntry.Text);

		messageListWidget.ExecuteQuery(queryString);
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

		messageListWidget.ThreadedView = b.Active;

		ExecuteQuery();
	}
#endif
}
