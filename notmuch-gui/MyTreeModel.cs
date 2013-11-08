using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using Gtk;
using System.Collections.Generic;
using NM = NotMuch;

namespace NotMuchGUI
{
	public class MyTreeModel : GLib.Object, TreeModelImplementor
	{
		int m_count;
		List<NM.Message> m_msgs;
		NM.Query m_query;

		public MyTreeModel(NM.Query query)
		{
			m_query = query;
			m_count = query.Count;
			m_msgs = new List<NM.Message>(m_count);
		}

		public int Count { get { return m_count; } }

		~MyTreeModel()
		{
			m_query.Dispose();
		}

		public void Append(NM.Message msg)
		{
			int idx = m_msgs.Count;

			m_msgs.Add(msg);
			/*
			TreeModelAdapter adapter = new TreeModelAdapter(this);

			var iter = TreeIter.Zero;
			iter.UserData = (IntPtr)idx;

			var path = new TreePath(new [] { idx });

			adapter.EmitRowChanged(path, iter);
			*/
		}

		public TreeModelFlags Flags
		{
			get
			{
				return TreeModelFlags.ListOnly;
			}
		}

		public int NColumns
		{
			get
			{
				return 3;
			}
		}

		public GLib.GType GetColumnType(int col)
		{
			GLib.GType result = GLib.GType.String;
			return result;
		}

		public bool GetIter(out TreeIter iter, TreePath path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			iter = TreeIter.Zero;

			if (path.Depth == 0)
				return false;

			int idx = path.Indices[0];

			iter.UserData = (IntPtr)idx;

			return true;
		}

		public TreePath GetPath(TreeIter iter)
		{
			int idx = (int)iter.UserData;

			return new TreePath(new int[] { idx });
		}

		public NM.Message? GetMessage(TreeIter iter)
		{
			int idx = (int)iter.UserData;

			if (idx >= m_msgs.Count)
				return null;

			return m_msgs[idx];
		}

		public void GetValue(TreeIter iter, int col, ref GLib.Value val)
		{
			int idx = (int)iter.UserData;

			string str;

			if (idx >= m_msgs.Count)
			{
				str = "null";
			}
			else
			{
				var msg = m_msgs[idx];

				switch (col)
				{
					case 0:
						str = msg.GetHeader("From");
						break;

					case 1:
						str = msg.GetHeader("Subject");
						break;

					case 2:
						var date = msg.Date.ToLocalTime();
						str = date.ToString("g");
						break;

					default:
						throw new Exception();
				}
			}

			val = new GLib.Value(str);

			//Console.WriteLine("getval {0} {1} {2}", idx, col, val);
		}

		public bool IterNext(ref TreeIter iter)
		{
			int idx = (int)iter.UserData;

			idx++;

			if (idx >= m_count)
				return false;

			iter.UserData = (IntPtr)idx;

			return true;
		}

		int ChildCount(object node)
		{
			return 0;
		}

		public bool IterChildren(out TreeIter child, TreeIter parent)
		{
			child = TreeIter.Zero;
			return false;
		}

		public bool IterHasChild(TreeIter iter)
		{
			return false;
		}

		public int IterNChildren(TreeIter iter)
		{
			return 0;
		}

		public bool IterNthChild(out TreeIter child, TreeIter parent, int n)
		{
			child = TreeIter.Zero;
			return false;
		}

		public bool IterParent(out TreeIter parent, TreeIter child)
		{
			parent = TreeIter.Zero;
			return false;
		}

		public void RefNode(TreeIter iter)
		{
		}

		public void UnrefNode(TreeIter iter)
		{
		}
	}
}

