using System;
using System.Runtime.InteropServices;

namespace NotMuch
{
	public class Database : IDisposable
	{
		public static Database Create(string path, out Status status)
		{
			IntPtr p;

			status = notmuch_database_create(path, out p);

			if (status != Status.SUCCESS)
				return null;

			return new Database(p);
		}

		public static Database Open(string path, DatabaseMode mode, out Status status)
		{
			IntPtr p;

			status = notmuch_database_open(path, mode, out p);

			if (status != Status.SUCCESS)
				return null;

			return new Database(p);
		}

		IntPtr m_handle;

		Database(IntPtr ptr)
		{
			m_handle = ptr;
		}

		~Database ()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (m_handle != IntPtr.Zero)
			{
				notmuch_database_destroy(m_handle);
				m_handle = IntPtr.Zero;
			}
		}

		public Message FindMessage(string messageId)
		{
			IntPtr msgPtr;

			Status r = notmuch_database_find_message(m_handle, messageId, out msgPtr);
			if (r != Status.SUCCESS)
				return Message.NullMessage;

			return new Message(msgPtr);
		}

		public string Path
		{
			get
			{
				IntPtr p = notmuch_database_get_path(m_handle);
				return Marshal.PtrToStringAnsi(p);
			}
		}

		public Tags AllTags
		{
			get
			{
				IntPtr p = notmuch_database_get_all_tags(m_handle);
				return new Tags(p);
			}
		}

		public Query CreateQuery(string queryString)
		{
			IntPtr query = notmuch_query_create(m_handle, queryString);

			return new Query(query);
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

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_query_create(IntPtr db, string queryString);
	}
}
