using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public class Database : DisposableBase
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

		public Message? FindMessage(string messageId)
		{
			IntPtr msgPtr;

			Status r= Native.notmuch_database_find_message(this.Handle, messageId, out msgPtr);
			if (r != Status.SUCCESS)
				return null;

			return new Message(msgPtr);
		}

		protected override void DestroyHandle()
		{
			Native.notmuch_database_destroy(this.Handle);
		}

		Database(IntPtr ptr)
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

		public Tags AllTags
		{
			get
			{
				IntPtr p = Native.notmuch_database_get_all_tags(this.Handle);
				return new Tags(p);
			}
		}
	}
}
