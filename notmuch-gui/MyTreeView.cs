using System;
using Gtk;

namespace NotMuchGUI
{
	class MyTreeView : TreeView
	{
		protected override bool OnExpandCollapseCursorRow(bool logical, bool expand, bool open_all)
		{
			if (!expand)
			{
				if (open_all)
				{
					/* collapse whole branch */
					MoveCursorToBranchRoot();
				}
				else
				{
					TreePath path;
					TreeViewColumn col;
					this.GetCursor(out path, out col);

					TreeIter iter;
					this.Model.GetIter(out iter, path);

					/* if the row is already collapsed, move to parent */
					if (!GetRowExpanded(path) || !this.Model.IterHasChild(iter))
					{
						if (this.Model.IterParent(out iter, iter))
						{
							path = this.Model.GetPath(iter);
							SetCursor(path, null, false);
						}
					}
				}
			}

			bool b = base.OnExpandCollapseCursorRow(logical, expand, open_all);

			/* gtk doesn't seem to scroll to the cursor when collapsing, so we need to do that here */
			ScrollToCursor();

			return b;
		}

		public void MoveCursorToBranchRoot()
		{
			TreePath path;
			TreeViewColumn col;

			this.GetCursor(out path, out col);

			path = new TreePath(new [] { path.Indices[0] });

			SetCursor(path, null, false);
		}

		public void ScrollToCursor()
		{
			TreePath path;
			TreeViewColumn col;
			this.GetCursor(out path, out col);

			ScrollToCell(path, col, false, 0, 0);
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

