using System;
using System.Reflection;
using Gtk;
using System.Collections;
using System.Runtime.InteropServices;

namespace Viewer
{
	public class MyTreeModel : GLib.Object, TreeModelImplementor
	{
		Assembly[] assemblies;

		public MyTreeModel()
		{
			assemblies = AppDomain.CurrentDomain.GetAssemblies();
		}

		object GetNodeAtPath(TreePath path)
		{
			if (path.Indices.Length > 0)
			{
				Assembly assm = assemblies[path.Indices[0]];
				if (path.Indices.Length > 1)
				{
					Type t = assm.GetTypes()[path.Indices[1]];
					if (path.Indices.Length > 2)
						return t.GetMembers()[path.Indices[2]];
					else
						return t;
				}
				else
					return assm;
			}
			else
				return null;
		}

		Hashtable node_hash = new Hashtable();

		public TreeModelFlags Flags
		{
			get
			{
				return TreeModelFlags.ItersPersist;
			}
		}

		public int NColumns
		{
			get
			{
				return 2;
			}
		}

		public GLib.GType GetColumnType(int col)
		{
			GLib.GType result = GLib.GType.String;
			return result;
		}

		TreeIter IterFromNode(object node)
		{
			GCHandle gch;
			if (node_hash[node] != null)
				gch = (GCHandle)node_hash[node];
			else
				gch = GCHandle.Alloc(node);
			TreeIter result = TreeIter.Zero;
			result.UserData = (IntPtr)gch;
			return result;
		}

		object NodeFromIter(TreeIter iter)
		{
			GCHandle gch = (GCHandle)iter.UserData;
			return gch.Target;
		}

		TreePath PathFromNode(object node)
		{
			if (node == null)
				return new TreePath();

			object work = node;
			TreePath path = new TreePath();

			if (work is MemberInfo)
			{
				Type parent = (work as MemberInfo).ReflectedType;
				path.PrependIndex(Array.IndexOf(parent.GetMembers(), work));
				work = parent;
			}

			if (work is Type)
			{
				Assembly assm = (work as Type).Assembly;
				path.PrependIndex(Array.IndexOf(assm.GetTypes(), work));
				work = assm;
			}

			if (work is Assembly)
				path.PrependIndex(Array.IndexOf(assemblies, node));

			return path;
		}

		public bool GetIter(out TreeIter iter, TreePath path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			iter = TreeIter.Zero;

			object node = GetNodeAtPath(path);
			if (node == null)
				return false;

			iter = IterFromNode(node);
			return true;
		}

		public TreePath GetPath(TreeIter iter)
		{
			object node = NodeFromIter(iter);
			if (node == null)
				throw new ArgumentException("iter");

			return PathFromNode(node);
		}

		int cou = 0;

		public void GetValue(TreeIter iter, int col, ref GLib.Value val)
		{
			object node = NodeFromIter(iter);
			if (node == null)
				return;

			if (node is Assembly)
				val = new GLib.Value(col == 0 ? (node as Assembly).GetName().Name : "Assembly");
			else if (node is Type)
				val = new GLib.Value(col == 0 ? (node as Type).Name : "Type");
			else
				val = new GLib.Value(col == 0 ? (node as MemberInfo).Name : "Member");

			Console.WriteLine("getval {0} {1} {2}", cou++, col, node);
		}

		public bool IterNext(ref TreeIter iter)
		{
			object node = NodeFromIter(iter);
			if (node == null)
				return false;

			int idx;
			if (node is Assembly)
			{
				idx = Array.IndexOf(assemblies, node) + 1;
				if (idx < assemblies.Length)
				{
					iter = IterFromNode(assemblies[idx]);
					return true;
				}
			}
			else if (node is Type)
			{
				Type[] siblings = (node as Type).Assembly.GetTypes();
				idx = Array.IndexOf(siblings, node) + 1;
				if (idx < siblings.Length)
				{
					iter = IterFromNode(siblings[idx]);
					return true;
				}
			}
			else
			{
				MemberInfo[] siblings = (node as MemberInfo).ReflectedType.GetMembers();
				idx = Array.IndexOf(siblings, node) + 1;
				if (idx < siblings.Length)
				{
					iter = IterFromNode(siblings[idx]);
					return true;
				}
			}
			return false;
		}

		int ChildCount(object node)
		{
			if (node is Assembly)
				return (node as Assembly).GetTypes().Length;
			else if (node is Type)
				return (node as Type).GetMembers().Length;
			else
				return 0;
		}

		public bool IterChildren(out TreeIter child, TreeIter parent)
		{
			child = TreeIter.Zero;

			if (parent.UserData == IntPtr.Zero)
			{
				child = IterFromNode(assemblies[0]);
				return true;
			}

			object node = NodeFromIter(parent);
			if (node == null || ChildCount(node) <= 0)
				return false;

			if (node is Assembly)
				child = IterFromNode((node as Assembly).GetTypes()[0]);
			else if (node is Type)
				child = IterFromNode((node as Type).GetMembers()[0]);
			return true;
		}

		public bool IterHasChild(TreeIter iter)
		{
			object node = NodeFromIter(iter);
			if (node == null || ChildCount(node) <= 0)
				return false;

			return true;
		}

		public int IterNChildren(TreeIter iter)
		{
			if (iter.UserData == IntPtr.Zero)
				return assemblies.Length;

			object node = NodeFromIter(iter);
			if (node == null)
				return 0;

			return ChildCount(node);
		}

		public bool IterNthChild(out TreeIter child, TreeIter parent, int n)
		{
			child = TreeIter.Zero;

			if (parent.UserData == IntPtr.Zero)
			{
				if (assemblies.Length <= n)
					return false;
				child = IterFromNode(assemblies[n]);
				return true;
			}

			object node = NodeFromIter(parent);
			if (node == null || ChildCount(node) <= n)
				return false;

			if (node is Assembly)
				child = IterFromNode((node as Assembly).GetTypes()[n]);
			else if (node is Type)
				child = IterFromNode((node as Type).GetMembers()[n]);
			return true;
		}

		public bool IterParent(out TreeIter parent, TreeIter child)
		{
			parent = TreeIter.Zero;
			object node = NodeFromIter(child);
			if (node == null || node is Assembly)
				return false;

			if (node is Type)
				parent = IterFromNode((node as Type).Assembly);
			else if (node is MemberInfo)
				parent = IterFromNode((node as MemberInfo).ReflectedType);
			return true;
		}

		public void RefNode(TreeIter iter)
		{
		}

		public void UnrefNode(TreeIter iter)
		{
		}
	}
}

