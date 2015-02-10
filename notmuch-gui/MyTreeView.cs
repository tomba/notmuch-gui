using System;
using Gtk;

namespace NotMuchGUI
{
	class MyTreeView : TreeView
	{
		protected override bool OnKeyPressEvent(Gdk.EventKey evnt)
		{
			if (evnt.Key == Gdk.Key.Right)
			{
				TreePath path;
				TreeViewColumn column;

				GetCursor(out path, out column);

				if (path == null)
					return false;

				ExpandRow(path, true);

				return true;
			}
			else if (evnt.Key == Gdk.Key.Left)
			{
				TreePath path;
				TreeViewColumn column;

				GetCursor(out path, out column);

				if (path == null)
					return false;

				CollapseRow(path);

				return true;
			}

			return base.OnKeyPressEvent(evnt);
		}
	}
}

