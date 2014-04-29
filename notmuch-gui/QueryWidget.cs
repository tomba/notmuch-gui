using System;
using Gtk;
using System.Linq;

namespace NotMuchGUI
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class QueryWidget : Gtk.Bin
	{
		ListStore m_queryStore;
		QueryCountUpdater m_queryCountUpdater;

		public event Action<string> QuerySelected;

		TreeView queryTreeview;

		public QueryWidget()
		{
			SetupQueryList();


			using (var cdb = new CachedDB())
			{
				var db = cdb.Database;

				foreach (var tag in db.GetAllTags())
					m_queryStore.AppendValues(String.Format("tag:{0}", tag), 0, 0);
			}



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

			var uiTags = MainClass.AppKeyFile.GetStringListOrNull("ui", "queries");
			if (uiTags != null)
			{
				foreach (var tag in uiTags)
					queryStore.AppendValues(tag, 0, 0);
			}

			m_queryStore = queryStore;
		}

		protected void OnQueryTreeviewCursorChanged(object sender, EventArgs e)
		{
			TreeSelection selection = (sender as TreeView).Selection;
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

