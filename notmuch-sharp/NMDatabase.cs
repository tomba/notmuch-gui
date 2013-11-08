using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public class NMDatabase : DisposableBase
	{
		public static NMDatabase Create(string path)
		{
			IntPtr p;

			Status r = Native.notmuch_database_create(path, out p);

			Console.WriteLine("db_create: {0}, {1}", r, p);

			if (r != Status.SUCCESS)
				throw new Exception("fail");

			var db = new NMDatabase(p);

			return db;
		}

		public static NMDatabase Open(string path, DatabaseMode mode)
		{
			IntPtr p;

			Status r = Native.notmuch_database_open(path, mode, out p);

			Console.WriteLine("db_open: {0}, {1}", r, p);

			if (r != Status.SUCCESS)
				throw new Exception("fail");

			var db = new NMDatabase(p);

			return db;
		}

		public NMMessage? FindMessage(string messageId)
		{
			IntPtr msgPtr;

			Status r= Native.notmuch_database_find_message(this.Handle, messageId, out msgPtr);
			if (r != Status.SUCCESS)
				return null;

			return new NMMessage(msgPtr);
		}

		protected override void DestroyHandle()
		{
			Native.notmuch_database_destroy(this.Handle);
		}

		NMDatabase(IntPtr ptr)
			: base(ptr)
		{
		}

		public string Path
		{
			get
			{
				IntPtr p = Native.notmuch_database_get_path(this.Handle);
				return Marshal.PtrToStringAnsi(p);
			}
		}
	}
}
