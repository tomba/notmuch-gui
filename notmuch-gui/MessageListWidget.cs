using System;
using Gtk;
using System.Diagnostics;
using NM = NotMuch;
using System.Linq;
using System.Collections.Generic;

namespace NotMuchGUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MessageListWidget : Gtk.Bin
	{
		public event EventHandler MessageSelected;
		public event EventHandler CountsChanged;

		public bool ThreadedView { get; set; }

		public int TotalCount { get; private set; }

		public int Count { get; private set; }

		MyWork m_work;

		public MessageListWidget()
		{
			this.Build();

			messagesTreeview.Selection.Mode = SelectionMode.Multiple;

			SetupMessagesTreeView();

			messagesTreeview.Selection.Changed += (sender, e) =>
			{
				if (this.MessageSelected != null)
					this.MessageSelected(this, e);
			};
		}

		public void MyFocus()
		{
			messagesTreeview.GrabFocus();
		}

		void SetupMessagesTreeView()
		{
			var tv = messagesTreeview;

			tv.FixedHeightMode = true;

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

			c = CreateTreeViewColumn("Tags", (int)MessageListColumns.Tags);
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

		void SetCommonCellSettings(Gtk.TreeViewColumn column, Gtk.CellRendererText cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			MessageListFlags flags = (MessageListFlags)model.GetValue(iter, (int)MessageListColumns.Flags);
			int threadNum = (int)model.GetValue(iter, (int)MessageListColumns.ThreadNum);

			cell.Weight = (flags & MessageListFlags.Unread) != 0 ? (int)Pango.Weight.Bold : (int)Pango.Weight.Normal;

			if ((flags & MessageListFlags.Match) == 0)
				cell.ForegroundGdk = new Gdk.Color(200, 200, 200);
			else
				cell.Foreground = null;

			if (threadNum % 2 == 0)
				cell.CellBackgroundGdk = CellBgColor1;
			else
				cell.CellBackgroundGdk = CellBgColor2;
		}

		void MyCellDataFunc(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			var c = (Gtk.CellRendererText)cell;

			SetCommonCellSettings(column, c, model, iter);
		}

		void DateCellDataFunc(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			var c = (Gtk.CellRendererText)cell;

			var stamp = (long)model.GetValue(iter, (int)MessageListColumns.Date);

			c.Text = NM.Utils.NotmuchTimeToDateTime(stamp).ToLocalTime().ToString("g");

			SetCommonCellSettings(column, c, model, iter);
		}

		TreeViewColumn CreateTreeViewColumn(string name, int col)
		{
			var re = new Gtk.CellRendererText();
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
			var re = new Gtk.CellRendererText();
			var c = new TreeViewColumn(name, re, "text", col)
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

		public string GetCurrentMessageID()
		{
			TreeSelection selection = messagesTreeview.Selection;

			int numRows = selection.CountSelectedRows();

			if (numRows != 1)
				return null;

			var path = selection.GetSelectedRows()[0];

			var model = selection.TreeView.Model;

			TreeIter iter;

			model.GetIter(out iter, path);

			return (string)model.GetValue(iter, (int)MessageListColumns.MessageId);
		}

		public string[] GetSelectedMessageIDs()
		{
			TreeSelection selection = messagesTreeview.Selection;

			var model = selection.TreeView.Model;

			var arr = selection.GetSelectedRows().Select(path =>
			{
				TreeIter iter;

				model.GetIter(out iter, path);

				return (string)model.GetValue(iter, (int)MessageListColumns.MessageId);
			}).ToArray();

			return arr;
		}

		public void ExecuteQuery(string queryString, bool retainSelection)
		{
			if (m_work != null)
			{
				m_work.Cancel();
				m_work = null;
			}

			if (string.IsNullOrWhiteSpace(queryString))
			{
				messagesTreeview.Model = new TreeModelAdapter(new MessagesTreeModel());
				return;
			}

			m_work = new MyWork(this, queryString, retainSelection);
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

		class MyWork
		{
			MessageListWidget m_parent;
			string m_queryString;
			bool m_cancel;
			string[] m_selectMsgIDs;

			public MyWork(MessageListWidget parent, string queryString, bool retainSelection)
			{
				m_parent = parent;
				m_queryString = queryString;
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

				query.Sort = NM.SortOrder.NEWEST_FIRST;

				long t1 = sw.ElapsedMilliseconds;

				int count = 0;

				long t2;

				if (!m_parent.ThreadedView)
				{
					var model = new MessagesTreeModel(query.CountMessages());

					m_parent.messagesTreeview.Model = new TreeModelAdapter(model);

					t2 = sw.ElapsedMilliseconds;

					SearchMessages(query, model, ref count);

					model.FinishAdding();
				}
				else
				{
					var model = new TreeStore(typeof(string), typeof(string), typeof(string), typeof(long), typeof(string), 
						typeof(int), typeof(int), typeof(int), typeof(int));

					t2 = sw.ElapsedMilliseconds;

					//model.SetSortColumnId((int)MessageListColumns.Date, SortType.Ascending);

					SearchThreads(query, model, out count);

					m_parent.messagesTreeview.Model = model;
					m_parent.messagesTreeview.ExpandAll();

					if (m_selectMsgIDs.Length == 0)
						ScrollToMostRecent(model);
					else
						SelectOldMessages(model);
				}

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

			void SearchMessages(NM.Query query, MessagesTreeModel model, ref int count)
			{
				var msgs = query.SearchMessages();

				foreach (var msg in msgs)
				{
					model.Append(msg.ID, 0);

					if (count == 0)
						m_parent.messagesTreeview.SetCursor(TreePath.NewFirst(), null, false);

					count++;

					if (count % 1000 == 0)
					{
						m_parent.Count = count;
						m_parent.TotalCount = model.Count;

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

			void SearchThreads(NM.Query query, TreeStore model, out int msgCount)
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

					// XXX max 1000 msgs
					if (msgCount > 1000)
						break;
					
					if (msgCount - lastUpdate > 1000)
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

			void ScrollToMostRecent(TreeStore model)
			{
				int num = model.IterNChildren();

				if (num == 0)
					return;

				TreeIter iter;

				var b = model.IterNthChild(out iter, num - 1);
				if (b == false)
					throw new Exception();

				while ((num = model.IterNChildren(iter)) != 0)
				{
					b = model.IterNthChild(out iter, iter, num - 1);
					if (b == false)
						throw new Exception();
				}

				var path = model.GetPath(iter);

				m_parent.messagesTreeview.ExpandToPath(path);

				m_parent.messagesTreeview.Selection.UnselectAll();
				m_parent.messagesTreeview.Selection.SelectPath(path);

				m_parent.messagesTreeview.ScrollToCell(path, null, false, 0, 0);
			}

			void SelectOldMessages(TreeStore model)
			{
				m_parent.messagesTreeview.Selection.UnselectAll();

				var l = new List<string>(m_selectMsgIDs);

				model.Foreach((m, path, iter) =>
				{
					var id = (string)m.GetValue(iter, (int)MessageListColumns.MessageId);
					
					if (l.Remove(id))
					{
						m_parent.messagesTreeview.ExpandToPath(path);
						m_parent.messagesTreeview.Selection.SelectIter(iter);
					}

					if (l.Count == 0)
					{
						m_parent.messagesTreeview.ScrollToCell(path, null, true, 0.5f, 0);
						return true;
					}
					
					return false;
				});
			}

			void AddMsgsRecursive(TreeStore model, NM.Message msg, int depth, ref int msgCount, ref TreeIter parent,
			                      int threadNum)
			{
				//Console.WriteLine("append {0}", msg.Id);

				var tags = msg.GetTags().ToList();

				TreeIter iter;

				if (parent.Equals(TreeIter.Zero))
					iter = model.InsertNode(0);
				else
					iter = model.AppendNode(parent);

				MessageListFlags flags = MessageListFlags.None;

				if (tags.Contains("unread"))
					flags |= MessageListFlags.Unread;

				if (msg.GetFlag(NM.MessageFlag.MATCH))
					flags |= MessageListFlags.Match;

				if (msg.GetFlag(NM.MessageFlag.EXCLUDED))
					flags |= MessageListFlags.Excluded;

				model.SetValues(iter,
					msg.ID,
					msg.From,
					msg.Subject,
					msg.DateStamp,
					string.Join("/", tags),
					(int)flags,
					depth,
					msgCount,
					threadNum);

				msgCount++;

				var replies = msg.GetReplies();

				foreach (var reply in replies)
					AddMsgsRecursive(model, reply, depth + 1, ref msgCount, ref iter, threadNum);
			}
		}
	}
}

