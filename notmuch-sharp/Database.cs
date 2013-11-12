using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public class Database : DisposableBase
	{
		public static Database Create(string path)
		{
			IntPtr p;

			Status r = notmuch_database_create(path, out p);

			if (r != Status.SUCCESS)
				throw new Exception("fail");

			var db = new Database(p);

			return db;
		}

		public static Database Open(string path, DatabaseMode mode, out Status status)
		{
			IntPtr p;

			status = notmuch_database_open(path, mode, out p);

			if (status != Status.SUCCESS)
				return null;

			return new Database(p);
		}

		public Message? FindMessage(string messageId)
		{
			IntPtr msgPtr;

			Status r = notmuch_database_find_message(this.Handle, messageId, out msgPtr);
			if (r != Status.SUCCESS)
				return null;

			return new Message(msgPtr);
		}

		protected override void DestroyHandle()
		{
			notmuch_database_destroy(this.Handle);
		}

		Database(IntPtr ptr)
			: base(ptr)
		{
		}

		public string Path
		{
			get
			{
				IntPtr p = notmuch_database_get_path(this.Handle);
				return Marshal.PtrToStringAnsi(p);
			}
		}

		public Tags AllTags
		{
			get
			{
				IntPtr p = notmuch_database_get_all_tags(this.Handle);
				return new Tags(p);
			}
		}

		[DllImport("libnotmuch")]
		static extern Status notmuch_database_create(string path, out IntPtr db);

		[DllImport("libnotmuch")]
		static extern Status notmuch_database_open(string path, DatabaseMode mode, out IntPtr db);

		[DllImport("libnotmuch")]
		static extern void notmuch_database_destroy(IntPtr db);

		[DllImport("libnotmuch")]
		static extern void notmuch_database_close(IntPtr db);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_database_get_path(IntPtr db);

		[DllImport("libnotmuch")]
		static extern Status notmuch_database_find_message(IntPtr db, string messageId, out IntPtr msg);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_database_get_all_tags(IntPtr db);
	}
}
