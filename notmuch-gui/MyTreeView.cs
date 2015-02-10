using System;
using Gtk;

namespace NotMuchGUI
{
	class MyTreeView : TreeView
	{
		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			bool shift = (evnt.State & Gdk.ModifierType.ShiftMask) != 0;

			if (evnt.Key == Gdk.Key.Right)
			{
				TreePath path;
				TreeViewColumn column;

				GetCursor(out path, out column);

				if (path == null)
					return false;

				ExpandRow(path, shift ? false : true);

				return true;
			}
			else if (evnt.Key == Gdk.Key.Left)
			{
				TreePath path;
				TreeViewColumn column;

				GetCursor(out path, out column);

				if (path == null)
					return false;

				TreeIter iter;
				this.Model.GetIter(out iter, path);

				if (!shift)
				{
					TreeIter parent;

					while (this.Model.IterParent(out parent, iter))
						iter = parent;

					path = this.Model.GetPath(iter);
					SetCursor(path, null, false);
				}
				else if (!GetRowExpanded(path) || !this.Model.IterHasChild(iter))
				{
					if (this.Model.IterParent(out iter, iter))
					{
						path = this.Model.GetPath(iter);
						SetCursor(path, null, false);
					}
				}

				CollapseRow(path);

				ScrollToCell(path, null, false, 0, 0);

				return true;
			}

			return base.OnKeyPressEvent(evnt);
		}

		public void ScrollToMostRecent()
		{
			var model = this.Model;

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

			ExpandToPath(path);

			Selection.UnselectAll();
			Selection.SelectPath(path);

			ScrollToCell(path, null, false, 0, 0);
		}
	}
}

