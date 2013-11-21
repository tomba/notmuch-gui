using System;
using Gtk;
using System.Diagnostics;
using NM = NotMuch;

namespace NotMuchGUI
{
	public partial class MessageListWidget : Gtk.TreeView
	{
		public event EventHandler MessageSelected;
		public event EventHandler CountsChanged;

		public bool ThreadedView { get; set; }

		public int TotalCount { get; private set; }

		public int Count { get; private set; }

		MyWork m_work;

		public MessageListWidget()
		{
			SetupMessagesTreeView();

			this.CursorChanged += (sender, e) =>
			{
				if (this.MessageSelected != null)
					this.MessageSelected(this, e);
			};
		}

		void SetupMessagesTreeView()
		{
			var tv = this;

			tv.FixedHeightMode = true;

			// colorize every other row
			//treeviewList.RulesHint = true;

			TreeViewColumn c;

			c = CreateTreeViewColumn("From", MessagesTreeModel.COL_FROM);
			c.FixedWidth = 350;
			tv.AppendColumn(c);

			c = CreateTreeViewColumn("Subject", MessagesTreeModel.COL_SUBJECT);
			c.Expand = true;
			c.FixedWidth = 150;
			tv.AppendColumn(c);

			c = CreateTreeViewColumn("Date", MessagesTreeModel.COL_DATE);
			c.FixedWidth = 150;
			tv.AppendColumn(c);

			c = CreateTreeViewColumn("Tags", MessagesTreeModel.COL_TAGS);
			c.FixedWidth = 150;
			tv.AppendColumn(c);

			c = CreateTreeViewColumn("D", MessagesTreeModel.COL_DEPTH);
			c.FixedWidth = 50;
			tv.AppendColumn(c);

			c = CreateTreeViewColumn("N", MessagesTreeModel.COL_MSG_NUM);
			c.FixedWidth = 50;
			tv.AppendColumn(c);
		}

		void MyCellDataFunc(TreeViewColumn column, CellRenderer cell, ITreeModel model, TreeIter iter)
		{
			bool unread = (bool)model.GetValue(iter, MessagesTreeModel.COL_UNREAD);

			var c = (Gtk.CellRendererText)cell;

			if (unread)
				c.Weight = (int)Pango.Weight.Bold;
			else
				c.Weight = (int)Pango.Weight.Normal;
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

		public string GetCurrentMessageID()
		{
			TreeSelection selection = this.Selection;
			ITreeModel model;
			TreeIter iter;

			if (!selection.GetSelected(out model, out iter))
				return null;

			var adap = (TreeModelAdapter)model;
			var myModel = (MessagesTreeModel)adap.Implementor;

			return myModel.GetMessageID(iter);
		}

		public void ExecuteQuery(string queryString)
		{
			if (m_work != null)
			{
				m_work.Cancel();
				m_work = null;
			}

			if (string.IsNullOrWhiteSpace(queryString))
			{
				this.Model = new TreeModelAdapter(new MessagesTreeModel());
				return;
			}

			m_work = new MyWork(this, queryString);
			m_work.Start();
		}

		class MyWork
		{
			MessageListWidget m_parent;
			string m_queryString;
			bool m_cancel;

			public MyWork(MessageListWidget parent, string queryString)
			{
				m_parent = parent;
				m_queryString = queryString;
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

				var model = new MessagesTreeModel(query.CountMessages());

				m_parent.Model = new TreeModelAdapter(model);

				long t2 = sw.ElapsedMilliseconds;

				if (!m_parent.ThreadedView)
				{
					SearchMessages(query, model, ref count);
				}
				else
				{
					SearchThreads(query, model, ref count);
				}

				if (m_cancel)
				{
					Console.WriteLine("Query exiting due to cancel '{0}'", m_queryString);

					sw.Stop();
					return;
				}

				long t3 = sw.ElapsedMilliseconds;

				model.FinishAdding();

				m_parent.Count = count;
				m_parent.TotalCount = model.Count;

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
						m_parent.SetCursor(TreePath.NewFirst(), null, false);

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
						bool shouldQuit = Application.RunIteration(false);

						if (shouldQuit)
							m_cancel = true;
					}

					if (m_cancel)
						return;
				}
			}

			void SearchThreads(NM.Query query, MessagesTreeModel model, ref int count)
			{
				var threads = query.SearchThreads();
				int lastUpdate = 0;

				foreach (var thread in threads)
				{
					//Console.WriteLine("thread {0}: {1}", thread.Id, thread.TotalMessages);

					bool firstLoop = count == 0;

					var msgs = thread.GetToplevelMessages();

					foreach (var msg in msgs)
						AddMsgsRecursive(model, msg, 0, ref count);

					if (firstLoop)
						m_parent.SetCursor(TreePath.NewFirst(), null, false);

					if (count - lastUpdate > 1000)
					{
						m_parent.Count = count;
						m_parent.TotalCount = model.Count;

						if (m_parent.CountsChanged != null)
							m_parent.CountsChanged(m_parent, EventArgs.Empty);

						lastUpdate = count;
					}

					bool shouldQuit = Application.RunIteration(false);

					if (shouldQuit)
						m_cancel = true;

					if (m_cancel)
						return;
				}
			}

			void AddMsgsRecursive(MessagesTreeModel model, NM.Message msg, int depth, ref int count)
			{
				//Console.WriteLine("append {0}", msg.Id);

				model.Append(msg.ID, depth);

				count++;

				var replies = msg.GetReplies();

				foreach (var reply in replies)
					AddMsgsRecursive(model, reply, depth + 1, ref count);
			}
		}
	}
}

