using System;
using System.Collections.Generic;
using Gtk;

namespace NotMuchGUI
{
	public static class Extensions
	{
		public static IEnumerable<object[]> AsEnumerable(this ListStore store)
		{
			foreach (object[] values in store)
				yield return values;
		}

		public static TreeIter Find(this ListStore store, Func<TreeIter, bool> f)
		{
			TreeIter iter;

			if (!store.GetIterFirst(out iter))
				return TreeIter.Zero;

			do
			{
				if (f(iter))
					return iter;
			}
			while (store.IterNext(ref iter));

			return TreeIter.Zero;
		}

		public static bool Contains(this ListStore store, Func<TreeIter, bool> f)
		{
			TreeIter iter;

			if (!store.GetIterFirst(out iter))
				return false;

			do
			{
				if (f(iter))
					return true;
			}
			while (store.IterNext(ref iter));

			return false;
		}
	}
}

