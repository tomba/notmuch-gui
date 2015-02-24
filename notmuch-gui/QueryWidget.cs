using System;
using System.Linq;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace NotMuchGUI
{
	public partial class QueryWidget : Bin
	{
		ListStore m_queryStore;
		QueryCountUpdater m_queryCountUpdater;

		public event Action<string> QuerySelected;

		[UI] TreeView queryTreeview;

		public QueryWidget()
		{
			Builder builder = new Gtk.Builder(null, "NotMuchGUI.UI.QueryWidget.ui", null);
			builder.Autoconnect(this);
			Add((Box)builder.GetObject("QueryWidget"));

			queryTreeview.Selection.Changed += OnQueryTreeviewSelectionChanged;

			SetupQueryList();

			this.Destroyed += (sender, e) =>
			{
				CancelUpdate();
			};

			AddUserQueries();
			AddTagQueries();

			m_queryCountUpdater = new QueryCountUpdater();

			m_queryCountUpdater.QueryCountCalculated += (key, count, unread) =>
			{
				var iter = m_queryStore.Find(i => (string)m_queryStore.GetValue(i, 0) == key);

				if (iter.UserData != IntPtr.Zero)
					m_queryStore.SetValue(iter, unread ? 1 : 2, count);
			};

			var queries = m_queryStore.AsEnumerable().Select(arr => (string)arr[0]).ToArray();
			m_queryCountUpdater.Start(queries);

			GLib.Idle.Add(() =>
			{
				// select first items
				queryTreeview.SetCursor(TreePath.NewFirst(), null, false);
				return false;
			});
		}

		public void CancelUpdate()
		{
			m_queryCountUpdater.Cancel();
		}

		void MyCellDataFunc(Gtk.TreeViewColumn column, Gtk.CellRenderer _cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
		{
			Gtk.CellRendererText cell = (Gtk.CellRendererText)_cell;

			int unread = (int)model.GetValue(iter, 1);

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

			c = queryTreeview.AppendColumn("Unread", new CellRendererText(), "text", 1);
			c.Resizable = true;
			c.Reorderable = true;

			c = queryTreeview.AppendColumn("Count", new CellRendererText(), "text", 2);
			c.Resizable = true;
			c.Reorderable = true;

			var queryStore = new Gtk.ListStore(typeof(string), typeof(int), typeof(int));

			queryTreeview.Model = queryStore;

			m_queryStore = queryStore;
		}

		void AddUserQueries()
		{
			var queries = MainClass.AppKeyFile.GetStringListOrNull("ui", "queries");
			if (queries != null)
			{
				foreach (var query in queries)
				{
					if (m_queryStore.Contains(i => (string)m_queryStore.GetValue(i, 0) == query))
						continue;

					m_queryStore.AppendValues(query, -1, -1);
				}
			}
		}

		void AddTagQueries()
		{
			using (var cdb = new CachedDB())
			{
				var db = cdb.Database;

				foreach (var tag in db.GetAllTags())
				{
					var queryStr = String.Format("tag:{0}", tag);

					if (m_queryStore.Contains(i => (string)m_queryStore.GetValue(i, 0) == queryStr))
						continue;

					m_queryStore.AppendValues(String.Format("tag:{0}", tag), -1, -1);
				}
			}
		}

		void OnQueryTreeviewSelectionChanged(object sender, EventArgs e)
		{
			TreeSelection selection = (TreeSelection)sender;
			ITreeModel model;
			TreeIter iter;

			string queryString = "";

			if (selection.GetSelected(out model, out iter))
				queryString = (string)model.GetValue(iter, 0);

			if (this.QuerySelected != null)
				this.QuerySelected(queryString);
		}
	}
}
