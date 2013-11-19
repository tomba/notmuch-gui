using System;
using Gtk;
using System.Diagnostics;
using NM = NotMuch;

namespace NotMuchGUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MessageListWidget : Gtk.Bin
	{
		public event EventHandler MessageSelected;
		public event EventHandler CountsChanged;

		bool m_cancelProcessing;
		bool m_processing;

		public bool ThreadedView { get; set; }

		public int TotalCount { get; private set; }
		public int Count { get; private set; }

		public MessageListWidget()
		{
			this.Build();

			SetupMessagesTreeView();

			messagesTreeview.CursorChanged += (sender, e) => 
			{
				if (this.MessageSelected != null)
					this.MessageSelected(this, e);
			};
		}

		void SetupMessagesTreeView()
		{
			var tv = messagesTreeview;

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

		void MyCellDataFunc(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
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
			TreeSelection selection = messagesTreeview.Selection;
			TreeModel model;
			TreeIter iter;

			if (!selection.GetSelected(out model, out iter))
				return null;

			var adap = (TreeModelAdapter)model;
			var myModel = (MessagesTreeModel)adap.Implementor;

			return myModel.GetMessageID(iter);
		}

		public void ExecuteQuery(string queryString)
		{
			if (m_processing)
			{
				Console.WriteLine("cancelling");

				m_cancelProcessing = true;

				Console.WriteLine("cancelling done");
			}

			if (m_processing)
			{
				Console.WriteLine("ProcessSearch already running");
				return;
			}

			if (string.IsNullOrWhiteSpace(queryString))
			{
				messagesTreeview.Model = new TreeModelAdapter(new MessagesTreeModel());
				return;
			}

			m_processing = true;

			Console.WriteLine("Query({0})", queryString);

			using (var cdb = new CachedDB())
			{
				var db = cdb.Database;

				RunQuery(db, queryString);
			}
		}

		void RunQuery(NM.Database db, string queryString)
		{
			var sw = Stopwatch.StartNew();

			var query = db.CreateQuery(queryString);

			query.Sort = NM.SortOrder.NEWEST_FIRST;

			long t1 = sw.ElapsedMilliseconds;

			int count = 0;

			var model = new MessagesTreeModel(query.CountMessages());

			messagesTreeview.Model = new TreeModelAdapter(model);

			long t2 = sw.ElapsedMilliseconds;

			if (!this.ThreadedView)
			{
				SearchMessages(query, model, ref count);
			}
			else
			{
				SearchThreads(query, model, ref count);
			}

			long t3 = sw.ElapsedMilliseconds;

			model.FinishAdding();

			this.Count = count;
			this.TotalCount = model.Count;

			if (this.CountsChanged != null)
				CountsChanged(this, EventArgs.Empty);

			long t4 = sw.ElapsedMilliseconds;

			sw.Stop();

			Console.WriteLine("Added {0} messages in {1}, {2}, {3}, {4} = {5} ms", count, t1, t2 - t1, t3 - t2, t4 - t3, t4);

			m_processing = false;
		}

		void SearchMessages(NM.Query query, MessagesTreeModel model, ref int count)
		{
			var msgs = query.SearchMessages();

			foreach (var msg in msgs)
			{
				if (m_cancelProcessing)
				{
					Console.WriteLine("CANCEL");
					m_cancelProcessing = false;
					return;
				}

				model.Append(msg.ID, 0);

				if (count == 0)
					messagesTreeview.SetCursor(TreePath.NewFirst(), null, false);

				count++;

				if (count % 100 == 0)
				{
					this.Count = count;
					this.TotalCount = model.Count;

					if (this.CountsChanged != null)
						CountsChanged(this, EventArgs.Empty);

					//Console.WriteLine("yielding");
					//await Task.Delay(100);
					Application.RunIteration();
					//await Task.Yield();
				}
			}
		}

		void SearchThreads(NM.Query query, MessagesTreeModel model, ref int count)
		{
			var threads = query.SearchThreads();
			int lastYield = 0;

			foreach (var thread in threads)
			{
				if (m_cancelProcessing)
				{
					Console.WriteLine("CANCEL");
					m_cancelProcessing = false;
					return;
				}

				//Console.WriteLine("thread {0}: {1}", thread.Id, thread.TotalMessages);

				bool firstLoop = count == 0;

				var msgs = thread.GetToplevelMessages();

				foreach (var msg in msgs)
					AddMsgsRecursive(model, msg, 0, ref count);

				if (firstLoop)
					messagesTreeview.SetCursor(TreePath.NewFirst(), null, false);

				if (count - lastYield > 500)
				{
					this.Count = count;
					this.TotalCount = model.Count;

					if (this.CountsChanged != null)
						CountsChanged(this, EventArgs.Empty);

					//Console.WriteLine("yielding");
					//await Task.Delay(1000);
					//await Task.Yield();
					Application.RunIteration();

					lastYield = count;
				}
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

