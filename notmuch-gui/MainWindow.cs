using System;
using System.Diagnostics;
using System.IO;
using Gtk;
using NotMuchGUI;
using NM = NotMuch;
using System.Collections.Generic;
using System.Linq;
using UI = Gtk.Builder.ObjectAttribute;

public class MainWindow: Window
{
	List<string> m_allTags = new List<string>();

	[UI] Label label1;
	[UI] Label label3;
	[UI] Entry queryEntry;

	[UI] Gtk.Action goBackAction;
	[UI] Gtk.Action goForwardAction;
	[UI] ToggleAction threadedAction;

	[UI] Gtk.Action quitAction;
	[UI] Gtk.Action GCAction;
	[UI] Gtk.Action newAction;
	[UI] Gtk.Action replyAction;
	[UI] Gtk.Action replyAllAction;
	[UI] ToggleAction msgSourceAction;
	[UI] ToggleAction htmlSrcAction;
	[UI] Gtk.Action fetchAction;
	[UI] Gtk.Action deleteAction;
	[UI] Gtk.Action refreshAction;
	[UI] ToggleAction toggleReadAction;

	[UI] Box hbox3;
	[UI] Paned hpaned1;

	QueryWidget querywidget;
	MessageListWidget messageListWidget;

	MessageWidget messagewidget;
	TagsWidget tagsWidget;

	public MainWindow(Builder builder, IntPtr handle)
		: base(handle)
	{
		builder.Autoconnect(this);

		LoadAccelMap();

		this.Icon = Gdk.Pixbuf.LoadFromResource("NotMuchGUI.mail.png");

		this.Destroyed += (sender, e) =>
		{
			Application.Quit();
		};

		goBackAction.Activated += OnGoBackActionActivated;
		goForwardAction.Activated += OnGoForwardActionActivated;
		threadedAction.Activated += OnThreadedActionActivated;

		GCAction.Activated += OnGcActionActivated;
		newAction.Activated += OnNewActionActivated;
		replyAction.Activated += OnReplyActionActivated;
		replyAllAction.Activated += OnReplyAllActionActivated;
		msgSourceAction.Activated += OnMsgSrcActionActivated;
		htmlSrcAction.Activated += OnHtmlSrcActionActivated;
		fetchAction.Activated += OnFetchActionActivated;
		deleteAction.Activated += OnDeleteActionActivated;
		refreshAction.Activated += OnRefreshActionActivated;
		toggleReadAction.Activated += OnToggleReadActionActivated;
		quitAction.Activated += (sender, e) => Application.Quit();

		querywidget = new QueryWidget();
		hpaned1.Pack1(querywidget, true, true);

		messageListWidget = new MessageListWidget();
		hpaned1.Pack2(messageListWidget, true, true);

		messagewidget = new MessageWidget();
		hbox3.Add(messagewidget);

		tagsWidget = new TagsWidget();
		hbox3.Add(tagsWidget);

		queryEntry.Activated += OnQueryEntryActivated;

		messageListWidget.MessageSelected += OnMessageListWidgetMessageSelected;
		messageListWidget.CountsChanged += OnMessageListWidgetCountsChanged;

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

	void LoadAccelMap()
	{
		var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MainWindow.accel");

		if (System.IO.File.Exists(path) == false)
			return;

		Gtk.AccelMap.Load(path);
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

	void OnQuerySelected(string query)
	{
		queryEntry.Text = query;
		queryEntry.Activate();
	}

	void OnMessageListWidgetCountsChanged(object sender, EventArgs e)
	{
		label3.Text = String.Format("{0}/{1} msgs", messageListWidget.Count, messageListWidget.TotalCount);
	}

	void OnMessageListWidgetMessageSelected(object sender, EventArgs e)
	{
		var ids = messageListWidget.GetSelectedMessageIDs();

		tagsWidget.UpdateTagsView(ids);

		if (ids.Length == 0)
		{
			messagewidget.Clear();
			return;
		}

		using (var cdb = new CachedDB())
		{
			var db = cdb.Database;
		
			var nmmsg = db.FindMessage(ids[0]);

			if (nmmsg.IsNull)
			{
				messagewidget.ShowError(string.Format("Failed to find message from database '{0}'", ids[0]));
				return;
			}

			var filename = nmmsg.FileName;

			if (File.Exists(filename) == false)
			{
				messagewidget.ShowError(string.Format("Failed to find email file '{0}'", filename));
				return;
			}

			MimeKit.MimeMessage mkmsg;

			try
			{
				mkmsg = MimeKit.MimeMessage.Load(filename);
			}
			catch (Exception exc)
			{
				messagewidget.ShowError(string.Format(
					"Failed to parse message from '{0}':\n{1}", filename, exc.Message));
				return;
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

			messagewidget.ShowEmail(mkmsg, nmmsg.FileName, nmmsg.ThreadID);
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

	void OnGcActionActivated(object sender, EventArgs e)
	{
		var sw = Stopwatch.StartNew();

		GC.Collect();
		GC.WaitForPendingFinalizers();

		sw.Stop();

		Console.WriteLine("GC in {0} ms", sw.ElapsedMilliseconds);
	}

	void OnReplyAllActionActivated(object sender, EventArgs e)
	{
		Reply(true);
	}

	void OnReplyActionActivated(object sender, EventArgs e)
	{
		Reply(false);
	}

	void Reply(bool replyAll)
	{
		var wnd = ComposeWindow.Create();

		var msgId = messageListWidget.GetSelectedMessageID();

		if (msgId == null)
			return;

		wnd.Reply(msgId, replyAll);

		wnd.ShowAll();
	}

	void OnNewActionActivated(object sender, EventArgs e)
	{
		var wnd = ComposeWindow.Create();

		wnd.New();

		wnd.ShowAll();
	}

	LinkedList<QueryHistoryItem> m_history = new LinkedList<QueryHistoryItem>();
	LinkedListNode<QueryHistoryItem> m_currentItem;
	bool m_skipExecute;

	void ExecuteQuery(QueryHistoryItem item, bool retainSelection = false)
	{
		messageListWidget.ExecuteQuery(item.Query, item.Threaded, retainSelection);
	}

	void OnQueryEntryActivated(object sender, EventArgs e)
	{
		if (m_skipExecute)
			return;

		ExecuteNewUIQuery(false);
	}

	void OnThreadedActionActivated(object sender, EventArgs e)
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

	void OnRefreshActionActivated(object sender, EventArgs e)
	{
		ExecuteQuery(m_currentItem.Value, true);
	}

	void OnMsgSrcActionActivated(object sender, EventArgs e)
	{
		var id = messageListWidget.GetCurrentMessageID();
		if (id == null)
			return;

		var wnd = MessageSourceWindow.Create();
		wnd.ShowMessage(id);
		wnd.ShowAll();
	}

	void OnHtmlSrcActionActivated(object sender, EventArgs e)
	{
		messagewidget.ShowHtmlSource = htmlSrcAction.Active;
	}

	void OnFetchActionActivated(object sender, EventArgs e)
	{
		var exe = MainClass.AppKeyFile.GetStringOrNull("fetch", "cmd");
		if (exe == null)
			throw new Exception();

		var dlg = TermDialog.Create();
		dlg.TransientFor = this;
		dlg.Start(exe);
		var resp = (ResponseType)dlg.Run();

		Console.WriteLine("got resp {0}", resp);

		dlg.Destroy();
	}

	void OnToggleReadActionActivated(object sender, EventArgs e)
	{
		var ids = messageListWidget.GetSelectedMessageIDs();

		if (ids.Length == 0)
			return;

		using (var cdb = new CachedDB(true))
		{
			var db = cdb.Database;

			var curMsg = db.GetMessage(ids[0]);

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

	void OnDeleteActionActivated(object sender, EventArgs e)
	{
		var ids = messageListWidget.GetSelectedMessageIDs();

		if (ids.Length == 0)
			return;

		using (var cdb = new CachedDB(true))
		{
			var db = cdb.Database;

			var curMsg = db.GetMessage(ids[0]);

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

	void OnGoBackActionActivated(object sender, EventArgs e)
	{
		m_currentItem = m_currentItem.Previous;
		UpdateAfterCurrentItemChange();
	}

	void OnGoForwardActionActivated(object sender, EventArgs e)
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
