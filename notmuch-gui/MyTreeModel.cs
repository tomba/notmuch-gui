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
		struct Entry
		{
			public NM.Message Msg;
			public int Depth;
		}

		public const int COL_FROM = 0;
		public const int COL_SUBJECT = 1;
		public const int COL_DATE = 2;
		public const int COL_TAGS = 3;
		public const int COL_UNREAD = 4;
		public const int COL_DEPTH = 5;
		public const int COL_MSG_NUM = 6;
		public const int COL_NUM_COLUMNS = 7;
		int m_count;
		List<Entry> m_entries;
		NM.Query m_query;

		public MyTreeModel()
		{
			m_query = null;
			m_count = 0;
			m_entries = new List<Entry>();
		}

		public MyTreeModel(NM.Query query)
		{
			m_query = query;
			m_count = query.Count;
			m_entries = new List<Entry>(m_count);
		}

		public int Count { get { return m_count; } }

		~MyTreeModel()
		{
			if (m_query != null)
			{
				m_query.Dispose();
				m_query = null;
			}
		}

		public void Append(NM.Message msg, int depth)
		{
			int idx = m_entries.Count;

			var entry = new Entry()
			{
				Msg = msg,
				Depth = depth,
			};

			m_entries.Add(entry);
			/*
			TreeModelAdapter adapter = new TreeModelAdapter(this);

			var iter = TreeIter.Zero;
			iter.UserData = (IntPtr)idx;

			var path = new TreePath(new [] { idx });

			adapter.EmitRowChanged(path, iter);
			*/
		}

		public void FinishAdding()
		{
			if (m_entries.Count == m_count)
				return;

			Console.WriteLine("ADJUST {0} -> {1}", m_count, m_entries.Count);

			TreeModelAdapter adapter = new TreeModelAdapter(this);

			var arr = new int[1];

			if (m_count < m_entries.Count)
			{
				for (int i = m_count; i < m_entries.Count; ++i)
				{
					var iter = TreeIter.Zero;
					iter.UserData = (IntPtr)i;

					arr[0] = i;

					using (var path = new TreePath(arr))
						adapter.EmitRowInserted(path, iter);
				}
			}
			else
			{
				for (int i = m_count; i >= m_entries.Count; --i)
				{
					arr[0] = i;

					using (var path = new TreePath(arr))
						adapter.EmitRowDeleted(path);
				}
			}

			m_count = m_entries.Count;
		}

		public TreeModelFlags Flags { get { return TreeModelFlags.ListOnly; } }

		public int NColumns { get { return COL_NUM_COLUMNS; } }

		public GLib.GType GetColumnType(int col)
		{
			switch (col)
			{
				case COL_DEPTH:
				case COL_MSG_NUM:
					return GLib.GType.Int;

				case COL_UNREAD:
					return GLib.GType.Boolean;

				default:
					return GLib.GType.String;
			}
		}

		public bool GetIter(out TreeIter iter, TreePath path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			iter = TreeIter.Zero;

			if (path.Depth == 0)
				return false;

			if (m_count == 0)
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

		public NM.Message GetMessage(TreeIter iter)
		{
			int idx = (int)iter.UserData;

			if (idx >= m_entries.Count)
				return NM.Message.NullMessage;

			return m_entries[idx].Msg;
		}

		public void GetValue(TreeIter iter, int col, ref GLib.Value val)
		{
			int idx = (int)iter.UserData;

			if (col == COL_MSG_NUM)
			{
				val = new GLib.Value(idx);
				return;
			}

			string str;

			if (idx >= m_entries.Count)
			{
				switch (col)
				{
					case COL_FROM:
					case COL_SUBJECT:
					case COL_DATE:
					case COL_TAGS:
						val = new GLib.Value("null");
						break;

					case COL_UNREAD:
						val = new GLib.Value(false);
						break;

					case COL_DEPTH:
						val = new GLib.Value(0);
						break;
				}

				return;
			}

			var entry = m_entries[idx];

			var msg = entry.Msg;

			switch (col)
			{
				case COL_FROM:
					{
						str = msg.GetHeader("From");
						val = new GLib.Value(str);
					}
					break;

				case COL_SUBJECT:
					{
						str = msg.GetHeader("Subject");

						str = new String('→', entry.Depth) + str;

						val = new GLib.Value(str);
					}
					break;

				case COL_DATE:
					{
						var date = msg.Date.ToLocalTime();
						str = date.ToString("g");
						val = new GLib.Value(str);
					}
					break;

				case COL_TAGS:
					{
						var tags = msg.GetTags();

						List<string> list = new List<string>();

						while (tags.Valid)
						{
							list.Add(tags.Current);
							tags.Next();
						}

						str = string.Join("/", list);

						val = new GLib.Value(str);
					}
					break;

				case COL_UNREAD:
					{
						var tags = msg.GetTags();

						val = new GLib.Value(false);

						while (tags.Valid)
						{
							if (tags.Current == "unread")
							{
								val = new GLib.Value(true);
								break;
							}

							tags.Next();
						}
					}
					break;

				case COL_DEPTH:
					val = new GLib.Value(entry.Depth);
					break;

				default:
					throw new Exception();
			}

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

