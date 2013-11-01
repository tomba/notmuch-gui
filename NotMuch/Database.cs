using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public class Database : Disposable
	{
		public static Database Create(string path)
		{
			IntPtr p;

			Status r = Native.notmuch_database_create(path, out p);

			Console.WriteLine("db_create: {0}, {1}", r, p);

			if (r != Status.SUCCESS)
				throw new Exception("fail");

			var db = new Database(p);

			return db;
		}

		public static Database Open(string path, DatabaseMode mode)
		{
			IntPtr p;

			Status r = Native.notmuch_database_open(path, mode, out p);

			Console.WriteLine("db_open: {0}, {1}", r, p);

			if (r != Status.SUCCESS)
				throw new Exception("fail");

			var db = new Database(p);

			return db;
		}

		protected override void DestroyHandle()
		{
			Native.notmuch_database_destroy(m_ptr);
		}
		
		Database(IntPtr ptr)
			: base(ptr)
		{
		}

		public string Path
		{
			get
			{
				IntPtr p = Native.notmuch_database_get_path(m_ptr);
				return Marshal.PtrToStringAnsi(p);
			}
		}
	}
}
