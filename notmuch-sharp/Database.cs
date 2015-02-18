using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Generic;

namespace NotMuch
{
	public sealed class Database : IDisposable
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
			//Console.WriteLine("DB({0:X})", (ulong)m_handle);
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
			//Console.WriteLine("~DB({0:X}, {1})", (ulong)m_handle, disposing);
			Debug.Assert(m_handle != IntPtr.Zero);

			notmuch_database_destroy(m_handle);
			m_handle = IntPtr.Zero;
		}

		public IDisposable BeginAtomic()
		{
			return new AtomicHolder(m_handle);
		}

		public Message FindMessage(string messageId)
		{
			if (messageId == null)
				throw new ArgumentNullException("messageId");

			IntPtr msgPtr;

			Debug.Assert(m_handle != IntPtr.Zero);

			Status r = notmuch_database_find_message(m_handle, messageId, out msgPtr);
			if (r != Status.SUCCESS)
				return Message.NullMessage;

			return new Message(msgPtr);
		}

		public Message GetMessage(string messageId)
		{
			if (messageId == null)
				throw new ArgumentNullException("messageId");

			var msg = FindMessage(messageId);

			if (msg.IsNull)
				throw new Exception();

			return msg;
		}

		public string Path
		{
			get
			{
				Debug.Assert(m_handle != IntPtr.Zero);

				IntPtr p = notmuch_database_get_path(m_handle);
				return Marshal.PtrToStringAnsi(p);
			}
		}

		public IEnumerable<string> GetAllTags()
		{
			Debug.Assert(m_handle != IntPtr.Zero);

			IntPtr hTags = notmuch_database_get_all_tags(m_handle);

			while (Tags.Valid(hTags))
			{
				var str = Tags.Get(hTags);
				str = string.Intern(str);
				yield return str;
				Tags.MoveToNext(hTags);
			}
		}

		public Query CreateQuery(string queryString)
		{
			if (queryString == null)
				throw new ArgumentNullException("queryString");

			Debug.Assert(m_handle != IntPtr.Zero);

			var query = new Query(notmuch_query_create(m_handle, queryString));

			return query;
		}

		class AtomicHolder : IDisposable
		{
			IntPtr m_handle;

			public AtomicHolder(IntPtr dbHandle)
			{
				m_handle = dbHandle;

				var status = notmuch_database_begin_atomic(m_handle);

				if (status != Status.SUCCESS)
					throw new Exception("begin atomic failed");
			}

			~AtomicHolder()
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
				var status = notmuch_database_end_atomic(m_handle);

				if (status != Status.SUCCESS)
					Console.WriteLine("end atomic failed");
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
		static extern Status notmuch_database_begin_atomic(IntPtr db);

		[DllImport("libnotmuch")]
		static extern Status notmuch_database_end_atomic(IntPtr db);

		[DllImport("libnotmuch")]
		static extern Status notmuch_database_find_message(IntPtr db, string messageId, out IntPtr msg);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_database_get_all_tags(IntPtr db);

		[DllImport("libnotmuch")]
		static extern IntPtr notmuch_query_create(IntPtr db, string queryString);
	}
}
