using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gtk;
using NM = NotMuch;

namespace NotMuchGUI
{
	public class MessageListWidget : Bin
	{
		public event EventHandler MessageSelected;
		public event EventHandler CountsChanged;
		public event Action<string> FocusThreadEvent;

		public int TotalCount { get; private set; }

		public int Count { get; private set; }

		MyWork m_work;

		int m_maxMsgs;

		readonly ScrolledWindow m_scrolledWindow;
		readonly MyTreeView m_messagesTreeview;

		public MessageListWidget()
		{
			if (MainClass.AppKeyFile.GetIntegerOrFalse("ui", "max-msgs", out m_maxMsgs) == false)
				m_maxMsgs = 1000;

			m_scrolledWindow = new ScrolledWindow();
			this.Add(m_scrolledWindow);

			m_messagesTreeview = new MyTreeView()
			{
				FixedHeightMode = true,
				EnableTreeLines = true,
			};

			m_messagesTreeview.Selection.Mode = SelectionMode.Multiple;
			m_messagesTreeview.ButtonPressEvent += OnMessagesTreeviewButtonPressEvent;

			SetupMessagesTreeView();

			m_messagesTreeview.Selection.Changed += (sender, e) =>
			{
				if (this.MessageSelected != null)
					this.MessageSelected(this, e);
			};

			m_scrolledWindow.Add(m_messagesTreeview);
		}

		public void MyFocus()
		{
			m_messagesTreeview.GrabFocus();
		}

		void SetupMessagesTreeView()
		{
			var tv = m_messagesTreeview;

			TreeViewColumn c;

			c = CreateTreeViewColumn("From", (int)MessageListColumns.From);
			c.FixedWidth = 350;
			tv.AppendColumn(c);

			c = CreateTreeViewColumn("Subject", (int)MessageListColumns.Subject);
			c.Expand = true;
			c.FixedWidth = 150;
			tv.AppendColumn(c);

			c = CreateDateTreeViewColumn("Date", (int)MessageListColumns.Date);
			c.FixedWidth = 150;
			tv.AppendColumn(c);

			c = CreateTagsTreeViewColumn("Tags", (int)MessageListColumns.Tags);
			c.FixedWidth = 150;
			tv.AppendColumn(c);

			c = CreateTreeViewColumn("D", (int)MessageListColumns.Depth);
			c.FixedWidth = 50;
			tv.AppendColumn(c);

			c = CreateTreeViewColumn("N", (int)MessageListColumns.MsgNum);
			c.FixedWidth = 50;
			tv.AppendColumn(c);

			c = CreateTreeViewColumn("T", (int)MessageListColumns.ThreadNum);
			c.FixedWidth = 50;
			tv.AppendColumn(c);
		}

		static Gdk.Color CellBgColor1 = new Gdk.Color(255, 255, 255);
		static Gdk.Color CellBgColor2 = new Gdk.Color(235, 235, 255);

		void SetCommonCellSettings(TreeViewColumn column, CellRendererText cell, MessageTreeStore model, ref TreeIter iter)
		{
			MessageListFlags flags = model.GetFlags(ref iter);
			int threadNum = model.GetThreadNum(ref iter);

			cell.Weight = (flags & MessageListFlags.Unread) != 0 ? (int)Pango.Weight.Bold : (int)Pango.Weight.Normal;

			if ((flags & MessageListFlags.NoMatch) != 0)
				cell.ForegroundGdk = new Gdk.Color(200, 200, 200);
			else
				cell.Foreground = null;

			if (threadNum % 2 == 0)
				cell.CellBackgroundGdk = CellBgColor1;
			else
				cell.CellBackgroundGdk = CellBgColor2;

			cell.Strikethrough = (flags & MessageListFlags.Deleted) != 0;
		}

		void MyCellDataFunc(TreeViewColumn column, CellRenderer cell, ITreeModel _model, TreeIter iter)
		{
			var model = (MessageTreeStore)_model;
			var c = (CellRendererText)cell;

			SetCommonCellSettings(column, c, model, ref iter);
		}

		void DateCellDataFunc(TreeViewColumn column, CellRenderer cell, ITreeModel _model, TreeIter iter)
		{
			var model = (MessageTreeStore)_model;
			var c = (CellRendererText)cell;

			var stamp = model.GetDate(ref iter);

			c.Text = NM.Utils.NotmuchTimeToDateTime(stamp).ToLocalTime().ToString("g");

			SetCommonCellSettings(column, c, model, ref iter);
		}

		void TagsCellDataFunc(TreeViewColumn column, CellRenderer cell, ITreeModel _model, TreeIter iter)
		{
			var model = (MessageTreeStore)_model;
			var c = (CellRendererText)cell;

			var tags = model.GetTags(ref iter);

			c.Text = tags != null ? string.Join(", ", tags) : "";

			SetCommonCellSettings(column, c, model, ref iter);
		}

		TreeViewColumn CreateTreeViewColumn(string name, int col)
		{
			var re = new CellRendererText();
			var c = new TreeViewColumn(name, re, "text", col)
			{
				Expand = false,
				Sizing = TreeViewColumnSizing.Fixed,
				FixedWidth = 50,
				Resizable = true,
				Reorderable = true,
			};
			c.SetCellDataFunc(re, MyCellDataFunc);

			return c;
		}

		TreeViewColumn CreateDateTreeViewColumn(string name, int col)
		{
			var re = new CellRendererText();
			var c = new TreeViewColumn(name, re)
			{
				Expand = false,
				Sizing = TreeViewColumnSizing.Fixed,
				FixedWidth = 50,
				Resizable = true,
				Reorderable = true,
				//SortIndicator = true,
				//SortOrder =  SortType.Descending,
				SortColumnId = col,
			};

			c.SetCellDataFunc(re, DateCellDataFunc);

			return c;
		}

		TreeViewColumn CreateTagsTreeViewColumn(string name, int col)
		{
			var re = new CellRendererText();
			var c = new TreeViewColumn(name, re)
			{
				Expand = false,
				Sizing = TreeViewColumnSizing.Fixed,
				FixedWidth = 50,
				Resizable = true,
				Reorderable = true,
				//SortIndicator = true,
				//SortOrder =  SortType.Descending,
				SortColumnId = col,
			};

			c.SetCellDataFunc(re, TagsCellDataFunc);

			return c;
		}

		public void RefreshMessages(IEnumerable<string> ids)
		{
			string[] idArray = ids.ToArray();

			using (var cdb = new CachedDB())
			{
				var db = cdb.Database;

				m_messagesTreeview.Model.Foreach((m, path, iter) =>
				{
					var model = (MessageTreeStore)m;
					var id = model.GetMessageID(ref iter);

					if (!idArray.Contains(id))
						return false;

					var msg = db.FindMessage(id);

					model.UpdateValues(ref iter, msg);

					return false;
				});
			}
		}

		/// <summary>
		/// Gets the message ID for the message under cursor
		/// </summary>
		public string GetCurrentMessageID()
		{
			TreePath path;
			TreeViewColumn column;

			m_messagesTreeview.GetCursor(out path, out column);

			if (path == null)
				return null;

			var model = (MessageTreeStore)m_messagesTreeview.Model;

			TreeIter iter;

			model.GetIter(out iter, path);

			return model.GetMessageID(ref iter);
		}

		/// <summary>
		/// Get the message IDs for a single selected message, or NULL if none or more than one selected
		/// </summary>
		public string GetSelectedMessageID()
		{
			TreeSelection selection = m_messagesTreeview.Selection;

			var paths = selection.GetSelectedRows();

			if (paths.Length != 1)
				return null;

			var path = paths[0];

			var model = (MessageTreeStore)m_messagesTreeview.Model;

			TreeIter iter;

			model.GetIter(out iter, path);

			return model.GetMessageID(ref iter);
		}

		/// <summary>
		/// Get the message IDs for selected messages
		/// </summary>
		public string[] GetSelectedMessageIDs()
		{
			TreeSelection selection = m_messagesTreeview.Selection;

			var model = (MessageTreeStore)m_messagesTreeview.Model;

			var arr = selection.GetSelectedRows().Select(path =>
			{
				TreeIter iter;

				model.GetIter(out iter, path);

				return model.GetMessageID(ref iter);
			}).ToArray();

			return arr;
		}

		public void ExecuteQuery(string queryString, bool threaded, bool retainSelection)
		{
			if (m_work != null)
			{
				m_work.Cancel();
				m_work = null;
			}

			if (string.IsNullOrWhiteSpace(queryString))
			{
				m_messagesTreeview.Model = null;
				return;
			}

			m_work = new MyWork(this, queryString, threaded, retainSelection);
			m_work.Start();
		}

		public void Cancel()
		{
			if (m_work != null)
			{
				m_work.Cancel();
				m_work = null;
			}
		}

		[GLib.ConnectBeforeAttribute]
		void OnMessagesTreeviewButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			/* right click */
			if (args.Event.Type == Gdk.EventType.ButtonPress && args.Event.Button == 3)
			{
				Menu m = new Menu();
				var item = new MenuItem("Focus Thread");
				item.ButtonPressEvent += OnFocusThreadSelected;
				m.Add(item);
				m.ShowAll();
				m.Popup();
			}
		}

		void OnFocusThreadSelected(object sender, ButtonPressEventArgs e)
		{
			var id = GetCurrentMessageID();

			if (id == null)
				return;

			string threadID;

			using (var cdb = new CachedDB())
			{
				var msg = cdb.Database.FindMessage(id);
				threadID = msg.ThreadID;
			}

			if (this.FocusThreadEvent != null)
				this.FocusThreadEvent(threadID);
		}

		class MyWork
		{
			MessageListWidget m_parent;
			string m_queryString;
			bool m_cancel;
			string[] m_selectMsgIDs;
			bool m_threaded;

			public MyWork(MessageListWidget parent, string queryString, bool threaded, bool retainSelection)
			{
				m_parent = parent;
				m_queryString = queryString;
				m_threaded = threaded;
				if (retainSelection)
					m_selectMsgIDs = parent.GetSelectedMessageIDs();
				else
					m_selectMsgIDs = new string[0];
			}

			public void Cancel()
			{
				Console.WriteLine("Cancel Query '{0}'", m_queryString);
				m_cancel = true;
			}

			public void Start()
			{
				Console.WriteLine("Start Query '{0}'", m_queryString);

				using (var cdb = new CachedDB())
				{
					var db = cdb.Database;

					RunQuery(db, m_queryString);
				}

				if (m_parent.m_work == this)
					m_parent.m_work = null;
			}

			void RunQuery(NM.Database db, string queryString)
			{
				var sw = Stopwatch.StartNew();

				var query = db.CreateQuery(queryString);

				query.AddTagExclude("deleted");
				query.Exclude = NM.Exclude.TRUE;

				query.Sort = NM.SortOrder.NEWEST_FIRST;

				long t1 = sw.ElapsedMilliseconds;

				int count;

				long t2 = sw.ElapsedMilliseconds;

				var model = new MessageTreeStore();

				if (!m_threaded)
				{
					SearchMessages(query, model, out count);
					m_parent.m_messagesTreeview.Model = model;
				}
				else
				{
					//model.SetSortColumnId((int)MessageListColumns.Date, SortType.Ascending);

					SearchThreads(query, model, out count);

					m_parent.m_messagesTreeview.Model = model;
					m_parent.m_messagesTreeview.ExpandAll();
				}

				if (m_selectMsgIDs.Length == 0)
					m_parent.m_messagesTreeview.ScrollToMostRecent();
				else
					SelectOldMessages(model);

				if (m_cancel)
				{
					Console.WriteLine("Query exiting due to cancel '{0}'", m_queryString);

					sw.Stop();
					return;
				}

				long t3 = sw.ElapsedMilliseconds;

				m_parent.TotalCount = m_parent.Count = count;

				if (m_parent.CountsChanged != null)
					m_parent.CountsChanged(m_parent, EventArgs.Empty);

				long t4 = sw.ElapsedMilliseconds;

				sw.Stop();

				Console.WriteLine("Added {0} msgs in {1}, {2}, {3}, {4} = {5} ms '{6}'",
					count, t1, t2 - t1, t3 - t2, t4 - t3, t4, m_queryString);
			}

			void SearchMessages(NM.Query query, MessageTreeStore model, out int count)
			{
				count = 0;

				var msgs = query.SearchMessages();

				foreach (var msg in msgs)
				{
					TreeIter parent = TreeIter.Zero;

					AddMsg(model, msg, 0, count, ref parent, 0, MessageListFlags.None);

					count++;

					if (count >= m_parent.m_maxMsgs)
						break;

					// XXX
					if (count % m_parent.m_maxMsgs == 0)
					{
						m_parent.TotalCount = m_parent.Count = count;

						if (m_parent.CountsChanged != null)
							m_parent.CountsChanged(m_parent, EventArgs.Empty);
					}

					if (count % 10 == 0)
					{
						bool b = Application.RunIteration(false);
						if (b)
							m_cancel = true;
					}

					if (m_cancel)
						return;
				}
			}

			void SearchThreads(NM.Query query, MessageTreeStore model, out int msgCount)
			{
				var threads = query.SearchThreads();
				int lastUpdate = 0;

				TreeIter iter = TreeIter.Zero;

				msgCount = 0;
				int threadCount = 0;

				foreach (var thread in threads)
				{
					//Console.WriteLine("thread {0}: {1}", thread.Id, thread.TotalMessages);

					bool firstLoop = msgCount == 0;

					var msgs = thread.GetToplevelMessages();

					foreach (var msg in msgs)
						AddMsgsRecursive(model, msg, 0, ref msgCount, ref iter, threadCount);

					threadCount++;

					if (firstLoop)
					{
						// we could focus here to the most recent msg, which was just added.
						// if only gtktreeview would show things properly.
					}

					if (msgCount > m_parent.m_maxMsgs)
						break;

					// XXX
					if (msgCount - lastUpdate > m_parent.m_maxMsgs)
					{
						m_parent.Count = msgCount;
						//m_parent.TotalCount = model.Count;

						if (m_parent.CountsChanged != null)
							m_parent.CountsChanged(m_parent, EventArgs.Empty);

						lastUpdate = msgCount;
					}

					bool b = Application.RunIteration(false);
					if (b)
						m_cancel = true;

					if (m_cancel)
						return;
				}
			}

			void SelectOldMessages(MessageTreeStore model)
			{
				m_parent.m_messagesTreeview.Selection.UnselectAll();

				var l = new List<string>(m_selectMsgIDs);

				model.Foreach((m, path, iter) =>
				{
					var id = model.GetMessageID(ref iter);
					
					if (l.Remove(id))
					{
						m_parent.m_messagesTreeview.ExpandToPath(path);
						m_parent.m_messagesTreeview.Selection.SelectIter(iter);
					}

					if (l.Count == 0)
					{
						m_parent.m_messagesTreeview.ScrollToCell(path, null, true, 0.5f, 0);
						return true;
					}
					
					return false;
				});

				// if the new query didn't contain any of the old messages, scroll to end
				if (l.Count == m_selectMsgIDs.Length)
					m_parent.m_messagesTreeview.ScrollToMostRecent();
			}

			TreeIter AddMsg(MessageTreeStore model, NM.Message msg, int depth, int msgNum, ref TreeIter parent, int threadNum,
			                MessageListFlags flags)
			{
				//Console.WriteLine("append {0}", msg.Id);

				TreeIter iter;

				if (parent.Equals(TreeIter.Zero))
					iter = model.InsertNode(0);
				else
					iter = model.AppendNode(parent);

				model.SetValues(ref iter, msg, flags, depth, msgNum, threadNum);

				return iter;
			}

			void AddMsgsRecursive(MessageTreeStore model, NM.Message msg, int depth, ref int msgCount, ref TreeIter parent,
			                      int threadNum)
			{
				//Console.WriteLine("append {0}", msg.Id);

				MessageListFlags flags = MessageListFlags.None;

				if (!msg.GetFlag(NM.MessageFlag.MATCH))
					flags |= MessageListFlags.NoMatch;

				if (msg.GetFlag(NM.MessageFlag.EXCLUDED))
					flags |= MessageListFlags.Excluded;

				TreeIter iter = AddMsg(model, msg, depth, msgCount, ref parent, threadNum, flags);

				msgCount++;

				var replies = msg.GetReplies();

				foreach (var reply in replies)
					AddMsgsRecursive(model, reply, depth + 1, ref msgCount, ref iter, threadNum);
			}
		}
	}
}

