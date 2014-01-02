using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using Gtk;
using System.Collections.Generic;
using NM = NotMuch;
using System.Linq;

namespace NotMuchGUI
{
	public class MessagesTreeModel : GLib.Object, TreeModelImplementor
	{
		struct Entry
		{
			public string ID;
			public int Depth;
		}

		int m_count;
		List<Entry> m_entries;

		public MessagesTreeModel()
		{
			m_count = 0;
			m_entries = new List<Entry>();
		}

		public MessagesTreeModel(int estimatedCount)
		{
			m_count = estimatedCount;
			m_entries = new List<Entry>(m_count);
		}

		public int Count { get { return m_count; } }

		public void Append(string msgID, int depth)
		{
			int idx = m_entries.Count;

			var entry = new Entry()
			{
				ID = msgID,
				Depth = depth,
			};

			m_entries.Add(entry);
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

		public int NColumns { get { return (int)MessageListColumns.NUM_COLUMNS; } }

		public GLib.GType GetColumnType(int col)
		{
			switch ((MessageListColumns)col)
			{
				case MessageListColumns.Depth:
				case MessageListColumns.MsgNum:
				case MessageListColumns.ThreadNum:
					return GLib.GType.Int;

				case MessageListColumns.Flags:
					return GLib.GType.Int;

				case MessageListColumns.Date:
					return GLib.GType.Int64;

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

		public string GetMessageID(TreeIter iter)
		{
			int idx = (int)iter.UserData;

			if (idx >= m_entries.Count)
				return null;

			return m_entries[idx].ID;
		}

		GLib.Value GetNullValue(int col)
		{
			switch ((MessageListColumns)col)
			{
				case MessageListColumns.From:
				case MessageListColumns.Subject:
				case MessageListColumns.Tags:
					return new GLib.Value("null");

				case MessageListColumns.Date:
					return new GLib.Value((long)0);

				case MessageListColumns.Flags:
					return new GLib.Value(0);

				case MessageListColumns.Depth:
				case MessageListColumns.ThreadNum:
					return new GLib.Value(0);

				default:
					throw new Exception();
			}
		}

		public void GetValue(TreeIter iter, int col, ref GLib.Value val)
		{
			int idx = (int)iter.UserData;

			if (col == (int)MessageListColumns.MsgNum)
			{
				val = new GLib.Value(idx);
				return;
			}

			string str;

			if (idx >= m_entries.Count)
			{
				val = GetNullValue(col);
				return;
			}

			var entry = m_entries[idx];

			using (var cdb = new CachedDB())
			{
				var db = cdb.Database;

				var msg = db.FindMessage(entry.ID);

				if (msg.IsNull)
				{
					val = GetNullValue(col);
					return;
				}

				switch ((MessageListColumns)col)
				{
					case MessageListColumns.MessageId:
						val = new GLib.Value(m_entries[idx].ID);
						break;

					case MessageListColumns.From:
						{
							str = msg.From;
							//str = entry.From;
							val = new GLib.Value(str);
						}
						break;

					case MessageListColumns.Subject:
						{
							str = msg.Subject;

							str = new String('→', entry.Depth) + str;

							val = new GLib.Value(str);
						}
						break;

					case MessageListColumns.Date:
						{
							var date = msg.DateStamp;
							val = new GLib.Value(date);
						}
						break;

					case MessageListColumns.Tags:
						{
							var list = msg.GetTags().ToList();

							str = string.Join("/", list);

							val = new GLib.Value(str);
						}
						break;

					case MessageListColumns.Flags:
						{
							MessageListFlags flags = MessageListFlags.Match;

							if (msg.GetTags().Contains("unread"))
								flags |= MessageListFlags.Unread;

							val = new GLib.Value((int)flags);
						}
						break;

					case MessageListColumns.Depth:
						val = new GLib.Value(entry.Depth);
						break;

					case MessageListColumns.ThreadNum:
						val = new GLib.Value(0);
						break;

					default:
						throw new Exception();
				}
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

