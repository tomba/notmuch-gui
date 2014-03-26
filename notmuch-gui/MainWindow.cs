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
	QueryCountUpdater m_queryCountUpdater;

	public MainWindow() : base(Gtk.WindowType.Toplevel)
	{
		Build();

		CachedDB.DBOpenEvent += (bool write) =>
		{
			Application.Invoke((s, o) =>
			{
				label1.Text = write ? "Writing..." : "Reading...";
			});
		};

		CachedDB.DBCloseEvent += () =>
		{
			Application.Invoke((s, o) =>
			{
				label1.Text = "";
			});
		};

		messageListWidget.ThreadedView = true;

		threadedAction.Active = messageListWidget.ThreadedView;

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

		tagsWidget.SetDBTags(m_allTags);
		tagsWidget.MsgTagsUpdatedEvent += messageListWidget.RefreshMessages;

		GLib.Idle.Add(() =>
		{
			// select first items
			queryTreeview.SetCursor(TreePath.NewFirst(), null, false);
			messageListWidget.MyFocus();
			return false;
		});

		m_queryCountUpdater = new QueryCountUpdater();

		m_queryCountUpdater.QueryCountCalculated += (key, count, unread) =>
		{
			var iter = m_queryStore.Find(i => (string)m_queryStore.GetValue(i, 0) == key);

			if (iter.UserData != IntPtr.Zero)
				m_queryStore.SetValue(iter, unread ? 2 : 1, count);
		};

		var queries = m_queryStore.AsEnumerable().Select(arr => (string)arr[0]).ToArray();
		m_queryCountUpdater.Start(queries);

		this.KeyPressEvent += HandleKeyPressEvent;
	}

	void HandleKeyPressEvent(object o, KeyPressEventArgs args)
	{
		if (args.Event.Key == Gdk.Key.m)
		{
			bool unread;

			using (var cdb = new CachedDB())
			{
				var db = cdb.Database;

				var curId = messageListWidget.GetCurrentMessageID();

				var msg = db.GetMessage(curId);

				unread = msg.GetTags().Contains("unread");

				unread = !unread;
			}

			var selected = messageListWidget.GetSelectedMessageIDs();

			using (var cdb = new CachedDB(true))
			{
				var db = cdb.Database;

				foreach (var id in selected)
				{
					var msg = db.GetMessage(id);

					if (unread)
						msg.AddTag("unread");
					else
						msg.RemoveTag("unread");
				}
			}

			messageListWidget.RefreshMessages(selected);
		}
	}

	void MyCellDataFunc(Gtk.TreeViewColumn column, Gtk.CellRenderer _cell, Gtk.TreeModel model, Gtk.TreeIter iter)
	{
		Gtk.CellRendererText cell = (Gtk.CellRendererText)_cell;

		int unread = (int)model.GetValue(iter, 2);

		cell.Weight = unread > 0 ? (int)Pango.Weight.Bold : (int)Pango.Weight.Normal;
	}

	void SetupQueryList()
	{
		var re = new CellRendererText();
		var c = queryTreeview.AppendColumn("Query", re, "text", 0);
		c.Expand = true;
		c.Resizable = true;
		c.Reorderable = true;
		re.Ellipsize = Pango.EllipsizeMode.Middle;
		c.SetCellDataFunc(re, MyCellDataFunc);

		c = queryTreeview.AppendColumn("C", new CellRendererText(), "text", 1);
		c.Resizable = true;
		c.Reorderable = true;

		c = queryTreeview.AppendColumn("U", new CellRendererText(), "text", 2);
		c.Resizable = true;
		c.Reorderable = true;

		var queryStore = new Gtk.ListStore(typeof(string), typeof(int), typeof(int));

		queryTreeview.Model = queryStore;

		var uiTags = MainClass.AppKeyFile.GetStringListOrNull("ui", "queries");
		if (uiTags != null)
		{
			foreach (var tag in uiTags)
				queryStore.AppendValues(tag, 0, 0);
		}

		m_queryStore = queryStore;
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		m_queryCountUpdater.Cancel();
		messageListWidget.Cancel();

		Application.Quit();
		a.RetVal = true;
	}

	protected void OnQueryTreeviewCursorChanged(object sender, EventArgs e)
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

	protected void OnMessageListWidgetCountsChanged(object sender, EventArgs e)
	{
		label3.Text = String.Format("{0}/{1} msgs", messageListWidget.Count, messageListWidget.TotalCount);
	}

	protected void OnMessageListWidgetMessageSelected(object sender, EventArgs e)
	{
		var ids = messageListWidget.GetSelectedMessageIDs();

		tagsWidget.UpdateTagsView(ids);

		if (ids.Length == 0)
		{
			messagewidget1.Clear();
			return;
		}

		using (var cdb = new CachedDB())
		{
			var db = cdb.Database;
		
			var msg = db.FindMessage(ids[0]);

			if (msg.IsNull)
			{
				Console.WriteLine("can't find message");
				messagewidget1.Clear();
				return;
			}

			var filename = msg.FileName;

			if (m_dbgWnd != null)
			{
				m_dbgWnd.SetSrc(File.ReadAllText(filename));
			}

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

	void ExecuteQuery(bool retainSelection = false)
	{
		var queryString = queryEntry.Text;

		if (string.IsNullOrWhiteSpace(dateSearchEntry.Text) == false)
			queryString = queryString + String.Format(" date:{0}", dateSearchEntry.Text);

		messageListWidget.ExecuteQuery(queryString, retainSelection);
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

	protected void OnRefreshActionActivated(object sender, EventArgs e)
	{
		ExecuteQuery(true);
	}

	protected void OnMsgSrcActionActivated(object sender, EventArgs e)
	{
		messagewidget1.ShowHtmlSource = msgSrcAction.Active;
	}

	protected void OnRefreshAction1Activated(object sender, EventArgs e)
	{
	}

	protected void OnFetchAction1Activated(object sender, EventArgs e)
	{
		var wnd = new RunWindow();
		wnd.ParentWindow = this.RootWindow;
		wnd.ShowAll();
		wnd.Run();
	}
}
