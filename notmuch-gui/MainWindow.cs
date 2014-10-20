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
	DebugWindow m_dbgWnd;
	List<string> m_allTags = new List<string>();

	[UI] Label label1;
	[UI] Label label3;
	[UI] Entry queryEntry;
	[UI] QueryWidget querywidget;
	[UI] MessageListWidget messageListWidget;
	[UI] MessageWidget messagewidget1;
	[UI] Gtk.Action goBackAction;
	[UI] Gtk.Action goForwardAction;
	[UI] Gtk.ToggleAction dbgAction;
	[UI] Gtk.ToggleAction threadedAction;
	[UI] Gtk.ToggleAction msgSrcAction;
	[UI] Box hbox3;

	TagsWidget tagsWidget;

	public MainWindow(Builder builder, IntPtr handle) : base(handle)
	{
		builder.Autoconnect (this);

		tagsWidget = new TagsWidget();
		hbox3.Add(tagsWidget);

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

		this.DeleteEvent += OnDeleteEvent;

		return;

		threadedAction.Active = true;

		using (var cdb = new CachedDB())
		{
			var db = cdb.Database;

			foreach (var tag in db.GetAllTags())
				m_allTags.Add(tag);
		}

		tagsWidget.SetDBTags(m_allTags);
		tagsWidget.MsgTagsUpdatedEvent += messageListWidget.RefreshMessages;

		querywidget.QuerySelected += OnQuerySelected;

		messageListWidget.FocusThreadEvent += (tid) =>
		{
			queryEntry.Text = string.Format("thread:{0}", tid);
			queryEntry.Activate();
		};

		messageListWidget.MyFocus();

	}

	protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
	{
		bool handled = base.OnKeyPressEvent(evnt);

		if (handled)
			return true;

		switch (evnt.Key)
		{
			case Gdk.Key.m:
				OnToggleReadActionActivated(null, null);
				return true;

			case Gdk.Key.Delete:
				OnDeleteActionActivated(null, null);
				return true;

			default:
				return false;
		}
	}

	protected void OnDeleteEvent(object sender, DeleteEventArgs a)
	{
		//querywidget.CancelUpdate();
		//messageListWidget.Cancel();

		Application.Quit();
		a.RetVal = true;
	}

	void OnQuerySelected(string query)
	{
		queryEntry.Text = query;
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
		
			var nmmsg = db.FindMessage(ids[0]);

			if (nmmsg.IsNull)
			{
				Console.WriteLine("can't find message");
				messagewidget1.Clear();
				return;
			}

			var filename = nmmsg.FileName;

			MimeKit.MimeMessage mkmsg;

			try
			{
				mkmsg = MimeKit.MimeMessage.Load(filename);
			}
			catch (Exception exc)
			{
				DialogHelpers.ShowDialog(this, MessageType.Error,
					"Failed to parse message",
					"Failed to parse message from '{0}':\n{1}", filename, exc.Message);
				return;
			}

			if (m_dbgWnd != null)
			{
				m_dbgWnd.SetSrc(File.ReadAllText(filename));

				var sb = new System.Text.StringBuilder();
				MimeKitHelpers.DumpMessage(mkmsg, sb, 0);
				m_dbgWnd.SetDump(sb.ToString());
			}

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

			messagewidget1.ShowEmail(mkmsg, nmmsg.FileName, nmmsg.ThreadID);
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
		var wnd = new ComposeWindow();
		wnd.ParentWindow = this.Window;

		var msgId = messageListWidget.GetCurrentMessageID();
		wnd.Reply(msgId, replyAll);

		wnd.Show();
	}

	protected void OnNewActionActivated(object sender, EventArgs e)
	{
		var wnd = new ComposeWindow();
		wnd.ParentWindow = this.Window;

		wnd.New();

		wnd.Show();
	}

	protected void OnQueryEntryChanged(object sender, EventArgs e)
	{
		//var queryStr = queryEntry.Text;

		//ExecuteQuery(queryStr);
	}

	LinkedList<QueryHistoryItem> m_history = new LinkedList<QueryHistoryItem>();
	LinkedListNode<QueryHistoryItem> m_currentItem;
	bool m_skipExecute;

	void ExecuteQuery(QueryHistoryItem item, bool retainSelection = false)
	{
		messageListWidget.ExecuteQuery(item.Query, item.Threaded, retainSelection);
	}

	protected void OnQueryEntryActivated(object sender, EventArgs e)
	{
		if (m_skipExecute)
			return;

		ExecuteNewUIQuery(false);
	}

	protected void OnThreadedActionActivated(object sender, EventArgs e)
	{
		if (m_skipExecute)
			return;

		ExecuteNewUIQuery(true);
	}

	void ExecuteNewUIQuery(bool retainSelection)
	{
		if (queryEntry.Text.Length == 0)
			return;

		var item = new QueryHistoryItem()
		{
			Query = queryEntry.Text,
			Threaded = threadedAction.Active,
		};

		if (m_currentItem != null && item.Equals(m_currentItem.Value))
			return;

		ExecuteQuery(item, retainSelection);

		var node = new LinkedListNode<QueryHistoryItem>(item);

		if (m_currentItem != null)
		{
			while (m_currentItem != m_history.Last)
				m_history.RemoveLast();
		}

		m_history.AddLast(node);
		m_currentItem = node;

		goBackAction.Sensitive = m_currentItem.Previous != null;
		goForwardAction.Sensitive = m_currentItem.Next != null;
	}

	protected void OnRefreshActionActivated(object sender, EventArgs e)
	{
		ExecuteQuery(m_currentItem.Value, true);
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
			//m_dbgWnd = new DebugWindow();
			m_dbgWnd.ShowAll();
		}
	}

	protected void OnMsgSrcActionActivated(object sender, EventArgs e)
	{
		messagewidget1.ShowHtmlSource = msgSrcAction.Active;
	}

	protected void OnFetchActionActivated(object sender, EventArgs e)
	{
		var exe = MainClass.AppKeyFile.GetStringOrNull("fetch", "cmd");
		if (exe == null)
			throw new Exception();

		var dlg = new TermDialog();
		dlg.ParentWindow = this.RootWindow;
		dlg.Start(exe);
		var resp = (ResponseType)dlg.Run();

		Console.WriteLine("got resp {0}", resp);

		dlg.Destroy();
	}

	protected void OnProcessActionActivated (object sender, EventArgs e)
	{
		var dlg = new TermDialog();
		dlg.ParentWindow = this.RootWindow;
		dlg.Start(MainClass.NotmuchExe, "new");
		var resp = (ResponseType)dlg.Run();

		Console.WriteLine("got resp {0}", resp);

		dlg.Destroy();

		OnRefreshActionActivated(null, null);
	}

	protected void OnToggleReadActionActivated(object sender, EventArgs e)
	{
		var ids = messageListWidget.GetSelectedMessageIDs();

		if (ids.Length == 0)
			return;

		using (var cdb = new CachedDB(true))
		{
			var db = cdb.Database;

			var curId = messageListWidget.GetCurrentMessageID();

			var curMsg = db.GetMessage(curId);

			bool unread = curMsg.GetTags().Contains("unread");

			unread = !unread;

			foreach (var id in ids)
			{
				var msg = db.GetMessage(id);

				if (unread)
					msg.AddTag("unread");
				else
					msg.RemoveTag("unread");
			}
		}

		messageListWidget.RefreshMessages(ids);
		tagsWidget.UpdateTagsView(ids);
	}

	protected void OnDeleteActionActivated(object sender, EventArgs e)
	{
		var ids = messageListWidget.GetSelectedMessageIDs();

		if (ids.Length == 0)
			return;

		using (var cdb = new CachedDB(true))
		{
			var db = cdb.Database;

			var curId = messageListWidget.GetCurrentMessageID();

			var curMsg = db.GetMessage(curId);

			bool deleted = curMsg.GetTags().Contains("deleted");

			deleted = !deleted;

			foreach (var id in ids)
			{
				var msg = db.FindMessage(id);

				if (deleted)
					msg.AddTag("deleted");
				else
					msg.RemoveTag("deleted");
			}
		}

		messageListWidget.RefreshMessages(ids);
		tagsWidget.UpdateTagsView(ids);
	}

	protected void OnGoBackActionActivated(object sender, EventArgs e)
	{
		m_currentItem = m_currentItem.Previous;
		UpdateAfterCurrentItemChange();
	}

	protected void OnGoForwardActionActivated(object sender, EventArgs e)
	{
		m_currentItem = m_currentItem.Next;
		UpdateAfterCurrentItemChange();
	}

	void UpdateAfterCurrentItemChange()
	{
		goBackAction.Sensitive = m_currentItem.Previous != null;
		goForwardAction.Sensitive = m_currentItem.Next != null;

		ExecuteQuery(m_currentItem.Value, false);

		m_skipExecute = true;

		queryEntry.Text = m_currentItem.Value.Query;
		threadedAction.Active = m_currentItem.Value.Threaded;

		m_skipExecute = false;
	}
}
